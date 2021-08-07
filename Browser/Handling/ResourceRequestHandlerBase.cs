﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using CefSharp;
using TweetDuck.Utils;
using TweetLib.Core.Utils;

namespace TweetDuck.Browser.Handling {
	class ResourceRequestHandlerBase : ResourceRequestHandler {
		private static readonly Regex TweetDeckResourceUrl = new Regex(@"/dist/(.*?)\.(.*?)\.(css|js)$");
		private static readonly SortedList<string, string> TweetDeckHashes = new SortedList<string, string>(4);

		public static void LoadResourceRewriteRules(string rules) {
			if (string.IsNullOrEmpty(rules)) {
				return;
			}

			TweetDeckHashes.Clear();

			foreach (string rule in rules.Replace(" ", "").ToLower().Split(',')) {
				var (key, hash) = StringUtils.SplitInTwo(rule, '=') ?? throw new ArgumentException("A rule must have one '=' character: " + rule);

				if (hash.All(chr => char.IsDigit(chr) || (chr >= 'a' && chr <= 'f'))) {
					TweetDeckHashes.Add(key, hash);
				}
				else {
					throw new ArgumentException("Invalid hash characters: " + rule);
				}
			}
		}

		protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback) {
			if (request.ResourceType == ResourceType.CspReport) {
				callback.Dispose();
				return CefReturnValue.Cancel;
			}

			if (BrowserUtils.HasDevTools) {
				NameValueCollection headers = request.Headers;
				headers.Remove("x-devtools-emulate-network-conditions-client-id");
				request.Headers = headers;
			}

			return base.OnBeforeResourceLoad(browserControl, browser, frame, request, callback);
		}

		protected override bool OnResourceResponse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response) {
			if ((request.ResourceType == ResourceType.Script || request.ResourceType == ResourceType.Stylesheet) && TweetDeckHashes.Count > 0) {
				string url = request.Url;
				Match match = TweetDeckResourceUrl.Match(url);

				if (match.Success && TweetDeckHashes.TryGetValue($"{match.Groups[1]}.{match.Groups[3]}", out string hash)) {
					if (match.Groups[2].Value == hash) {
						Program.Reporter.LogVerbose("[RequestHandlerBase] Accepting " + url);
					}
					else {
						Program.Reporter.LogVerbose("[RequestHandlerBase] Replacing " + url + " hash with " + hash);
						request.Url = TweetDeckResourceUrl.Replace(url, $"/dist/$1.{hash}.$3");
						return true;
					}
				}
			}

			return base.OnResourceResponse(browserControl, browser, frame, request, response);
		}
	}
}

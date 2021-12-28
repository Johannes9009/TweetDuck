﻿using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using TweetDuck.Browser.Data;
using TweetDuck.Dialogs;
using TweetDuck.Management;
using TweetLib.Core;
using TweetLib.Core.Features.Twitter;
using TweetLib.Utils.Static;
using Cookie = CefSharp.Cookie;

namespace TweetDuck.Utils {
	static class TwitterUtils {
		public static readonly Color BackgroundColor = Color.FromArgb(28, 99, 153);
		public const string BackgroundColorOverride = "setTimeout(function f(){let h=document.head;if(!h){setTimeout(f,5);return;}let e=document.createElement('style');e.innerHTML='body,body::before{background:#1c6399!important;margin:0}';h.appendChild(e);},1)";

		public static readonly ResourceLink LoadingSpinner = new ResourceLink("https://ton.twimg.com/tduck/spinner", ResourceHandlers.ForBytes(Properties.Resources.spinner, "image/apng"));

		public static readonly string[] DictionaryWords = {
			"tweetdeck", "TweetDeck", "tweetduck", "TweetDuck", "TD"
		};

		private static void DownloadTempImage(string url, ImageQuality quality, Action<string> process) {
			string file = Path.Combine(BrowserCache.CacheFolder, TwitterUrls.GetImageFileName(url) ?? Path.GetRandomFileName());

			if (FileUtils.FileExistsAndNotEmpty(file)) {
				process(file);
			}
			else {
				DownloadFileAuth(TwitterUrls.GetMediaLink(url, quality), file, () => {
					process(file);
				}, ex => {
					FormMessage.Error("Image Download", "An error occurred while downloading the image: " + ex.Message, FormMessage.OK);
				});
			}
		}

		public static void ViewImage(string url, ImageQuality quality) {
			DownloadTempImage(url, quality, path => {
				string ext = Path.GetExtension(path);

				if (ImageUrl.ValidExtensions.Contains(ext)) {
					App.SystemHandler.OpenAssociatedProgram(path);
				}
				else {
					FormMessage.Error("Image Download", "Invalid file extension " + ext, FormMessage.OK);
				}
			});
		}

		public static void CopyImage(string url, ImageQuality quality) {
			DownloadTempImage(url, quality, path => {
				Image image;

				try {
					image = Image.FromFile(path);
				} catch (Exception ex) {
					FormMessage.Error("Copy Image", "An error occurred while copying the image: " + ex.Message, FormMessage.OK);
					return;
				}

				ClipboardManager.SetImage(image);
			});
		}

		public static void DownloadImage(string url, string username, ImageQuality quality) {
			DownloadImages(new string[] { url }, username, quality);
		}

		public static void DownloadImages(string[] urls, string username, ImageQuality quality) {
			if (urls.Length == 0) {
				return;
			}

			string firstImageLink = TwitterUrls.GetMediaLink(urls[0], quality);
			int qualityIndex = firstImageLink.IndexOf(':', firstImageLink.LastIndexOf('/'));

			string filename = TwitterUrls.GetImageFileName(firstImageLink);
			string ext = Path.GetExtension(filename); // includes dot

			using SaveFileDialog dialog = new SaveFileDialog {
				AutoUpgradeEnabled = true,
				OverwritePrompt = urls.Length == 1,
				Title = "Save Image",
				FileName = qualityIndex == -1 ? filename : $"{username} {Path.ChangeExtension(filename, null)} {firstImageLink.Substring(qualityIndex + 1)}".Trim() + ext,
				Filter = (urls.Length == 1 ? "Image" : "Images") + (string.IsNullOrEmpty(ext) ? " (unknown)|*.*" : $" (*{ext})|*{ext}")
			};

			if (dialog.ShowDialog() == DialogResult.OK) {
				static void OnFailure(Exception ex) {
					FormMessage.Error("Image Download", "An error occurred while downloading the image: " + ex.Message, FormMessage.OK);
				}

				if (urls.Length == 1) {
					DownloadFileAuth(firstImageLink, dialog.FileName, null, OnFailure);
				}
				else {
					string pathBase = Path.ChangeExtension(dialog.FileName, null);
					string pathExt = Path.GetExtension(dialog.FileName);

					for (int index = 0; index < urls.Length; index++) {
						DownloadFileAuth(TwitterUrls.GetMediaLink(urls[index], quality), $"{pathBase} {index + 1}{pathExt}", null, OnFailure);
					}
				}
			}
		}

		public static void DownloadVideo(string url, string username) {
			string filename = TwitterUrls.GetFileNameFromUrl(url);
			string ext = Path.GetExtension(filename);

			using SaveFileDialog dialog = new SaveFileDialog {
				AutoUpgradeEnabled = true,
				OverwritePrompt = true,
				Title = "Save Video",
				FileName = string.IsNullOrEmpty(username) ? filename : $"{username} {filename}".TrimStart(),
				Filter = "Video" + (string.IsNullOrEmpty(ext) ? " (unknown)|*.*" : $" (*{ext})|*{ext}")
			};

			if (dialog.ShowDialog() == DialogResult.OK) {
				DownloadFileAuth(url, dialog.FileName, null, ex => {
					FormMessage.Error("Video Download", "An error occurred while downloading the video: " + ex.Message, FormMessage.OK);
				});
			}
		}

		private static void DownloadFileAuth(string url, string target, Action onSuccess, Action<Exception> onFailure) {
			const string authCookieName = "auth_token";

			TaskScheduler scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			using ICookieManager cookies = Cef.GetGlobalCookieManager();

			cookies.VisitUrlCookiesAsync(url, true).ContinueWith(task => {
				string cookieStr = null;

				if (task.Status == TaskStatus.RanToCompletion) {
					Cookie found = task.Result?.Find(cookie => cookie.Name == authCookieName); // the list may be null

					if (found != null) {
						cookieStr = $"{found.Name}={found.Value}";
					}
				}

				WebClient client = WebUtils.NewClient(BrowserUtils.UserAgentChrome);
				client.Headers[HttpRequestHeader.Cookie] = cookieStr;
				client.DownloadFileCompleted += WebUtils.FileDownloadCallback(target, onSuccess, onFailure);
				client.DownloadFileAsync(new Uri(url), target);
			}, scheduler);
		}
	}
}

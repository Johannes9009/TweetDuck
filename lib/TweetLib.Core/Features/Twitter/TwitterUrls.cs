﻿using System;
using System.IO;
using System.Text.RegularExpressions;

namespace TweetLib.Core.Features.Twitter{
    public static class TwitterUrls{
        public const string TweetDeck = "https://tweetdeck.twitter.com";
        private const string TwitterTrackingUrl = "t.co";

        private static readonly Lazy<Regex> RegexAccountLazy = new Lazy<Regex>(() => new Regex(@"^https?://twitter\.com/(?!signup$|tos$|privacy$|search$|search-)([^/?]+)/?$", RegexOptions.Compiled), false);
        public static Regex RegexAccount => RegexAccountLazy.Value;

        public static bool IsTweetDeck(string url){
            return url.Contains("//tweetdeck.twitter.com/");
        }

        public static bool IsTwitter(string url){
            return url.Contains("//twitter.com/");
        }

        public static bool IsTwitterLogin2Factor(string url){
            return url.Contains("//twitter.com/account/login_verification");
        }

        public static string? GetFileNameFromUrl(string url){
            string file = Path.GetFileName(new Uri(url).AbsolutePath);
            return string.IsNullOrEmpty(file) ? null : file;
        }

        public static string GetMediaLink(string url, ImageQuality quality){
            return ImageUrl.TryParse(url, out var obj) ? obj.WithQuality(quality) : url;
        }

        public static string? GetImageFileName(string url){
            return GetFileNameFromUrl(ImageUrl.TryParse(url, out var obj) ? obj.WithNoQuality : url);
        }

        public enum UrlType{
            Invalid, Tracking, Fine
        }

        public static UrlType Check(string url){
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri)){
                string scheme = uri.Scheme;

                if (scheme == Uri.UriSchemeHttps || scheme == Uri.UriSchemeHttp || scheme == Uri.UriSchemeFtp || scheme == Uri.UriSchemeMailto){
                    return uri.Host == TwitterTrackingUrl ? UrlType.Tracking : UrlType.Fine;
                }
            }

            return UrlType.Invalid;
        }
    }
}

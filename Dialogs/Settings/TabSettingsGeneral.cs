﻿using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TweetDuck.Browser.Handling.General;
using TweetDuck.Controls;
using TweetDuck.Utils;
using TweetLib.Core.Systems.Updates;
using TweetLib.Core.Utils;

namespace TweetDuck.Dialogs.Settings {
	sealed partial class TabSettingsGeneral : FormSettings.BaseTab {
		private readonly Action reloadColumns;

		private readonly UpdateHandler updates;
		private int updateCheckEventId = -1;

		private readonly int browserListIndexDefault;
		private readonly int browserListIndexCustom;

		private readonly int videoPlayerListIndexDefault;
		private readonly int videoPlayerListIndexCustom;

		private readonly int searchEngineIndexDefault;
		private readonly int searchEngineIndexCustom;

		public TabSettingsGeneral(Action reloadColumns, UpdateHandler updates) {
			InitializeComponent();

			this.reloadColumns = reloadColumns;

			this.updates = updates;
			this.updates.CheckFinished += updates_CheckFinished;

			Disposed += (sender, args) => this.updates.CheckFinished -= updates_CheckFinished;

			// user interface

			toolTip.SetToolTip(checkExpandLinks, "Expands links inside the tweets. If disabled,\r\nthe full links show up in a tooltip instead.");
			toolTip.SetToolTip(checkFocusDmInput, "Places cursor into Direct Message input\r\nfield when opening a conversation.");
			toolTip.SetToolTip(checkOpenSearchInFirstColumn, "By default, TweetDeck adds Search columns at the end.\r\nThis option makes them appear before the first column instead.");
			toolTip.SetToolTip(checkKeepLikeFollowDialogsOpen, "Allows liking and following from multiple accounts at once,\r\ninstead of automatically closing the dialog after taking an action.");
			toolTip.SetToolTip(checkBestImageQuality, "When right-clicking a tweet image, the context menu options\r\nwill use links to the original image size (:orig in the URL).");
			toolTip.SetToolTip(checkAnimatedAvatars, "Some old Twitter avatars could be uploaded as animated GIFs.");
			toolTip.SetToolTip(labelZoomValue, "Changes the zoom level.\r\nAlso affects notifications and screenshots.");
			toolTip.SetToolTip(trackBarZoom, toolTip.GetToolTip(labelZoomValue));

			checkExpandLinks.Checked = Config.ExpandLinksOnHover;
			checkFocusDmInput.Checked = Config.FocusDmInput;
			checkOpenSearchInFirstColumn.Checked = Config.OpenSearchInFirstColumn;
			checkKeepLikeFollowDialogsOpen.Checked = Config.KeepLikeFollowDialogsOpen;
			checkBestImageQuality.Checked = Config.BestImageQuality;
			checkAnimatedAvatars.Checked = Config.EnableAnimatedImages;

			trackBarZoom.SetValueSafe(Config.ZoomLevel);
			labelZoomValue.Text = trackBarZoom.Value + "%";

			// updates

			toolTip.SetToolTip(checkUpdateNotifications, "Checks for updates every hour.\r\nIf an update is dismissed, it will not appear again.");
			toolTip.SetToolTip(btnCheckUpdates, "Forces an update check, even for updates that had been dismissed.");

			checkUpdateNotifications.Checked = Config.EnableUpdateCheck;

			// browser settings

			toolTip.SetToolTip(checkSmoothScrolling, "Toggles smooth mouse wheel scrolling.");
			toolTip.SetToolTip(checkTouchAdjustment, "Toggles Chromium touch screen adjustment.\r\nDisabled by default, because it is very imprecise with TweetDeck.");
			toolTip.SetToolTip(checkHardwareAcceleration, "Uses graphics card to improve performance.\r\nDisable if you experience visual glitches, or to save a small amount of RAM.");
			toolTip.SetToolTip(comboBoxCustomBrowser, "Sets the default browser for opening links.");
			toolTip.SetToolTip(comboBoxCustomVideoPlayer, "Sets the default application for playing videos.");
			toolTip.SetToolTip(comboBoxSearchEngine, "Sets the default website for opening searches.");

			checkSmoothScrolling.Checked = Config.EnableSmoothScrolling;
			checkTouchAdjustment.Checked = Config.EnableTouchAdjustment;
			checkHardwareAcceleration.Checked = SysConfig.HardwareAcceleration;

			foreach (WindowsUtils.Browser browserInfo in WindowsUtils.FindInstalledBrowsers()) {
				comboBoxCustomBrowser.Items.Add(browserInfo);
			}

			browserListIndexDefault = comboBoxCustomBrowser.Items.Add("(default browser)");
			browserListIndexCustom = comboBoxCustomBrowser.Items.Add("(custom program...)");
			UpdateBrowserPathSelection();

			videoPlayerListIndexDefault = comboBoxCustomVideoPlayer.Items.Add("(default TweetDuck player)");
			videoPlayerListIndexCustom = comboBoxCustomVideoPlayer.Items.Add("(custom program...)");
			UpdateVideoPlayerPathSelection();

			comboBoxSearchEngine.Items.Add(new SearchEngine("DuckDuckGo", "https://duckduckgo.com/?q="));
			comboBoxSearchEngine.Items.Add(new SearchEngine("Google", "https://www.google.com/search?q="));
			comboBoxSearchEngine.Items.Add(new SearchEngine("Bing", "https://www.bing.com/search?q="));
			comboBoxSearchEngine.Items.Add(new SearchEngine("Yahoo", "https://search.yahoo.com/search?p="));
			searchEngineIndexDefault = comboBoxSearchEngine.Items.Add("(no engine set)");
			searchEngineIndexCustom = comboBoxSearchEngine.Items.Add("(custom url...)");
			UpdateSearchEngineSelection();

			// locales

			toolTip.SetToolTip(checkSpellCheck, "Underlines words that are spelled incorrectly.");
			toolTip.SetToolTip(comboBoxSpellCheckLanguage, "Language used for spell check.");
			toolTip.SetToolTip(comboBoxTranslationTarget, "Language tweets are translated into.");
			toolTip.SetToolTip(comboBoxFirstDayOfWeek, "First day of week used in the date picker.");

			checkSpellCheck.Checked = Config.EnableSpellCheck;

			try {
				foreach (LocaleUtils.Item item in LocaleUtils.SpellCheckLanguages) {
					comboBoxSpellCheckLanguage.Items.Add(item);
				}
			} catch {
				comboBoxSpellCheckLanguage.Items.Add(new LocaleUtils.Item("en-US"));
			}

			comboBoxSpellCheckLanguage.SelectedItem = new LocaleUtils.Item(Config.SpellCheckLanguage);

			foreach (LocaleUtils.Item item in LocaleUtils.TweetDeckTranslationLocales) {
				comboBoxTranslationTarget.Items.Add(item);
			}

			comboBoxTranslationTarget.SelectedItem = new LocaleUtils.Item(Config.TranslationTarget);

			var daysOfWeek = comboBoxFirstDayOfWeek.Items;
			daysOfWeek.Add("(based on system locale)");
			daysOfWeek.Add(new DayOfWeekItem("Monday", DayOfWeek.Monday));
			daysOfWeek.Add(new DayOfWeekItem("Tuesday", DayOfWeek.Tuesday));
			daysOfWeek.Add(new DayOfWeekItem("Wednesday", DayOfWeek.Wednesday));
			daysOfWeek.Add(new DayOfWeekItem("Thursday", DayOfWeek.Thursday));
			daysOfWeek.Add(new DayOfWeekItem("Friday", DayOfWeek.Friday));
			daysOfWeek.Add(new DayOfWeekItem("Saturday", DayOfWeek.Saturday));
			daysOfWeek.Add(new DayOfWeekItem("Sunday", DayOfWeek.Sunday));
			comboBoxFirstDayOfWeek.SelectedItem = daysOfWeek.OfType<DayOfWeekItem>().FirstOrDefault(dow => dow.Id == Config.CalendarFirstDay) ?? daysOfWeek[0];
		}

		public override void OnReady() {
			checkExpandLinks.CheckedChanged += checkExpandLinks_CheckedChanged;
			checkFocusDmInput.CheckedChanged += checkFocusDmInput_CheckedChanged;
			checkOpenSearchInFirstColumn.CheckedChanged += checkOpenSearchInFirstColumn_CheckedChanged;
			checkKeepLikeFollowDialogsOpen.CheckedChanged += checkKeepLikeFollowDialogsOpen_CheckedChanged;
			checkBestImageQuality.CheckedChanged += checkBestImageQuality_CheckedChanged;
			checkAnimatedAvatars.CheckedChanged += checkAnimatedAvatars_CheckedChanged;
			trackBarZoom.ValueChanged += trackBarZoom_ValueChanged;

			checkUpdateNotifications.CheckedChanged += checkUpdateNotifications_CheckedChanged;
			btnCheckUpdates.Click += btnCheckUpdates_Click;

			checkSmoothScrolling.CheckedChanged += checkSmoothScrolling_CheckedChanged;
			checkTouchAdjustment.CheckedChanged += checkTouchAdjustment_CheckedChanged;
			checkHardwareAcceleration.CheckedChanged += checkHardwareAcceleration_CheckedChanged;
			comboBoxCustomBrowser.SelectedIndexChanged += comboBoxCustomBrowser_SelectedIndexChanged;
			btnCustomBrowserChange.Click += btnCustomBrowserChange_Click;
			comboBoxCustomVideoPlayer.SelectedIndexChanged += comboBoxCustomVideoPlayer_SelectedIndexChanged;
			btnCustomVideoPlayerChange.Click += btnCustomVideoPlayerChange_Click;
			comboBoxSearchEngine.SelectedIndexChanged += comboBoxSearchEngine_SelectedIndexChanged;

			checkSpellCheck.CheckedChanged += checkSpellCheck_CheckedChanged;
			comboBoxSpellCheckLanguage.SelectedValueChanged += comboBoxSpellCheckLanguage_SelectedValueChanged;
			comboBoxTranslationTarget.SelectedValueChanged += comboBoxTranslationTarget_SelectedValueChanged;
			comboBoxFirstDayOfWeek.SelectedValueChanged += comboBoxFirstDayOfWeek_SelectedValueChanged;
		}

		public override void OnClosing() {
			Config.ZoomLevel = trackBarZoom.Value;
		}

		#region User Interface

		private void checkExpandLinks_CheckedChanged(object sender, EventArgs e) {
			Config.ExpandLinksOnHover = checkExpandLinks.Checked;
		}

		private void checkFocusDmInput_CheckedChanged(object sender, EventArgs e) {
			Config.FocusDmInput = checkFocusDmInput.Checked;
		}

		private void checkOpenSearchInFirstColumn_CheckedChanged(object sender, EventArgs e) {
			Config.OpenSearchInFirstColumn = checkOpenSearchInFirstColumn.Checked;
		}

		private void checkKeepLikeFollowDialogsOpen_CheckedChanged(object sender, EventArgs e) {
			Config.KeepLikeFollowDialogsOpen = checkKeepLikeFollowDialogsOpen.Checked;
		}

		private void checkBestImageQuality_CheckedChanged(object sender, EventArgs e) {
			Config.BestImageQuality = checkBestImageQuality.Checked;
		}

		private void checkAnimatedAvatars_CheckedChanged(object sender, EventArgs e) {
			Config.EnableAnimatedImages = checkAnimatedAvatars.Checked;
			BrowserProcessHandler.UpdatePrefs().ContinueWith(task => reloadColumns());
		}

		private void trackBarZoom_ValueChanged(object sender, EventArgs e) {
			if (trackBarZoom.AlignValueToTick()) {
				zoomUpdateTimer.Stop();
				zoomUpdateTimer.Start();
				labelZoomValue.Text = trackBarZoom.Value + "%";
			}
		}

		private void zoomUpdateTimer_Tick(object sender, EventArgs e) {
			Config.ZoomLevel = trackBarZoom.Value;
			zoomUpdateTimer.Stop();
		}

		#endregion

		#region Updates

		private void checkUpdateNotifications_CheckedChanged(object sender, EventArgs e) {
			Config.EnableUpdateCheck = checkUpdateNotifications.Checked;
		}

		private void btnCheckUpdates_Click(object sender, EventArgs e) {
			Config.DismissedUpdate = null;

			btnCheckUpdates.Enabled = false;
			updateCheckEventId = updates.Check(true);
		}

		private void updates_CheckFinished(object sender, UpdateCheckEventArgs e) {
			if (e.EventId == updateCheckEventId) {
				btnCheckUpdates.Enabled = true;

				e.Result.Handle(update => {
					if (update.VersionTag == Program.VersionTag) {
						FormMessage.Information("No Updates Available", "Your version of TweetDuck is up to date.", FormMessage.OK);
					}
				}, ex => {
					Program.Reporter.HandleException("Update Check Error", "An error occurred while checking for updates.", true, ex);
				});
			}
		}

		#endregion

		#region Browser Settings

		private void checkSmoothScrolling_CheckedChanged(object sender, EventArgs e) {
			Config.EnableSmoothScrolling = checkSmoothScrolling.Checked;
		}

		private void checkTouchAdjustment_CheckedChanged(object sender, EventArgs e) {
			Config.EnableTouchAdjustment = checkTouchAdjustment.Checked;
		}

		private void checkHardwareAcceleration_CheckedChanged(object sender, EventArgs e) {
			SysConfig.HardwareAcceleration = checkHardwareAcceleration.Checked;
		}

		private void UpdateBrowserChangeButton() {
			btnCustomBrowserChange.Visible = comboBoxCustomBrowser.SelectedIndex == browserListIndexCustom;
		}

		private void UpdateBrowserPathSelection() {
			if (string.IsNullOrEmpty(Config.BrowserPath) || !File.Exists(Config.BrowserPath)) {
				comboBoxCustomBrowser.SelectedIndex = browserListIndexDefault;
			}
			else {
				WindowsUtils.Browser browserInfo = comboBoxCustomBrowser.Items.OfType<WindowsUtils.Browser>().FirstOrDefault(browser => browser.Path == Config.BrowserPath);

				if (browserInfo == null || Config.BrowserPathArgs != null) {
					comboBoxCustomBrowser.SelectedIndex = browserListIndexCustom;
				}
				else {
					comboBoxCustomBrowser.SelectedItem = browserInfo;
				}
			}

			UpdateBrowserChangeButton();
		}

		private void comboBoxCustomBrowser_SelectedIndexChanged(object sender, EventArgs e) {
			if (comboBoxCustomBrowser.SelectedIndex == browserListIndexCustom) {
				btnCustomBrowserChange_Click(sender, e);
			}
			else {
				Config.BrowserPath = (comboBoxCustomBrowser.SelectedItem as WindowsUtils.Browser)?.Path; // default browser item is a string and casts to null
				Config.BrowserPathArgs = null;
				UpdateBrowserChangeButton();
			}
		}

		private void btnCustomBrowserChange_Click(object sender, EventArgs e) {
			using (DialogSettingsExternalProgram dialog = new DialogSettingsExternalProgram("External Browser", "Open Links With...") {
				Path = Config.BrowserPath,
				Args = Config.BrowserPathArgs
			}) {
				if (dialog.ShowDialog() == DialogResult.OK) {
					Config.BrowserPath = dialog.Path;
					Config.BrowserPathArgs = dialog.Args;
				}
			}

			comboBoxCustomBrowser.SelectedIndexChanged -= comboBoxCustomBrowser_SelectedIndexChanged;
			UpdateBrowserPathSelection();
			comboBoxCustomBrowser.SelectedIndexChanged += comboBoxCustomBrowser_SelectedIndexChanged;
		}

		private void UpdateVideoPlayerChangeButton() {
			btnCustomVideoPlayerChange.Visible = comboBoxCustomVideoPlayer.SelectedIndex == videoPlayerListIndexCustom;
		}

		private void UpdateVideoPlayerPathSelection() {
			if (string.IsNullOrEmpty(Config.VideoPlayerPath) || !File.Exists(Config.VideoPlayerPath)) {
				comboBoxCustomVideoPlayer.SelectedIndex = videoPlayerListIndexDefault;
			}
			else {
				comboBoxCustomVideoPlayer.SelectedIndex = videoPlayerListIndexCustom;
			}

			UpdateVideoPlayerChangeButton();
		}

		private void comboBoxCustomVideoPlayer_SelectedIndexChanged(object sender, EventArgs e) {
			if (comboBoxCustomVideoPlayer.SelectedIndex == videoPlayerListIndexCustom) {
				btnCustomVideoPlayerChange_Click(sender, e);
			}
			else {
				Config.VideoPlayerPath = null;
				Config.VideoPlayerPathArgs = null;
				UpdateVideoPlayerChangeButton();
			}
		}

		private void btnCustomVideoPlayerChange_Click(object sender, EventArgs e) {
			using (DialogSettingsExternalProgram dialog = new DialogSettingsExternalProgram("External Video Player", "Play Videos With...") {
				Path = Config.VideoPlayerPath,
				Args = Config.VideoPlayerPathArgs
			}) {
				if (dialog.ShowDialog() == DialogResult.OK) {
					Config.VideoPlayerPath = dialog.Path;
					Config.VideoPlayerPathArgs = dialog.Args;
				}
			}

			comboBoxCustomVideoPlayer.SelectedIndexChanged -= comboBoxCustomVideoPlayer_SelectedIndexChanged;
			UpdateVideoPlayerPathSelection();
			comboBoxCustomVideoPlayer.SelectedIndexChanged += comboBoxCustomVideoPlayer_SelectedIndexChanged;
		}

		private void comboBoxSearchEngine_SelectedIndexChanged(object sender, EventArgs e) {
			if (comboBoxSearchEngine.SelectedIndex == searchEngineIndexCustom) {
				using (DialogSettingsSearchEngine dialog = new DialogSettingsSearchEngine()) {
					if (dialog.ShowDialog() == DialogResult.OK) {
						Config.SearchEngineUrl = dialog.Url.Trim();
					}
				}

				comboBoxSearchEngine.SelectedIndexChanged -= comboBoxSearchEngine_SelectedIndexChanged;
				UpdateSearchEngineSelection();
				comboBoxSearchEngine.SelectedIndexChanged += comboBoxSearchEngine_SelectedIndexChanged;
			}
			else {
				Config.SearchEngineUrl = (comboBoxSearchEngine.SelectedItem as SearchEngine)?.Url; // default search engine item is a string and casts to null
			}
		}

		private void UpdateSearchEngineSelection() {
			if (string.IsNullOrEmpty(Config.SearchEngineUrl)) {
				comboBoxSearchEngine.SelectedIndex = searchEngineIndexDefault;
			}
			else {
				SearchEngine engineInfo = comboBoxSearchEngine.Items.OfType<SearchEngine>().FirstOrDefault(engine => engine.Url == Config.SearchEngineUrl);

				if (engineInfo == null) {
					comboBoxSearchEngine.SelectedIndex = searchEngineIndexCustom;
				}
				else {
					comboBoxSearchEngine.SelectedItem = engineInfo;
				}
			}
		}

		private sealed class SearchEngine {
			private string Name { get; }
			public string Url { get; }

			public SearchEngine(string name, string url) {
				Name = name;
				Url = url;
			}

			public override int GetHashCode() => Name.GetHashCode();
			public override bool Equals(object obj) => obj is SearchEngine other && Name == other.Name;
			public override string ToString() => Name;
		}

		#endregion

		#region Locales

		private void checkSpellCheck_CheckedChanged(object sender, EventArgs e) {
			Config.EnableSpellCheck = checkSpellCheck.Checked;
			BrowserProcessHandler.UpdatePrefs();
		}

		private void comboBoxSpellCheckLanguage_SelectedValueChanged(object sender, EventArgs e) {
			Config.SpellCheckLanguage = (comboBoxSpellCheckLanguage.SelectedItem as LocaleUtils.Item)?.Code ?? "en-US";
		}

		private void comboBoxTranslationTarget_SelectedValueChanged(object sender, EventArgs e) {
			Config.TranslationTarget = (comboBoxTranslationTarget.SelectedItem as LocaleUtils.Item)?.Code ?? "en";
		}

		private void comboBoxFirstDayOfWeek_SelectedValueChanged(object sender, EventArgs e) {
			Config.CalendarFirstDay = (comboBoxFirstDayOfWeek.SelectedItem as DayOfWeekItem)?.Id ?? -1;
		}

		private sealed class DayOfWeekItem {
			private string Name { get; }
			public int Id { get; }

			public DayOfWeekItem(string name, DayOfWeek dow) {
				Name = name;
				Id = LocaleUtils.GetJQueryDayOfWeek(dow);
			}

			public override int GetHashCode() => Name.GetHashCode();
			public override bool Equals(object obj) => obj is DayOfWeekItem other && Name == other.Name;
			public override string ToString() => Name;
		}

		#endregion
	}
}

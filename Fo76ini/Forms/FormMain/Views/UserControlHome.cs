﻿using Fo76ini.Interface;
using Fo76ini.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fo76ini.Forms.FormMain.Tabs
{
    public partial class UserControlHome : UserControl
    {
        public UserControlHome()
        {
            InitializeComponent();

            this.Load += UserControlHome_Load;
            this.backgroundWorkerGetLatestVersion.DoWork += backgroundWorkerGetLatestVersion_DoWork;
            this.backgroundWorkerGetLatestVersion.RunWorkerCompleted += backgroundWorkerGetLatestVersion_RunWorkerCompleted;

            this.pictureBoxButtonUpdate.Click += PictureBoxButtonUpdate_Click;

            // Handle translations:
            Translation.LanguageChanged += OnLanguageChanged;
        }

        private void UserControlHome_Load(object sender, EventArgs e)
        {
            // What's new:
            LoadWhatsNew();

            // Check for updates:
            CheckVersion();
            IniFiles.Config.Set("General", "sPreviousVersion", Shared.VERSION);
        }

        public void OnLanguageChanged(object sender, TranslationEventArgs e)
        {
            Translation translation = (Translation)sender;

            // Set labels and stuff:
            this.labelTranslationAuthor.Visible = e.HasAuthor;
            this.labelTranslationBy.Visible = e.HasAuthor;
            this.labelTranslationAuthor.Text = e.HasAuthor ? translation.Author : "";

            // TODO: UpdateUI?
            this.CheckVersion();

            // Set font:
            this.labelWelcome.Font = CustomFonts.GetHeaderFont();

            //this.Refresh(); // Forces redraw
        }

        /*
         **************************************************************
         * Check version
         **************************************************************
         */

        #region Check version

        public void CheckVersion(bool force = false)
        {
            if (this.backgroundWorkerGetLatestVersion.IsBusy)
                return;

            this.labelConfigVersion.Text = Shared.VERSION;
            IniFiles.Config.Set("General", "sVersion", Shared.VERSION);

            panelUpdate.Visible = false;

            if (!force && Configuration.IgnoreUpdates)
            {
                this.labelConfigVersion.ForeColor = Color.Black;
                return;
            }

            this.labelConfigVersion.ForeColor = Color.Gray;
            this.pictureBoxSpinnerCheckForUpdates.Visible = true;

            // Checking version in background:
            this.backgroundWorkerGetLatestVersion.RunWorkerAsync();
        }

        private void backgroundWorkerGetLatestVersion_DoWork(object sender, DoWorkEventArgs e)
        {
            Versioning.GetLatestVersion();
        }

        private void backgroundWorkerGetLatestVersion_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            this.pictureBoxSpinnerCheckForUpdates.Visible = false;

            // Failed:
            if (Shared.LatestVersion == null)
            {
                this.labelConfigVersion.ForeColor = Color.Black;
                panelUpdate.Visible = false;
                return;
            }

            if (Versioning.UpdateAvailable || Configuration.Debug.ForceShowUpdateButton)
            {
                panelUpdate.Visible = true;
                labelNewVersion.Text = string.Format(Localization.GetString("newVersionAvailable"), Shared.LatestVersion);
                labelNewVersion.ForeColor = Color.Crimson;
                this.labelConfigVersion.ForeColor = Color.Red;
            }
            else
            {
                // All good, latest version:
                panelUpdate.Visible = false;
                this.labelConfigVersion.ForeColor = Color.DarkGreen;
            }
        }

        #endregion

        #region Update available

        // "Get the latest version from NexusMods" link:
        private void linkLabelManualDownloadPage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabelManualDownloadPage.LinkVisited = true;
            Process.Start(Shared.URLs.AppNexusModsDownloadURL);
        }

        private void PictureBoxButtonUpdate_Click(object sender, EventArgs e)
        {
            if (UpdateButtonClicked != null)
                UpdateButtonClicked(sender, e);
        }
        public event EventHandler UpdateButtonClicked;

        #endregion




        #region What's new

        /*
         * What's new:
         */

        private void LoadWhatsNew()
        {
#if DEBUG
            string debugFile = Path.Combine(Shared.AppConfigFolder, "What's new.html");
            if (File.Exists(debugFile))
                this.webBrowserWhatsNew.DocumentText = File.ReadAllText(debugFile);
            else
#endif
                this.webBrowserWhatsNew.Url = new Uri(Shared.URLs.RemoteWhatsNewHTMLURL);
        }

        #endregion

        private void styledButtonWhatsNew_Click(object sender, EventArgs e)
        {
            // Web browser does only work on Windows 10 (and newer) for some reason:
            if (Utils.IsWindows10OrNewer())
            {
                this.tabControlWithoutHeader1.SelectedTab = this.tabPageWhatsNew;

                // "Fixing" the web browser not rendering by resizing the window, which helps for some reason:
                // (https://stackoverflow.com/a/68837431)
                // TODO
                this.ParentForm.Height += 1;
                this.ParentForm.Height -= 1;
            }
            else
            {
                // Open users web browser on Windows 7 instead:
                Utils.OpenURL(Shared.URLs.RemoteWhatsNewHTMLURL);
            }
        }

        private void styledButtonGoBack_Click(object sender, EventArgs e)
        {
            this.tabControlWithoutHeader1.SelectedTab = this.tabPageHome;
        }

        #region Web links

        private void pictureBoxButtonSupport_Click(object sender, EventArgs e)
        {
            Utils.OpenURL("https://ko-fi.com/felisdiligens");
        }

        private void styledButtonNexusMods_Click(object sender, EventArgs e)
        {
            Utils.OpenURL("https://www.nexusmods.com/fallout76/mods/546");
        }

        private void styledButtonGitHub_Click(object sender, EventArgs e)
        {
            Utils.OpenURL("https://github.com/FelisDiligens/Fallout76-QuickConfiguration");
        }

        private void styledButtonWikiAndGuides_Click(object sender, EventArgs e)
        {
            Utils.OpenURL("https://github.com/FelisDiligens/Fallout76-QuickConfiguration/wiki");
        }

        private void styledButtonBugReports_Click(object sender, EventArgs e)
        {
            Utils.OpenURL("https://www.nexusmods.com/fallout76/mods/546?tab=bugs");
        }

        private void styledButtonBethesdaNetStatus_Click(object sender, EventArgs e)
        {
            Utils.OpenURL("https://bethesda.net/status");
        }

        private void styledButtonNukesAndDragonsBuildPlanner_Click(object sender, EventArgs e)
        {
            Utils.OpenURL("https://nukesdragons.com/fallout-76/character");
        }

        private void styledButtonxTranslator_Click(object sender, EventArgs e)
        {
            Utils.OpenURL("https://www.nexusmods.com/skyrimspecialedition/mods/134");
        }

        #endregion
    }
}

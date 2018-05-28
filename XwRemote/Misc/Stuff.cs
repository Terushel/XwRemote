﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpRaven;
using SharpRaven.Data;
using XwMaxLib.Extentions;
using XwRemote.Properties;

namespace XwRemote.Misc
{
    public partial class Stuff : Form
    {
        private string NewVersion = "";

        //************************************************************************************
        public Stuff()
        {
            InitializeComponent();
            version.Text = Main.CurrentVersion;
            faTabStrip1.SelectedItem = faTabAbout;
        }

        //************************************************************************************
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkLabel1.Text);
        }

        //************************************************************************************
        private void Stuff_Load(object sender, EventArgs e)
        {
            buttonUpdate.Enabled = false;
            using (Stream oStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XwRemote.Credits.rtf"))
            {
                richTextBox1.LoadFile(oStream, RichTextBoxStreamType.RichText);
            }
            
            scroller1.Interval = 40;
            scroller1.TextToScroll = Resources.whynot;
        }

        //************************************************************************************
        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        //************************************************************************************
        private async void faTabStrip1_TabStripItemSelectionChanged(FarsiLibrary.Win.TabStripItemChangedEventArgs e)
        {
            scroller1.Stop();
            if (e.Item == faTabWhyNot)
                scroller1.Start();

            if (e.Item == faTabUpdates)
                await CheckUpdates();
        }

        //************************************************************************************
        private void buttonSend_Click(object sender, EventArgs e)
        {
            if (CommentBox.Text.Trim() == "")
            {
                CommentBox.ShowBalloon(ToolTipIcon.Warning, "", "Empty message?");
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            string message = "";
            message += "***************************** MESSAGE *****************************\n";
            message += CommentBox.Text + "\n";
            message += "****************************** NAME *******************************\n";
            message += NameBox.Text + "\n";
            message += "****************************** MAIL *******************************\n";
            message += MailBox.Text + "\n";
            message += "*******************************************************************\n";

            var ravenClient = new RavenClient("https://11dbb280832c4f52a000577bf8eee1f8@sentry.io/1210500");
            SentryMessage msg = new SentryMessage(message);
            SentryEvent ev = new SentryEvent(msg);
            ravenClient.Capture(ev);
            Close();
        }

        //************************************************************************************
        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkLabel2.Text);
        }

        //************************************************************************************
     
        private void linkLabel3_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.paypal.me/maxsnts");
        }

        //************************************************************************************
        private async Task CheckUpdates()
        {
            using (WebClient client = new WebClient())
            {
                string URL = $"https://github.com/maxsnts/{Main.UpdateRepo}/releases/latest";
                try
                {
                    string content = await client.DownloadStringTaskAsync(URL);
                    Match m = Regex.Match(content, @"(?isx)/releases/tag/v(?<VERSION>.*?)""");
                    string latestVersion = m.Result("${VERSION}");

                    if (latestVersion != Main.CurrentVersion)
                    {
                        labelVersion.Text = $"There is a new version available: {latestVersion}";
                        NewVersion = latestVersion;
                        buttonUpdate.Enabled = true;
                    }
                    else
                    {
                        labelVersion.Text = "There is no new version at this date";
                        buttonUpdate.Enabled = false;
                    }
                }
                catch
                {
                    labelVersion.Text = "Unable to check for updates, update manually";
                    linkLatest.Text = URL;
                    linkLatest.Visible = true;
                    buttonUpdate.Enabled = false;
                }
            }
        }

        //************************************************************************************
        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            string path = Environment.CurrentDirectory;
            using (WebClient client = new WebClient())
            {
                string URL = $"https://github.com/maxsnts/{Main.UpdateRepo}/releases/download/v{NewVersion}/{Main.UpdateRepo}.v{NewVersion}.zip";
                try
                {
                    client.DownloadFile(URL, Path.Combine(path, $"{Main.UpdateRepo}.zip"));

                    File.WriteAllBytes(Path.Combine(path, "XwUpdater.exe"), Resources.XwUpdater);
               
                    using (Process process = new Process())
                    {
                        process.StartInfo.FileName = Path.Combine(path, "XwUpdater.exe");
                        process.StartInfo.WorkingDirectory = path;
                        process.StartInfo.Arguments = $"\"{Main.UpdateRepo}.exe\" \"{Main.UpdateRepo}.zip\" \"{path}\"";
                        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        process.Start();
                        Environment.Exit(0);
                    }
                }
                catch
                {
                    labelVersion.Text = "Unable to check for updates, update manually";
                    linkLatest.Text = URL;
                    linkLatest.Visible = true;
                    buttonUpdate.Enabled = false;
                }
            }
        }

        //************************************************************************************
        private void linkLatest_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkLatest.Text);
        }

        //************************************************************************************
        private void linkReleases_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkReleases.Text);
        }
    }
}
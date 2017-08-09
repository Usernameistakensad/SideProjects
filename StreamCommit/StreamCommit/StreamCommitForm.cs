using LibGit2Sharp;
using MetroFramework.Controls;
using MetroFramework.Forms;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace StreamCommit
{
    public partial class StreamCommitForm : MetroForm
    {
        public Settings Settings;
        private Committer _committer;
        private bool IsReadyToRun => !string.IsNullOrWhiteSpace(Settings.FolderToWatch) && Settings.HasCredentials;
        private Color _defaultMetroColor;

        public StreamCommitForm()
        {
            InitializeComponent();
            _defaultMetroColor = tbFolderToWatch.BackColor;
            Settings = new Settings();
            Settings.PropertyChanged += SettingsChanged;
            UpdateFolderToWatch();
            FormClosing += (o, e) =>
            {
                if (_committer != null)
                    _committer.Dispose();
            };

            _committer = new Committer();
            _committer.StatusChanged += StatusChanged;
        }

        private void UpdateFolderToWatch()
        {
            if (string.IsNullOrWhiteSpace(Settings.FolderToWatch))
            {
                btnToggleRun.Enabled = false;
                return;
            }

            if (!Repository.IsValid(Settings.FolderToWatch))
            {
                MessageBox.Show("The folder you selected is not a valid Git repository.");
                Settings.FolderToWatch = "";
                return;
            }
            tbFolderToWatch.Text = Settings.FolderToWatch;
        }

        private void StatusChanged(string newStatus)
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.BeginInvoke((MethodInvoker)delegate () { lblStatus.Text = newStatus; });
            }
            else
            {
                lblStatus.Text = $"Status: {newStatus}";
            }
        }

        private void ToggleRun(object sender, EventArgs e)
        {
            if (!IsReadyToRun)
                return;

            if (_committer == null)
            {
                _committer = new Committer();
                _committer.Path = Settings.FolderToWatch;
                _committer.CommitInterval = Settings.CommitInterval;
            }

            var btn = (MetroButton)sender;

            if (_committer.Running)
            {
                btnCredentials.Enabled = false;
                btnFolderToWatch.Enabled = false;
                _committer.StopMonitoring();
            }
            else
            {
                btnCredentials.Enabled = true;
                btnFolderToWatch.Enabled = true;
                _committer.StartMonitoring();
            }
            btn.Text = (_committer.Running ? "Stop" : "Start");
        }

        private void PickFolderToWatch(object sender, EventArgs e)
        {
            var result = fbdFolderToWatch.ShowDialog();
            if (result != DialogResult.OK)
                return;

            if (!string.IsNullOrWhiteSpace(fbdFolderToWatch.SelectedPath))
            {
                Settings.FolderToWatch = fbdFolderToWatch.SelectedPath;
                tbFolderToWatch.Text = Settings.FolderToWatch;
                UpdateFolderToWatch();
            }
        }

        private void btnCredentials_Click(object sender, EventArgs e)
        {
            var result = new EnterCredentials().ShowDialog();
        }

        private void SettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_committer != null && _committer.Running)
                return;

            btnToggleRun.Enabled = IsReadyToRun;
        }

        private void tbCommitInterval_TextChanged(object sender, EventArgs e)
        {
            int tmp;
            if (int.TryParse(tbCommitInterval.Text, out tmp))
            {
                tbCommitInterval.BackColor = _defaultMetroColor;
                Settings.CommitInterval = tmp * 1000; //convert to ms
            }
            else
            {
                tbCommitInterval.BackColor = Color.Red;
            }
        }

        private void StreamCommitForm_Load(object sender, EventArgs e)
        {
            tbFolderToWatch.Text = Settings.FolderToWatch ?? "";
            tbCommitInterval.Text = (Settings.CommitInterval / 1000).ToString() ?? "10";

            btnToggleRun.Enabled = IsReadyToRun;
        }
    }
}
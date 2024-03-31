using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VaultSync
{
    public partial class UpdateCheckForm : Form
    {
        readonly UpdateCheck checker = new UpdateCheck();
        public bool DisplayOnly { get; set; }

        public UpdateCheckForm(bool displayOnly)
        {
            InitializeComponent();

            DisplayOnly = displayOnly;

            if (!DisplayOnly) {
                checker.OnVersionResult += OnVersionCheckResult;
            }
            SiteLink.Text = Strings.VersionCheckURL;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void UpdateCheck_Load(object sender, EventArgs e)
        {
            Message.Text = Strings.UpdateCheck;
            if (DisplayOnly)
            {
                OnVersionCheckResult(UpdateCheck.VersionResult.UpdateAvailable);
            }
            else
            {
                checker.CheckForUdate();
            }
        }

        private void OnVersionCheckResult(UpdateCheck.VersionResult checkResult)
        {
            var message = "";
            var link = false;
            switch (checkResult)
            {
                case UpdateCheck.VersionResult.UpdateAvailable:
                    message = Strings.UpdateAvailable;
                    link = true;
                    break;
                case UpdateCheck.VersionResult.NoChange:
                    message = Strings.UpdateNotRequired;
                    break;
                case UpdateCheck.VersionResult.Error:
                    message = Strings.UpdateCheckFailed;
                    link = true;
                    break;
            }

            Action messageUpdate = delegate {
                Message.Text = message;
            };

            if (Message.InvokeRequired)
            {
                Message.Invoke(messageUpdate);
            }
            else
            {
                messageUpdate();
            }

            Action linkUpdate = delegate {
                SiteLink.Visible = link;
            };

            if (SiteLink.InvokeRequired)
            {
                SiteLink.Invoke(linkUpdate);
            }
            else
            {
                linkUpdate();
            }
        }

        private void SiteLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(SiteLink.Text);
        }
    }
}

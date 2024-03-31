// Copyright © 2019-2023 Simon Knight
// This file is part of VaultSync.

// VaultSync is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// any later version.

// VaultSync is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with VaultSync.  If not, see<https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VaultSync
{
    public abstract partial class ProgressForm : Form
    {
        protected Int64 total;
        private Int64 count;
        private bool aborting;
        protected DirectoryManagement manager;
        protected static readonly int CLOSE_DELAY = 200; // Milliseconds to wait after progress completes before closing the form

        public ProgressForm(DirectoryManagement mgr)
        {
            manager = mgr;
            manager.Abort = false;
            manager.Progress = Progress;
            aborting = false;
            count = 0;
            InitializeComponent();
        }

        private void AbortButton_Click(object sender, EventArgs e)
        {
            aborting = manager.Abort = true;
            abortButton.Text = "Cancel in progress";
        }

        abstract protected void ProgressForm_Load(object sender, EventArgs e);

        public void Progress(string item)
        {
            if (total > 0 && !aborting)
            {
                count += 1;
                int percent = (int)((count * 100) / total);
                if (percent > 100)
                {
                    percent = 100;
                }
                Invoke(new Action(() => {progressBar.Value = percent; fileName.Text = item; }));
            }
        }
    }

    public class ExtractProgressForm : ProgressForm
    {
        private readonly List<FileDetail> items;

        public ExtractProgressForm(DirectoryManagement mgr, List<FileDetail> items) : base(mgr)
        {
            this.items = items;
        }

        override protected void ProgressForm_Load(object sender, EventArgs e)
        {
            ExtractFiles();
        }

        async private void ExtractFiles()
        {
            await Task.Run(() => { total = manager.CountSelectedFiles(items); });
            await Task.Run(() => { manager.ExtractFiles(items); });
            await Task.Delay(CLOSE_DELAY);
            Close();
        }
    }

    public class SyncProgressForm : ProgressForm
    {
        public SyncProgressForm(DirectoryManagement mgr) : base(mgr)
        {
        }

        override protected void ProgressForm_Load(object sender, EventArgs e)
        {
            SyncFiles();
        }

        async private void SyncFiles()
        {
            await Task.Run(() => { total = manager.CountSyncFiles(); });
            await Task.Run(() => { manager.SyncFiles(); });
            await Task.Delay(CLOSE_DELAY);
            Close();
        }
    }

    public class DeleteProgressForm : ProgressForm
    {
        private readonly List<FileDetail> items;

        public DeleteProgressForm(DirectoryManagement mgr, List<FileDetail> items) : base(mgr)
        {
            this.items = items;
        }

        override protected void ProgressForm_Load(object sender, EventArgs e)
        {
            DeleteFiles();
        }

        async private void DeleteFiles()
        {
            await Task.Run(() => { total = manager.CountSelectedFiles(items); });
            await Task.Run(() => { manager.DeleteFiles(items); });
            await Task.Delay(CLOSE_DELAY);
            Close();
        }
    }
}

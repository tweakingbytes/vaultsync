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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;

namespace VaultSync
{
    public partial class MainForm : Form
    {
        // Column sorters for each of the lists
        private readonly ListViewColumnSorter syncColumnSorter;
        private readonly ListViewColumnSorter ignoreColumnSorter;

        private DirectoryManagement directoryManager; // single instance of the directory manager
        private ListCache listCache;
        private static readonly int MAX_LOGIN_ATTEMPTS = 5;

        public MainForm()
        {
            InitializeComponent();

            // Create an instance of a ListView column sorter and assign it 
            // to the ListView controls.
            syncColumnSorter = new ListViewColumnSorter();
            syncList.ListViewItemSorter = syncColumnSorter;

            ignoreColumnSorter = new ListViewColumnSorter();
            ignoreList.ListViewItemSorter = ignoreColumnSorter;
        }

        private string VaultPath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "VaultSync");
        }

        // Refresh the file list view with the contents of the directory table from database
        private void RefreshContentView()
        {
            contentsList.BeginUpdate();
            contentsList.SelectedIndices.Clear();
            SetListViewType();
            SetToolEnables();

            contentsList.VirtualListSize = listCache.ResetCache(0, "");
            contentsList.EndUpdate();

            if (contentsList.VirtualListSize == 0)
            {
                // If there is nothing in the content list then show the sync tab
                tabControl.SelectedTab = tabSync;
            }

            SetToolEnables();
        }

        // Refresh the ignore list view with the contents of the ignore table from database
        private void RereshIgnoreView()
        {
            ignoreList.BeginUpdate();
            ignoreList.Items.Clear();
            ignoreList.SelectedItems.Clear();
            directoryManager.LoadIgnore(UpdateIgnoreList);
            ignoreList.EndUpdate();
            ignoreList.Sort();
        }

        // One stop shop for setting control enables
        private void SetToolEnables()
        {
            SetBackEnable();
            SetUpEnable();
            SetSyncEnable();
            SetExtractEnable();
            SetAddEnable();
            SetDeleteEnable();
            SetIgnoreEnable();
            SetEditEnable();
            SetReHostEnable();
        }

        // Can edit on the ignore tab if there are items there
        private void SetEditEnable()
        {
            contextEdit.Enabled = (tabControl.SelectedTab == tabIgnore && ignoreList.Items.Count > 0);
        }

        // Can ignore files on the file tab if there are items there
        private void SetIgnoreEnable()
        {
            contextIgnore.Enabled = (tabControl.SelectedTab == tabVault && contentsList.VirtualListSize > 0);
        }

        // Can go back a directory level on the file tab if there are items there and a parent directory exists
        private void SetBackEnable()
        {
            if (tabControl.SelectedTab == tabVault && contentsList.VirtualListSize > 0)
            {
                for (int index = 0; index < contentsList.VirtualListSize; index += 1)
                {
                    ListViewItem item = listCache.GetItem(index);
                    if (item.Name.Contains(Path.DirectorySeparatorChar))
                    {
                        toolStripUpLevel.Enabled = true;
                        return;
                    }
                }
            }
            toolStripUpLevel.Enabled = false;
        }

        // Sync is enabled if there are any sync items
        private void SetSyncEnable()
        {
            toolStripSyncButton.Enabled = syncList.Items.Count > 0;
        }

        // Can go to the root directory on the file tab if entries exist that are not at the root
        private void SetUpEnable()
        {
            if (tabControl.SelectedTab == tabVault && contentsList.VirtualListSize > 0)
            {
                for (int index = 0; index < contentsList.VirtualListSize; index += 1)
                {
                    ListViewItem item = listCache.GetItem(index);
                    if (item.Name.Contains(Path.DirectorySeparatorChar))
                    {
                        toolStripUp.Enabled = true;
                        return;
                    }
                }
            }
            toolStripUp.Enabled = false;
        }

        // Can extract in the file tab if items are selected or in the context menu if items exist
        private void SetExtractEnable()
        {
            toolExtract.Enabled = (tabControl.SelectedTab == tabVault && contentsList.SelectedIndices.Count > 0);
            contextExtract.Enabled = (tabControl.SelectedTab == tabVault && contentsList.VirtualListSize > 0);
        }

        // Can delete items if any are selected or on the context menu if items exist
        private void SetDeleteEnable()
        {
            if (tabControl.SelectedTab == tabSync)
            {
                toolDelete.Enabled = syncList.SelectedItems.Count > 0;
                contextDelete.Enabled = syncList.Items.Count > 0;
            }
            else if (tabControl.SelectedTab == tabVault)
            {
                toolDelete.Enabled = contentsList.SelectedIndices.Count > 0;
                contextDelete.Enabled = contentsList.VirtualListSize > 0;
            }
            else if (tabControl.SelectedTab == tabIgnore)
            {
                toolDelete.Enabled = ignoreList.SelectedItems.Count > 0;
                contextDelete.Enabled = ignoreList.Items.Count > 0;
            }
        }

        // Can add new items on the sync and ignore tabs. Can add folders on the sync tab.
        private void SetAddEnable()
        {
            if (tabControl.SelectedTab == tabSync)
            {
                toolAddItem.Enabled = toolAddFolder.Enabled = true;
            }
            else if (tabControl.SelectedTab == tabIgnore)
            {
                toolAddItem.Enabled = true;
                toolAddFolder.Enabled = false;
            }
            else
            {
                toolAddItem.Enabled = toolAddFolder.Enabled = false;
            }
        }

        // Can rehost items if any are selected or on the context menu if items exist
        private void SetReHostEnable()
        {
            if (tabControl.SelectedTab == tabVault && contentsList.SelectedIndices.Count > 0)
            {
                ListViewItem item = listCache.GetItem(contentsList.SelectedIndices[0]); // Item under the mouse
                FileDetail detail = (FileDetail)item.Tag;

                moveHostToolStripMenuItem.Enabled = (detail.Host != "" && detail.Host != System.Environment.MachineName);
            }
            else
            {
                moveHostToolStripMenuItem.Enabled = false;
            }
        }


        // Action handler to add items to the sync list view.
        // The job parameter isn't used here but is required by other handlers
        private void UpdateSyncList(string host, string path, DataConnector.SyncType type, Action<string> job)
        {
            ListViewItem item = new ListViewItem(path);
            string typeString = Utils.TypeString(type);
            item.Name = path;
            item.SubItems.Add(typeString);
            item.SubItems.Add(host);
            Utils.SetIcon(item);
            syncList.Items.Add(item);
        }

        // Action handler for the ignore list.
        private void UpdateIgnoreList(string pattern)
        {
            ListViewItem item = new ListViewItem(pattern)
            {
                Name = pattern
            };
            ignoreList.Items.Add(item);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // See if the program is already running and don't let it be run twice
            CheckForExisitingProcess();

            // Assign the image cache to the sync and file list views
            ImageCache.Init();
            contentsList.SmallImageList = ImageCache.SmallThumbnail;
            contentsList.LargeImageList = ImageCache.LargeThumbnail;
            syncList.SmallImageList = ImageCache.SmallThumbnail;
        }

        void CheckForExisitingProcess()
        {
            string me = Process.GetCurrentProcess().ProcessName;
            int count = 0;
            foreach (Process proc in Process.GetProcesses())
            {
                if (proc.ProcessName.Equals(me))
                {
                    count++;
                }
            }

            if (count > 1)
            {
                MessageBoxForm.Show(Strings.DuplicateProcess);

                Application.Exit();
            }
        }

        private void CreateVault()
        {
            ChangePasswordForm pwd = new ChangePasswordForm();
            if (pwd.ShowDialog() == DialogResult.Cancel)
            {
                Application.Exit();
                return;
            }

            string vaultLocation = VaultPath();
            directoryManager = new DirectoryManagement(pwd.Password, vaultLocation);

            if (directoryManager.Create())
            {
                listCache = new ListCache(directoryManager.DB);
                toolStripChangePwd.Enabled = true;
                toolHelp.Enabled = true;
                toolUpdate.Enabled = true;
            }
            else
            {
                MessageBoxForm.Show(string.Format(Strings.VaultCreateFailure, vaultLocation));
                Application.Exit();
                return;
            }
        }

        // Keep asking for a password until the database successfully opens, we run out of attempts or the user gives up
        private void TryToOpenTheDatabase()
        {
            PasswordForm pwd = new PasswordForm();
            for (int i =0; i < MAX_LOGIN_ATTEMPTS; i += 1)
            {
                if (pwd.ShowDialog() == DialogResult.Cancel)
                {
                    Application.Exit();
                    return;
                }
                string password = pwd.Password;
                pwd.DisplayIncorrectPassword();
                Cursor = Cursors.WaitCursor;
                if (OpenVault(password))
                {
                    toolStripChangePwd.Enabled = true;
                    toolHelp.Enabled = true;
                    toolUpdate.Enabled = true;
                    Cursor = Cursors.Arrow;
                    startUpTimer.Enabled = true;
                    startUpTimer.Start();
                    return;
                }
                Cursor = Cursors.Arrow;
            }

            // Failed to open the database within the number of attempts
            MessageBoxForm.Show(Strings.TooManyAttempts);
            
            Application.Exit();
            return;
        }

        // Open the vault if the password is correct
        // Return false if the vault won't open
        private bool OpenVault(string password)
        {
            string vaultLocation = VaultPath();
            directoryManager = new DirectoryManagement(password, vaultLocation);

            if (directoryManager.Open())
            {
                listCache = new ListCache(directoryManager.DB);
                UseWaitCursor = true;
                syncList.BeginUpdate();
                directoryManager.LoadSyncNames(UpdateSyncList, null);
                syncList.EndUpdate();
                syncList.Sort();
                RefreshContentView();
                RereshIgnoreView();
                SetToolEnables();
                SetAutoSync();
                SetViewType();
                UseWaitCursor = false;
                return true;
            }
            return false;
        }

        private void SetListViewType()
        {
            switch(ViewType.SelectedIndex)
            {
                case 0: contentsList.View = View.LargeIcon; break;
                case 1: contentsList.View = View.SmallIcon; break;
                default: contentsList.View = View.Details; break;
            }            
        }

        private void SetViewType()
        {
            try
            {
                var viewType = directoryManager.DB.GetParameter("view-type");
                ViewType.SelectedIndex = Int32.Parse(viewType);
            }
            catch
            {
                ViewType.SelectedIndex = 2; // Select 'Details' as default
            }
        }

        private void DoAutoSync()
        {
            if (AutoSync.Checked)
            {
                toolStripSyncButton.PerformClick();
            }
        }

        private void SetAutoSync()
        {
            AutoSync.Enabled = true;
            string doAutosync = directoryManager.DB.GetParameter("auto-sync");
            AutoSync.Checked = doAutosync != null && doAutosync.Equals("yes");
        }

        // Most tool enables depend on selection so if it changes reset the enables
        private void FileList_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetToolEnables();
        }

        // Multiselect with shift fires this event
        private void FileList_VirtualItemsSelectionRangeChanged(object sender, ListViewVirtualItemsSelectionRangeChangedEventArgs e)
        {
            SetToolEnables();
        }

        // Endpoint for the event handler for adding a file to the sync list
        // Open a multiselect file dialog and add each file selected
        private void AddFileClick()
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Multiselect = true;
                DialogResult result = dlg.ShowDialog();

                if (result == DialogResult.OK)
                {
                    syncList.BeginUpdate();
                    foreach (string name in dlg.FileNames)
                    {
                        // Add the file to the list if it was sucessfully added to the database (duplicates are ignored)
                        AddSyncItem(name, DataConnector.SyncType.File);
                    }
                    syncList.EndUpdate();
                    syncList.Sort();
                }
                SetToolEnables();
            }
        }

        // Endpoint for the event handler for adding a folder to the sync list
        // Open a folder browser dialog and add the selected folder
        private void AddFolderClick()
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                DialogResult result = dlg.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dlg.SelectedPath))
                {
                    AddSyncItem(dlg.SelectedPath, DataConnector.SyncType.Folder);
                    syncList.Sort();
                }
            }
            SetToolEnables();
        }

        private void AddSyncItem(string path, DataConnector.SyncType type)
        {
            // Add the folder to the list if it was sucessfully added to the database (duplicates are ignored)
            if (directoryManager.InsertSyncPoint(Environment.MachineName, path, type))
            {
                UpdateSyncList(Environment.MachineName, path, type, null);
            }
        }

        // Delete selected sync items
        private void DeleteSyncItems()
        {
            syncList.BeginUpdate();
            foreach (ListViewItem item in syncList.SelectedItems)
            {
                directoryManager.DeleteSyncPoint(Environment.MachineName, item.Name);
                syncList.Items.Remove(item);
            }
            syncList.SelectedItems.Clear();
            syncList.EndUpdate();
            syncList.Sort();

            SetToolEnables();
        }

        // End point for an extract event
        // Ask if the file should be extracted to the original folder and provide an alternative folder if not
        // Allow the user to abandon the extract at each stage
        private void ExtractClick()
        {
            DialogResult dialogResult = MessageBoxForm.Show(Strings.AskIfExtractToOriginalFolder, MessageBoxButtons.YesNoCancel);
            switch (dialogResult) {
                case DialogResult.Yes:
                    directoryManager.ExtractFolder = null;
                    break;

                case DialogResult.No:
                    using (FolderBrowserDialog dlg = new FolderBrowserDialog())
                    {
                        dialogResult = dlg.ShowDialog();

                        if (dialogResult == DialogResult.OK && !string.IsNullOrWhiteSpace(dlg.SelectedPath))
                        {
                            directoryManager.ExtractFolder = dlg.SelectedPath;
                        }
                        else
                        {
                            return;
                        }
                    }
                    break;

                case DialogResult.Cancel:
                    return;
            }

            DoExtraction();
        }

        // Set up the progress dialog to do the extraction asynchronously
        // The extraction folder has been previously set in the directory manager
        private void DoExtraction()
        {
            List<FileDetail> items = GetSelectionList();

            ProgressForm progress = new ExtractProgressForm(directoryManager, items);
            progress.ShowDialog();
            SetExtractEnable();
        }


        // Context menu handler to add a file to the ignore list
        private void IgnoreClick()
        {
            ListViewItem item = listCache.GetItem(contentsList.SelectedIndices[0]); // Item under the mouse
            FileDetail detail = (FileDetail)item.Tag;
            string pattern;
            if (detail.IsFolder)
            {
                // This is a directory entry so add a wild card to make it ignore all paths below the directory
                pattern = item.Name + Path.DirectorySeparatorChar + "*";
            }
            else
            {
                pattern = detail.Path;
            }

            DoIgnoreInsert(pattern);
        }

        // Allow the user to edit the ignore pattern before saving it
        private void DoIgnoreInsert(string pattern)
        {
            IgnorePatternForm dlg = new IgnorePatternForm
            {
                IgnorePattern = pattern
            };
            if (dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(dlg.IgnorePattern))
            {
                ignoreList.BeginUpdate();

                // Update the list only if the item was saved (duplicates are ignored)
                if (directoryManager.InsertIgnoreItem(dlg.IgnorePattern))
                {
                    UpdateIgnoreList(dlg.IgnorePattern);
                }

                // Deselect ignored item
                ignoreList.SelectedItems.Clear();
                ignoreList.EndUpdate();
                ignoreList.Sort();

                SetToolEnables();
            }
        }

        // Return a list of FileDetails from the file view selection list
        private List<FileDetail> GetSelectionList()
        {
            List<FileDetail> items = new List<FileDetail>();
            foreach (int index in contentsList.SelectedIndices)
            {
                ListViewItem item = listCache.GetItem(index);
                if (item.Tag != null)
                {
                    items.Add((FileDetail)(item.Tag));
                }
            }

            return items;
        }

        // Set up a progress dialog to delete the files asynchronously
        private void DeleteVaultItem()
        {
            List<FileDetail> items = GetSelectionList();

            ProgressForm progress = new DeleteProgressForm(directoryManager, items);
            progress.ShowDialog();

            contentsList.BeginUpdate();
            contentsList.SelectedIndices.Clear();
            contentsList.VirtualListSize = listCache.ResetCache();
            contentsList.EndUpdate();
            if (contentsList.VirtualListSize == 0)
            {
                // Everyting in the list has been deleted and possibly also up the directory chain, so display the file list at the root
                ListFromDirectoryRoot();
            }
            else
            {
                SetToolEnables();
            }
        }

        // Selections have changed so update the control enables
        private void ListContentsView_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetToolEnables();
        }

        // Event handler on the file list
        private void ListContentsView_DoubleClick(object sender, EventArgs e)
        {
            FileDetail detail = (FileDetail)listCache.GetItem(contentsList.SelectedIndices[0]).Tag;
            if (detail.IsFolder)
            {
                // Item is a directory so open the next diretory level down
                Int64 parentId = detail.Parent;
                string text = listCache.GetItem(contentsList.SelectedIndices[0]).Name;
                CurrentDirectory.Text = text;

                UseWaitCursor = true;
                contentsList.BeginUpdate();
                SetToolEnables();
                contentsList.VirtualListSize = listCache.ResetCache(parentId, text);
                contentsList.EndUpdate();

                // contentsList.Sort();
                SetToolEnables();
                UseWaitCursor = false;
            }
            else
            {
                // Item is a file so then the file using the system viewer for the file type
                directoryManager.ShowFile(detail);
            }
        }

        // Event handler for the up button
        private void ToolStripUpButton_Click(object sender, EventArgs e)
        {
            ListFromDirectoryRoot();
            SetToolEnables();
        }

        // Display the file list at the root
        private void ListFromDirectoryRoot()
        {
            UseWaitCursor = true;
            contentsList.SelectedIndices.Clear();
            SetToolEnables();
            CurrentDirectory.Text = "";

            contentsList.BeginUpdate();
            contentsList.VirtualListSize = listCache.ResetCache(0, "");
            contentsList.EndUpdate();
            SetToolEnables();
            UseWaitCursor = false;
        }

        // Event handler for the back button
        // Load the contents of the parent folder to the file list
        private void ToolStripUpLevel_Click(object sender, EventArgs e)
        {
            var here = listCache.GetItem(0).Name;
            var path = directoryManager.RemoveLastFolder(here);
            var nextPath = directoryManager.RemoveLastFolder(path);
            CurrentDirectory.Text = nextPath;

            UseWaitCursor = true;
            contentsList.SelectedIndices.Clear();
            SetToolEnables();
            contentsList.BeginUpdate();
            contentsList.VirtualListSize = listCache.ResetCache(directoryManager.FindOrCreatParent(path), nextPath);
            contentsList.EndUpdate();
            SetToolEnables();
            UseWaitCursor = false;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            directoryManager?.Close();
            Cursor = Cursors.Arrow;
        }

        // Clear all selections when the tab is switched
        private void TabControl_Selected(object sender, TabControlEventArgs e)
        {
            syncList.SelectedItems.Clear();
            contentsList.SelectedIndices.Clear();
            ignoreList.SelectedItems.Clear();
            SetToolEnables();
        }

        // Set up a progress for to perform the sync asynchronously
        private void ToolStripSyncButton_Click(object sender, EventArgs e)
        {
            ProgressForm progress = new SyncProgressForm(directoryManager);
            progress.ShowDialog();
            RefreshContentView();
            SetToolEnables();
        }

        // Delay vault opening until the main form is actually shown
        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (!Directory.Exists(VaultPath()))
                {
                    CreateVault();
                }
                else
                {
                    TryToOpenTheDatabase();
                }
            }

            private void ToolStripChangePwd_Click(object sender, EventArgs e)
        {
            ChangePasswordForm dlg = new ChangePasswordForm();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                directoryManager.ChangePassword(dlg.Password);
            }
        }

        private void TabControl_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    if (toolDelete.Enabled)
                    {
                        ProcessDelete();
                    }
                    e.Handled = true;
                    break;

                case Keys.F2:
                    // Show the name of the temp database file
                    MessageBoxForm.Show(directoryManager.TempDBName.Replace('\\', '/')); // Make the path compatible with SQLite command shell
                    e.Handled = true;
                    break;

                case Keys.V:
                    if (e.Control && tabControl.SelectedTab == tabSync)
                    {
                        System.Collections.Specialized.StringCollection files = Clipboard.GetFileDropList();
                        AddDroppedFiles(files);
                        e.Handled = true;
                    }
                    break;
            }

        }

        private void AddDroppedFiles(IEnumerable files)
        {
            syncList.BeginUpdate();
            foreach (string path in files)
            {
                if (Directory.Exists(path))
                {
                    AddSyncItem(path, DataConnector.SyncType.Folder);
                }
                else
                {
                    AddSyncItem(path, DataConnector.SyncType.File);
                }
            }
            syncList.EndUpdate();
            syncList.Sort();
        }

        private void ToolDelete_Click(object sender, EventArgs e)
        {
            ProcessDelete();
        }

        private void ProcessDelete()
        {
            if (tabControl.SelectedTab == tabSync)
            {
                DeleteSyncItems();
            }
            else if (tabControl.SelectedTab == tabVault)
            {
                DeleteVaultItem();
            }
            else if (tabControl.SelectedTab == tabIgnore)
            {
                DeleteIgnoreItems();
            }
        }

        // Change the file list column sorting according to the column clicked
        private void ListContentsView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == listCache.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (listCache.Order == SortOrder.Ascending)
                {
                    listCache.Order = SortOrder.Descending;
                }
                else
                {
                    listCache.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                listCache.SortColumn = e.Column;
                listCache.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            contentsList.BeginUpdate();
            listCache.ResetCache();
            contentsList.EndUpdate();
            ListViewExtensions.SetSortIcon(contentsList, e.Column, listCache.Order);
        }

        // Change the sync list column sorting according to the column clicked
        private void SyncList_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == syncColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (syncColumnSorter.Order == SortOrder.Ascending)
                {
                    syncColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    syncColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                syncColumnSorter.SortColumn = e.Column;
                syncColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            syncList.Sort();
            ListViewExtensions.SetSortIcon(syncList, e.Column, syncColumnSorter.Order);
        }

        // Change the ignore list column sorting according to the column clicked
        private void ListIgnore_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == ignoreColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (ignoreColumnSorter.Order == SortOrder.Ascending)
                {
                    ignoreColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    ignoreColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                ignoreColumnSorter.SortColumn = e.Column;
                ignoreColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            ignoreList.Sort();
            ListViewExtensions.SetSortIcon(ignoreList, e.Column, ignoreColumnSorter.Order);
        }


        private void ToolExtract_Click(object sender, EventArgs e)
        {
            ExtractClick();
        }

        private void ToolAddFile_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == tabSync)
            {
                AddFileClick();
            }
            if (tabControl.SelectedTab == tabIgnore)
            {
                AddIgnoreClick();
            }
        }

        private void AddIgnoreClick()
        {
            DoIgnoreInsert("");
       }

        private void DeleteIgnoreItems()
        {
            foreach(ListViewItem item in ignoreList.SelectedItems)
            {
                if (directoryManager.DeleteIgnorePattern(item.Name))
                {
                    ignoreList.Items.Remove(item);
                }
            }
            ignoreList.SelectedItems.Clear();
        }

        private void ToolAddFolder_Click(object sender, EventArgs e)
        {
            AddFolderClick();
        }

        private void ListIgnore_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetToolEnables();
        }

        private void IgnoreList_DoubleClick(object sender, EventArgs e)
        {
            EditIgnoreItem();
        }

        private void EditIgnoreItem()
        {
            IgnorePatternForm dlg = new IgnorePatternForm();
            ListViewItem item = ignoreList.SelectedItems[0];
            dlg.IgnorePattern = item.Name;
            if (dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(dlg.IgnorePattern))
            {
                ignoreList.BeginUpdate();
                directoryManager.DeleteIgnorePattern(item.Name);
                if (directoryManager.InsertIgnoreItem(dlg.IgnorePattern))
                {   
                    item.Name = dlg.IgnorePattern;
                }

                ignoreList.SelectedItems.Clear();
                ignoreList.EndUpdate();
                ignoreList.Sort();

                SetToolEnables();
            }
        }

        private void ContextIgnore_Click(object sender, EventArgs e)
        {
            IgnoreClick();
        }

        private void ContentDelete_Click(object sender, EventArgs e)
        {
            ProcessDelete();
        }

        private void ContextExtract_Click(object sender, EventArgs e)
        {
            ExtractClick();
        }

        private void ContextEdit_Click(object sender, EventArgs e)
        {
            EditIgnoreItem();
        }

        private void ToolHelp_Click(object sender, EventArgs e)
        {
            try
            {
                // Find the help file with the executable
                string exe = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                string path = Path.GetDirectoryName(exe).Replace('\\', '/');
                if (path[path.Length-1] != '/') {
                    path += "/";
                }
                string url = "file:///" + path + "Help/index.html";

                System.Diagnostics.Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBoxForm.Show("Can't open the help file " + ex.Message);
            }
        }

        private void ContentsList_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = listCache.GetItem(e.ItemIndex);
        }

        private void TabControl_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && tabControl.SelectedTab == tabSync)
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void TabControl_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            AddDroppedFiles(files);
        }

        private void ToolUpdate_Click(object sender, EventArgs e)
        {
            UpdateCheckForm check = new UpdateCheckForm(false);
            check.ShowDialog();
        }

        private void StartUpTimer_Tick(object sender, EventArgs e)
        {
            startUpTimer.Stop();
            startUpTimer.Enabled = false;
            StartShake();
            NagTimer.Start();
            DoAutoSync();
            UpdateCheck checker = new UpdateCheck();
            checker.OnVersionResult += OnVersionCheckResult;
            checker.CheckForUdate();
        }

        private void OnVersionCheckResult(UpdateCheck.VersionResult checkResult)
        {
            if (checkResult == UpdateCheck.VersionResult.UpdateAvailable) {
                if (!IsDisposed)
                {

                    if (InvokeRequired)
                    {
                        Action show = delegate
                        {
                            ShowUpateAvailableDialog();
                        };

                        Invoke(show);
                    }
                    else
                    {
                        ShowUpateAvailableDialog();
                    }
                }
            }
        }
        
        private void ShowUpateAvailableDialog()
        {
            UpdateCheckForm check = new UpdateCheckForm(true);
            check.ShowDialog();
        }

        private void OnAutoSyncClick(object sender, EventArgs e)
        {
            String check = "no";
            if (!AutoSync.Checked)
            {
                check = "yes";
            }
 
            directoryManager.DB.UpdateParameter("auto-sync", check);
            SetAutoSync();
        }

        private void MoveHostToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem item = listCache.GetItem(contentsList.SelectedIndices[0]); // Item under the mouse
            FileDetail detail = (FileDetail)item.Tag;

            string moveMsg = string.Format(Strings.AskHostMove, detail.Host, System.Environment.MachineName);
            DialogResult dialogResult = MessageBoxForm.Show(moveMsg, MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                directoryManager.MoveHost(detail.Host, System.Environment.MachineName);
                RefreshContentView();
            }
        }

        private void BtnDonate_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://www.paypal.com/donate/?hosted_button_id=8DY8ZCSBLQ3SN");
            }
            catch (Exception ex)
            {
                MessageBoxForm.Show("Can't open the PayPal web site " + ex.Message);
            }

        }

        private int timerCount;

        private void StartShake()
        {
            if (timerCount != 0)
            {
                return;
            }

            timerCount = 0;
            ShakerTimer.Enabled = true;
            ShakerTimer.Start();
        }

        private void ShakerTimer_Tick(object sender, EventArgs e)
        {
            const int SHAKE_SIZE = 1;

            if ((timerCount & 1) == 0)
            {
                btnDonate.Top += SHAKE_SIZE;
            }
            else
            {
                btnDonate.Top -= SHAKE_SIZE;
            }

            timerCount += 1;
            if (timerCount >= 12) // Make it even so the buton resets to home location
            {
                ShakerTimer.Stop();
                timerCount = 0;
            }

        }

        private void BtnDonate_MouseEnter(object sender, EventArgs e)
        {
            StartShake();
        }

        private void NagTimer_Tick(object sender, EventArgs e)
        {
            StartShake();
        }

        private void ViewType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var index = ViewType.SelectedIndex.ToString();
                var updated = directoryManager.DB.UpdateParameter("view-type", index);
                if (!updated)
                {
                    MessageBoxForm.Show(Strings.UpdateViewTypeFailed);
                }
            }
            catch
            {
                MessageBoxForm.Show(Strings.UpdateViewTypeFailed);
            }
            SetListViewType();
        }

        private void CurrentDirectory_Click(object sender, EventArgs e)
        {

        }
    }
}

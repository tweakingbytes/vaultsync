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

namespace VaultSync
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.syncList = new System.Windows.Forms.ListView();
            this.name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.type = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.host = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabControl = new System.Windows.Forms.TabControl();
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.contextExtract = new System.Windows.Forms.ToolStripMenuItem();
            this.contextEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.contextIgnore = new System.Windows.Forms.ToolStripMenuItem();
            this.contextDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.moveHostToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabVault = new System.Windows.Forms.TabPage();
            this.contentsList = new System.Windows.Forms.ListView();
            this.path = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.mod = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.size = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.fromHost = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabSync = new System.Windows.Forms.TabPage();
            this.tabIgnore = new System.Windows.Forms.TabPage();
            this.ignoreList = new System.Windows.Forms.ListView();
            this.columnPattern = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panelForTab = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.CurrentDirectory = new System.Windows.Forms.Label();
            this.navigationStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripUpLevel = new System.Windows.Forms.ToolStripButton();
            this.toolStripUp = new System.Windows.Forms.ToolStripButton();
            this.toolStripSyncButton = new System.Windows.Forms.ToolStripButton();
            this.toolExtract = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolAddItem = new System.Windows.Forms.ToolStripButton();
            this.toolAddFolder = new System.Windows.Forms.ToolStripButton();
            this.toolDelete = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripChangePwd = new System.Windows.Forms.ToolStripButton();
            this.AutoSync = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.ViewType = new System.Windows.Forms.ToolStripComboBox();
            this.toolUpdate = new System.Windows.Forms.ToolStripButton();
            this.toolHelp = new System.Windows.Forms.ToolStripButton();
            this.panelPanelForPwd = new System.Windows.Forms.Panel();
            this.btnDonate = new System.Windows.Forms.Button();
            this.startUpTimer = new System.Windows.Forms.Timer(this.components);
            this.ShakerTimer = new System.Windows.Forms.Timer(this.components);
            this.NagTimer = new System.Windows.Forms.Timer(this.components);
            this.tabControl.SuspendLayout();
            this.contextMenu.SuspendLayout();
            this.tabVault.SuspendLayout();
            this.tabSync.SuspendLayout();
            this.tabIgnore.SuspendLayout();
            this.panelForTab.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.navigationStrip.SuspendLayout();
            this.panelPanelForPwd.SuspendLayout();
            this.SuspendLayout();
            // 
            // syncList
            // 
            this.syncList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.name,
            this.type,
            this.host});
            resources.ApplyResources(this.syncList, "syncList");
            this.syncList.FullRowSelect = true;
            this.syncList.HideSelection = false;
            this.syncList.Name = "syncList";
            this.syncList.ShowGroups = false;
            this.syncList.Sorting = System.Windows.Forms.SortOrder.Descending;
            this.syncList.UseCompatibleStateImageBehavior = false;
            this.syncList.View = System.Windows.Forms.View.Details;
            this.syncList.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.SyncList_ColumnClick);
            this.syncList.SelectedIndexChanged += new System.EventHandler(this.FileList_SelectedIndexChanged);
            // 
            // name
            // 
            resources.ApplyResources(this.name, "name");
            // 
            // type
            // 
            resources.ApplyResources(this.type, "type");
            // 
            // host
            // 
            resources.ApplyResources(this.host, "host");
            // 
            // tabControl
            // 
            this.tabControl.AllowDrop = true;
            this.tabControl.ContextMenuStrip = this.contextMenu;
            this.tabControl.Controls.Add(this.tabVault);
            this.tabControl.Controls.Add(this.tabSync);
            this.tabControl.Controls.Add(this.tabIgnore);
            resources.ApplyResources(this.tabControl, "tabControl");
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Selected += new System.Windows.Forms.TabControlEventHandler(this.TabControl_Selected);
            this.tabControl.DragDrop += new System.Windows.Forms.DragEventHandler(this.TabControl_DragDrop);
            this.tabControl.DragEnter += new System.Windows.Forms.DragEventHandler(this.TabControl_DragEnter);
            this.tabControl.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TabControl_KeyDown);
            // 
            // contextMenu
            // 
            this.contextMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contextExtract,
            this.contextEdit,
            this.contextIgnore,
            this.contextDelete,
            this.moveHostToolStripMenuItem});
            this.contextMenu.Name = "contextMenu";
            resources.ApplyResources(this.contextMenu, "contextMenu");
            // 
            // contextExtract
            // 
            this.contextExtract.Name = "contextExtract";
            resources.ApplyResources(this.contextExtract, "contextExtract");
            this.contextExtract.Click += new System.EventHandler(this.ContextExtract_Click);
            // 
            // contextEdit
            // 
            this.contextEdit.Name = "contextEdit";
            resources.ApplyResources(this.contextEdit, "contextEdit");
            this.contextEdit.Click += new System.EventHandler(this.ContextEdit_Click);
            // 
            // contextIgnore
            // 
            this.contextIgnore.Name = "contextIgnore";
            resources.ApplyResources(this.contextIgnore, "contextIgnore");
            this.contextIgnore.Click += new System.EventHandler(this.ContextIgnore_Click);
            // 
            // contextDelete
            // 
            this.contextDelete.Name = "contextDelete";
            resources.ApplyResources(this.contextDelete, "contextDelete");
            this.contextDelete.Click += new System.EventHandler(this.ContentDelete_Click);
            // 
            // moveHostToolStripMenuItem
            // 
            this.moveHostToolStripMenuItem.Name = "moveHostToolStripMenuItem";
            resources.ApplyResources(this.moveHostToolStripMenuItem, "moveHostToolStripMenuItem");
            this.moveHostToolStripMenuItem.Click += new System.EventHandler(this.MoveHostToolStripMenuItem_Click);
            // 
            // tabVault
            // 
            this.tabVault.Controls.Add(this.contentsList);
            resources.ApplyResources(this.tabVault, "tabVault");
            this.tabVault.Name = "tabVault";
            this.tabVault.UseVisualStyleBackColor = true;
            // 
            // contentsList
            // 
            this.contentsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.path,
            this.mod,
            this.size,
            this.fromHost});
            this.contentsList.ContextMenuStrip = this.contextMenu;
            resources.ApplyResources(this.contentsList, "contentsList");
            this.contentsList.FullRowSelect = true;
            this.contentsList.HideSelection = false;
            this.contentsList.Name = "contentsList";
            this.contentsList.Sorting = System.Windows.Forms.SortOrder.Descending;
            this.contentsList.UseCompatibleStateImageBehavior = false;
            this.contentsList.View = System.Windows.Forms.View.Details;
            this.contentsList.VirtualMode = true;
            this.contentsList.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.ListContentsView_ColumnClick);
            this.contentsList.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.ContentsList_RetrieveVirtualItem);
            this.contentsList.SelectedIndexChanged += new System.EventHandler(this.ListContentsView_SelectedIndexChanged);
            this.contentsList.VirtualItemsSelectionRangeChanged += new System.Windows.Forms.ListViewVirtualItemsSelectionRangeChangedEventHandler(this.FileList_VirtualItemsSelectionRangeChanged);
            this.contentsList.DoubleClick += new System.EventHandler(this.ListContentsView_DoubleClick);
            // 
            // path
            // 
            resources.ApplyResources(this.path, "path");
            // 
            // mod
            // 
            resources.ApplyResources(this.mod, "mod");
            // 
            // size
            // 
            resources.ApplyResources(this.size, "size");
            // 
            // fromHost
            // 
            resources.ApplyResources(this.fromHost, "fromHost");
            // 
            // tabSync
            // 
            this.tabSync.Controls.Add(this.syncList);
            resources.ApplyResources(this.tabSync, "tabSync");
            this.tabSync.Name = "tabSync";
            this.tabSync.UseVisualStyleBackColor = true;
            // 
            // tabIgnore
            // 
            this.tabIgnore.Controls.Add(this.ignoreList);
            resources.ApplyResources(this.tabIgnore, "tabIgnore");
            this.tabIgnore.Name = "tabIgnore";
            this.tabIgnore.UseVisualStyleBackColor = true;
            // 
            // ignoreList
            // 
            this.ignoreList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnPattern});
            resources.ApplyResources(this.ignoreList, "ignoreList");
            this.ignoreList.FullRowSelect = true;
            this.ignoreList.HideSelection = false;
            this.ignoreList.Name = "ignoreList";
            this.ignoreList.ShowGroups = false;
            this.ignoreList.Sorting = System.Windows.Forms.SortOrder.Descending;
            this.ignoreList.UseCompatibleStateImageBehavior = false;
            this.ignoreList.View = System.Windows.Forms.View.Details;
            this.ignoreList.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.ListIgnore_ColumnClick);
            this.ignoreList.SelectedIndexChanged += new System.EventHandler(this.ListIgnore_SelectedIndexChanged);
            this.ignoreList.DoubleClick += new System.EventHandler(this.IgnoreList_DoubleClick);
            // 
            // columnPattern
            // 
            resources.ApplyResources(this.columnPattern, "columnPattern");
            // 
            // panelForTab
            // 
            resources.ApplyResources(this.panelForTab, "panelForTab");
            this.panelForTab.Controls.Add(this.panel2);
            this.panelForTab.Controls.Add(this.panel1);
            this.panelForTab.Name = "panelForTab";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tabControl);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.CurrentDirectory);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // CurrentDirectory
            // 
            resources.ApplyResources(this.CurrentDirectory, "CurrentDirectory");
            this.CurrentDirectory.Name = "CurrentDirectory";
            this.CurrentDirectory.Click += new System.EventHandler(this.CurrentDirectory_Click);
            // 
            // navigationStrip
            // 
            this.navigationStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.navigationStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripUpLevel,
            this.toolStripUp,
            this.toolStripSyncButton,
            this.toolExtract,
            this.toolStripSeparator1,
            this.toolAddItem,
            this.toolAddFolder,
            this.toolDelete,
            this.toolStripSeparator2,
            this.toolStripChangePwd,
            this.AutoSync,
            this.toolStripSeparator3,
            this.ViewType,
            this.toolUpdate,
            this.toolHelp});
            resources.ApplyResources(this.navigationStrip, "navigationStrip");
            this.navigationStrip.Name = "navigationStrip";
            // 
            // toolStripUpLevel
            // 
            this.toolStripUpLevel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripUpLevel, "toolStripUpLevel");
            this.toolStripUpLevel.Name = "toolStripUpLevel";
            this.toolStripUpLevel.Click += new System.EventHandler(this.ToolStripUpLevel_Click);
            // 
            // toolStripUp
            // 
            this.toolStripUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripUp, "toolStripUp");
            this.toolStripUp.Name = "toolStripUp";
            this.toolStripUp.Click += new System.EventHandler(this.ToolStripUpButton_Click);
            // 
            // toolStripSyncButton
            // 
            this.toolStripSyncButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripSyncButton, "toolStripSyncButton");
            this.toolStripSyncButton.Name = "toolStripSyncButton";
            this.toolStripSyncButton.Click += new System.EventHandler(this.ToolStripSyncButton_Click);
            // 
            // toolExtract
            // 
            this.toolExtract.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolExtract, "toolExtract");
            this.toolExtract.Name = "toolExtract";
            this.toolExtract.Click += new System.EventHandler(this.ToolExtract_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // toolAddItem
            // 
            this.toolAddItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolAddItem, "toolAddItem");
            this.toolAddItem.Name = "toolAddItem";
            this.toolAddItem.Click += new System.EventHandler(this.ToolAddFile_Click);
            // 
            // toolAddFolder
            // 
            this.toolAddFolder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolAddFolder, "toolAddFolder");
            this.toolAddFolder.Name = "toolAddFolder";
            this.toolAddFolder.Click += new System.EventHandler(this.ToolAddFolder_Click);
            // 
            // toolDelete
            // 
            this.toolDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolDelete, "toolDelete");
            this.toolDelete.Name = "toolDelete";
            this.toolDelete.Click += new System.EventHandler(this.ToolDelete_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // toolStripChangePwd
            // 
            this.toolStripChangePwd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripChangePwd, "toolStripChangePwd");
            this.toolStripChangePwd.Name = "toolStripChangePwd";
            this.toolStripChangePwd.Click += new System.EventHandler(this.ToolStripChangePwd_Click);
            // 
            // AutoSync
            // 
            this.AutoSync.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.AutoSync, "AutoSync");
            this.AutoSync.Name = "AutoSync";
            this.AutoSync.Click += new System.EventHandler(this.OnAutoSyncClick);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            // 
            // ViewType
            // 
            resources.ApplyResources(this.ViewType, "ViewType");
            this.ViewType.AutoToolTip = true;
            this.ViewType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ViewType.Items.AddRange(new object[] {
            resources.GetString("ViewType.Items"),
            resources.GetString("ViewType.Items1"),
            resources.GetString("ViewType.Items2")});
            this.ViewType.Name = "ViewType";
            this.ViewType.SelectedIndexChanged += new System.EventHandler(this.ViewType_SelectedIndexChanged);
            // 
            // toolUpdate
            // 
            this.toolUpdate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolUpdate, "toolUpdate");
            this.toolUpdate.Name = "toolUpdate";
            this.toolUpdate.Click += new System.EventHandler(this.ToolUpdate_Click);
            // 
            // toolHelp
            // 
            this.toolHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolHelp, "toolHelp");
            this.toolHelp.Name = "toolHelp";
            this.toolHelp.Click += new System.EventHandler(this.ToolHelp_Click);
            // 
            // panelPanelForPwd
            // 
            resources.ApplyResources(this.panelPanelForPwd, "panelPanelForPwd");
            this.panelPanelForPwd.Controls.Add(this.btnDonate);
            this.panelPanelForPwd.Controls.Add(this.navigationStrip);
            this.panelPanelForPwd.Name = "panelPanelForPwd";
            // 
            // btnDonate
            // 
            resources.ApplyResources(this.btnDonate, "btnDonate");
            this.btnDonate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.btnDonate.Name = "btnDonate";
            this.btnDonate.UseVisualStyleBackColor = false;
            this.btnDonate.Click += new System.EventHandler(this.BtnDonate_Click);
            this.btnDonate.MouseEnter += new System.EventHandler(this.BtnDonate_MouseEnter);
            // 
            // startUpTimer
            // 
            this.startUpTimer.Interval = 1000;
            this.startUpTimer.Tick += new System.EventHandler(this.StartUpTimer_Tick);
            // 
            // ShakerTimer
            // 
            this.ShakerTimer.Interval = 60;
            this.ShakerTimer.Tick += new System.EventHandler(this.ShakerTimer_Tick);
            // 
            // NagTimer
            // 
            this.NagTimer.Enabled = true;
            this.NagTimer.Interval = 60000;
            this.NagTimer.Tick += new System.EventHandler(this.NagTimer_Tick);
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelForTab);
            this.Controls.Add(this.panelPanelForPwd);
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.tabControl.ResumeLayout(false);
            this.contextMenu.ResumeLayout(false);
            this.tabVault.ResumeLayout(false);
            this.tabSync.ResumeLayout(false);
            this.tabIgnore.ResumeLayout(false);
            this.panelForTab.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.navigationStrip.ResumeLayout(false);
            this.navigationStrip.PerformLayout();
            this.panelPanelForPwd.ResumeLayout(false);
            this.panelPanelForPwd.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListView syncList;
        private System.Windows.Forms.ColumnHeader name;
        private System.Windows.Forms.ColumnHeader host;
        private System.Windows.Forms.ColumnHeader type;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabSync;
        private System.Windows.Forms.TabPage tabVault;
        private System.Windows.Forms.ListView contentsList;
        private System.Windows.Forms.ColumnHeader path;
        private System.Windows.Forms.ColumnHeader mod;
        private System.Windows.Forms.ColumnHeader size;
        private System.Windows.Forms.Panel panelForTab;
        private System.Windows.Forms.ToolStrip navigationStrip;
        private System.Windows.Forms.ToolStripButton toolStripUpLevel;
        private System.Windows.Forms.ToolStripButton toolStripUp;
        private System.Windows.Forms.ToolStripButton toolStripSyncButton;
        private System.Windows.Forms.Panel panelPanelForPwd;
        private System.Windows.Forms.ColumnHeader fromHost;
        private System.Windows.Forms.ToolStripButton toolStripChangePwd;
        private System.Windows.Forms.ToolStripButton toolExtract;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolAddItem;
        private System.Windows.Forms.ToolStripButton toolAddFolder;
        private System.Windows.Forms.ToolStripButton toolDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.TabPage tabIgnore;
        private System.Windows.Forms.ListView ignoreList;
        private System.Windows.Forms.ColumnHeader columnPattern;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem contextDelete;
        private System.Windows.Forms.ToolStripMenuItem contextIgnore;
        private System.Windows.Forms.ToolStripMenuItem contextExtract;
        private System.Windows.Forms.ToolStripMenuItem contextEdit;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolHelp;
        private System.Windows.Forms.ToolStripButton toolUpdate;
        private System.Windows.Forms.Timer startUpTimer;
        private System.Windows.Forms.ToolStripButton AutoSync;
        private System.Windows.Forms.ToolStripMenuItem moveHostToolStripMenuItem;
        private System.Windows.Forms.Button btnDonate;
        private System.Windows.Forms.Timer ShakerTimer;
        private System.Windows.Forms.Timer NagTimer;
        private System.Windows.Forms.ToolStripComboBox ViewType;
        private System.Windows.Forms.Label CurrentDirectory;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
    }
}


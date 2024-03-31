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
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Threading;
using System.Drawing;

namespace VaultSync
{

    // This class wraps all the directory and file operations
    public class DirectoryManagement
    {
        private string password;
        private readonly string vaultLocation;
        private readonly string DBName;
        private readonly string tempDBName;
        private readonly string root;
        private readonly string VALIDATION_PARAM = "validation_start";
        private DataConnector db;
        private RijndaelManaged encryptionKey;
        private IgnorePatternMatcher ignoreMatcher;
        private Task validationTask;

        enum ApplyState { Ask, YesAll, Yes, No, NoAll, Cancel };
        private ApplyState applyState;
        private string validationStartPoint;
        private bool fastForwardValidation;
        private readonly long HEADROOM = 1024 * 1024 * 5; // Keep at least 5MB free on the device

        public string TempDBName { get => tempDBName;  }
        public DataConnector DB { get => db; }

        private Int64 Counter { get; set; }

        public Action<string> Progress { get; set; }
        public string ExtractFolder { get; set; }

        public DirectoryManagement(string password, string vaultLocation)
        {
            Abort = false;
            this.password = password;
            this.vaultLocation = vaultLocation;
            DBName = Path.Combine(vaultLocation, "index.dat");
            tempDBName = Path.GetTempFileName();
            root = Path.GetPathRoot(DBName);
        }

        public bool Create()
        {
            // Create a new vault directory and encryption control file. Open a temporary index database file.
            Directory.CreateDirectory(vaultLocation);
            encryptionKey = FileEncryption.KeyManagement(GenerateControlPath(), password);
            if (encryptionKey == null)
            {
                return false;
            }

            db = new DataConnector(tempDBName);
            db.CreateDBIfNecessary();
            db.UpdateSchema(); // handle schema changes
            db.DataChanged = true; // force database save on close even if no changes are made, otherwise the vault directory is incomplete
            return true;
        }

        public bool Open()
        {
            // Get the encryption key
            encryptionKey = FileEncryption.KeyManagement(GenerateControlPath(), password);
            if (encryptionKey == null)
            {
                return false;
            }

            FileEncryption.FileDecrypt(DBName, tempDBName, encryptionKey);
            db = new DataConnector(tempDBName);
            if (!db.IsValid())
            {
                db.Close();
                db = null;
                DeleteTempDB();
                return false;
            }
            db.UpdateSchema(); // handle future schema changes
            validationTask = Task.Run(() => ValidateIndex());
            return true;
        }

        private void DeleteTempDB()
        {
            if (File.Exists(tempDBName))
            {
                File.Delete(tempDBName);
            }
        }

        public void Close()
        {
            if (db == null) return;

            Abort = true;
            if (validationTask != null)
            {
                validationTask.Wait();
            }

            try
            {
                db.Vacuum();
                db.Close(); // Force the database engine to release the file so it can be moved

                if (db.DataChanged)
                {
                    SaveNewDatabase();
                }
                DeleteTempDB();
            }
            catch (Exception e)
            {
                MessageBoxForm.Show("Problem with close: " + e.Message);
            }
        }

        private void SaveNewDatabase()
        {
            // Exchange the encrypted database for the new one, keeping the most recent as a backup

            string newDB = DBName + "1";
            if (File.Exists(newDB))
            {
                File.Delete(newDB); // file should not exist but it will if the last save had an exception
            }

            try
            {
                FileEncryption.FileEncrypt(tempDBName, newDB, encryptionKey);
            }
            catch (Exception)
            {
                // File still hasn't been released. Try another garbage collect and wait a bit
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Thread.Sleep(200);
                FileEncryption.FileEncrypt(tempDBName, newDB, encryptionKey);
            }

            // Rename the current database as the backup, removing any previous version
            if (File.Exists(DBName))
            {
                string backupName = Path.ChangeExtension(DBName, "bak");
                File.Delete(backupName);
                File.Move(DBName, backupName);
            }

            // Replace the newly vacated database name with the new one and remove the temporary
            File.Move(newDB, DBName);
            File.Delete(tempDBName);
        }

        private long GetDBFileSize()
        {
            FileInfo fi = new FileInfo(tempDBName);
            return fi.Length;
        }

        private bool IsInsufficientSpace(long fileSize)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name.Equals(root))
                {
                    // Note: The old index is already on the disk and is kept for safety, so only need to account for the size of new index
                    if ((drive.AvailableFreeSpace - GetDBFileSize() - fileSize) < HEADROOM)
                    {
                        MessageBox.Show(Strings.OutOfSpace);
                        Abort = true;
                        return true;
                    }
                    break;
                }
            }
            return false;
        }

        public bool Abort { get ; set ; }

        public void SyncFiles()
        {
            SetupIgnoreMatcher();
            LoadSyncNames(SyncAction, SyncFile);
        }

        public Int64 CountSyncFiles()
        {
            SetupIgnoreMatcher();
            Counter = 0;
            LoadSyncNames(SyncAction, Count);
            return Counter;
        }

        public void Count(string path)
        {
            Counter += 1;
        }

        private void SyncAction(string host, string path, DataConnector.SyncType type, Action<string> job)
        {
            if (type == DataConnector.SyncType.Folder)
            {
                SyncDirectory(path, job);
            }
            else
            {
                if (ignoreMatcher.IgnoreFile(path)) return;

                Progress?.Invoke(path);
                job(path);
            }
        }

        private void SyncDirectory(string root, Action<string> job)
        {
            EnumerateDirectory(false, root, job);
        }

        private void EnumerateDirectory(bool silent, string root, Action<string> job)
        {
            try
            {
                FileInfo fi = new FileInfo(root);
                FileAttributes fa = fi.Attributes;
                if ((fa & (FileAttributes.System | FileAttributes.ReparsePoint)) != 0 
                    || (ignoreMatcher != null && ignoreMatcher.IgnoreFile(root)))
                {
                    return; // Skip this directory
                }

                EnumerateFiles(silent, root, job);
                foreach (string dir in Directory.EnumerateDirectories(root))
                {
                    if (Abort) return;
                    EnumerateDirectory(silent, dir, job);
                }
            }
            catch (Exception e)
            {
                FileInfo fi = new FileInfo(root);
                FileAttributes fa = fi.Attributes;
                MessageBoxForm.Show(string.Format(Strings.EnumerateDirectoryFailed, root, e.Message, fa.ToString()));
            }
        }

        private void EnumerateFiles(bool silent, string dir, Action<string> job)
        {
            try
            {
                foreach (string path in Directory.EnumerateFiles(dir))
                {
                    if (Abort) return;
                    if (ignoreMatcher != null &&  ignoreMatcher.IgnoreFile(path)) continue;

                    if (!silent)
                    {
                        Progress?.Invoke(path);
                    }
                    job(path);
                }
            }
            catch (Exception e)
            {
                FileInfo fi = new FileInfo(dir);
                FileAttributes fa = fi.Attributes;
                MessageBoxForm.Show(string.Format(Strings.EnumerateDirectoryFailed, dir, e.Message, fa.ToString()));
            }
        }

        public void SyncFile(string path)
        {
            FileInfo fi;
            try
            {
                fi = new FileInfo(path);
                if (IsInsufficientSpace(fi.Length))
                {
                    return;
                }
            }
            catch (FileNotFoundException)
            {
                // Ignore missing files
                return;
            }

            FileDetail fd = db.GetFileDetails(path);
            if (fd == null)
            {
 
                fd = new FileDetail(path, Guid.NewGuid().ToString(), fi.Length, fi.CreationTimeUtc.ToFileTimeUtc(), fi.LastWriteTimeUtc.ToFileTimeUtc(), db.FindOrCreatParent(path), System.Environment.MachineName);
                try
                {
                    db.InsertFileDetail(fd);
                    EncryptFile(path, fd.EncryptedName);
                }
                catch (Exception e)
                {
                    MessageBoxForm.Show(string.Format(Strings.SyncFailure, path, e.Message));
                    DeleteFile(path, fd.EncryptedName, System.Environment.MachineName);
                }
            }
            else
            {
                if (fd.Size != fi.Length || fd.ModDate != fi.LastWriteTimeUtc.ToFileTimeUtc())
                {
                    try
                    {
                        fd.Size = fi.Length;
                        fd.ModDate = fi.LastWriteTimeUtc.ToFileTimeUtc();

                        db.UpdateFileDetail(fd);
                        EncryptFile(path, fd.EncryptedName);
                    }
                    catch (Exception e)
                    {
                        MessageBoxForm.Show(string.Format(Strings.SyncFailure, path, e.Message));
                    }
                }
            }
        }
        
        private void EncryptFile(string path, string encryptedName)
        {
            string encPath = GenerateEncryptedFilePath(encryptedName);
            FileEncryption.FileEncrypt(path, encPath, encryptionKey);
        }

        //private void DecryptFile(string encryptedName, string path)
        //{
        //    string encPath = GenerateEncryptedFilePath(encryptedName);
        //    FileEncryption.FileDecrypt(encPath, path, encryptionKey);
        //}

        private void DeleteFile(string path, string host, string encryptedName)
        {
            if (Abort) return;

            // File delete         
            Progress?.Invoke(path);
            File.Delete(GenerateEncryptedFilePath(encryptedName));
            db.DeleteFile(path, host);
        }

        private void ValidationAction(string path)
        {
            if (Abort) return;

            if (Path.GetExtension(path).Length > 0)
            {
                return; // Skip control files. Data files have no extension.
            }

            string encryptedName = Path.GetFileName(path);

            if (fastForwardValidation && !validationStartPoint.Equals(encryptedName))
            {
                return; // Skip until start point reached
            }

            fastForwardValidation = false;

            if (!db.IndexExists(encryptedName))
            {
                File.Delete(path); // file is not indexed so delete it
            }

            validationStartPoint = encryptedName;
        }

        public void ValidateIndex()
        {
            validationStartPoint = db.GetParameter(VALIDATION_PARAM);
            if (validationStartPoint == null)
            {
                validationStartPoint = "";
            }
            fastForwardValidation = !string.IsNullOrEmpty(validationStartPoint);

            EnumerateDirectory(true, vaultLocation, ValidationAction);

            if (!Abort)
            {
                // Clean finish of validation
                validationStartPoint = "";
            }
            db.UpdateParameter(VALIDATION_PARAM, validationStartPoint);
        }

        private string GenerateEncryptedFilePath(string encryptedName)
        {
            string encDir = Path.Combine(vaultLocation, encryptedName.Substring(0, 2));
            if (!Directory.Exists(encDir))
            {
                Directory.CreateDirectory(encDir);
            }

            string encPath = Path.Combine(encDir, encryptedName);
            return encPath;
        }

        private string GenerateControlPath()
        {
            return Path.Combine(vaultLocation, "control.dat");
        }

        private string GenerateExtractPath(FileDetail item)
        {
            if (string.IsNullOrEmpty(ExtractFolder))
            {
                return item.Path;
            }
            return Path.Combine(ExtractFolder, RemoveRoot(item.Path));
        }
        

        public void LoadSyncNames(System.Action<string, string, DataConnector.SyncType, Action<string>> action, Action<string> job)
        {
            db.ListSyncPoints(action, job);
        }

        public bool InsertSyncPoint(string host, string path, DataConnector.SyncType type)
        {
            return db.InsertSyncPoint(host, path, type);
        }

        public void DeleteSyncPoint(string host, string path)
        {
            db.DeleteSyncPoint(host, path);
        }

        public void DeleteFile(FileDetail item)
        {
            if (item.IsFolder)
            {
                // Directory delete
                if (Abort) return;

                db.ListContents(DeleteFile, item.Parent, "");
            }
            else
            {
                // File delete          
                DeleteFile(item.Path, item.Host, item.EncryptedName);
            }
        }

        public void ExtractFiles(List<FileDetail> items)
        {
            applyState = ApplyState.Ask;
            foreach (FileDetail item in items)
            {
                ExtractFile(item);
            }
        }

        public void ExtractFile(FileDetail item)
        {
            if (Abort) return;

            if (item.IsFolder)
            {
                // Directory extract
                db.ListContents(ExtractFile, item.Parent, "");
            }
            else
            {
                ExtractTheFile(item);
            }
        }

        private void ExtractTheFile(FileDetail item)
        {
            if (!CheckExtractionOk(item)) return;

            // File extract
            string extractFolder = GenerateExtractPath(item);

            Directory.CreateDirectory(Path.GetDirectoryName(extractFolder));

            Progress?.Invoke(item.Path);
            FileEncryption.FileDecrypt(GenerateEncryptedFilePath(item.EncryptedName), extractFolder, encryptionKey);

            // Restore the file date/time
            if (File.Exists(extractFolder))
            {
                DateTime createDate = DateTime.FromFileTime(item.CreateDate);
                DateTime modDate = DateTime.FromFileTime(item.ModDate);
                if (item.CreateDate != 0)
                {
                    File.SetCreationTimeUtc(extractFolder, createDate);
                }
                else
                {
                    File.SetCreationTimeUtc(extractFolder, modDate);
                }
                File.SetLastWriteTimeUtc(extractFolder, modDate);
            }
        }

        private bool CheckExtractionOk(FileDetail item)
        {
            if (item.Host != System.Environment.MachineName)
            {
                switch (applyState)
                {
                    case ApplyState.Ask:
                    case ApplyState.Yes:
                    case ApplyState.No:
                        SetApplySate(item);
                        break;

                    default:
                        break;
                }

                switch (applyState)
                {
                    case ApplyState.Cancel:
                        Abort = true;
                        return false;

                    case ApplyState.No:
                    case ApplyState.NoAll:
                        Progress?.Invoke(item.Path);
                        return false;

                    case ApplyState.Yes:
                    case ApplyState.YesAll:
                    case ApplyState.Ask:
                        return true;
                }
            }
            return true;
        }

        private void SetApplySate(FileDetail item)
        {
            CheckMachineForm check = new CheckMachineForm
            {
                Path = item.Path,
                Host = item.Host
            };
            DialogResult result = check.ShowDialog();

            switch (result)
            {
                case DialogResult.Yes:
                    if (check.ApplyAll)
                    {
                        applyState = ApplyState.YesAll;
                    }
                    else
                    {
                        applyState = ApplyState.Yes;
                    }
                    break;

                case DialogResult.No:
                    if (check.ApplyAll)
                    {
                        applyState = ApplyState.NoAll;
                    }
                    else
                    {
                        applyState = ApplyState.No;
                    }
                    break;

                case DialogResult.Cancel:
                    Abort = true;
                    applyState = ApplyState.Cancel;
                    break;
           }

        }

        public Int64 CountSelectedFiles
            (List<FileDetail> items)
        {
            Counter = 0;
            foreach (FileDetail item in items)
            {
                CountSelectedFile(item);
            }
            return Counter;
        }

        private void CountSelectedFile(FileDetail item)
        {
            if (item.IsFolder)
            {
                // Directory count
                db.ListContents(CountSelectedFile, item.Parent, "");
            }
            else
            {
                Counter += 1;
            }
        }

        public void DeleteFiles(List<FileDetail> items)
        {
            foreach (FileDetail item in items)
            {
                if (Abort) return;
                DeleteFile(item);
            }
            db.DeleteChildlessFolders();
        }

        public void LoadContent(System.Action<FileDetail> action)
        {
            db.ListContents(action, 0, "");
        }

        public void LoadIgnore(System.Action<string> action)
        {
            db.ListIgnoreItems(action);
        }

        public void LoadContent(System.Action<FileDetail> action, Int64 parent, string currentText)
        {
            db.ListContents(action, parent, currentText);
        }

        public bool InsertIgnoreItem(string pattern)
        {
            ResetIgnoreMatcher();
            return db.InsertIgnoreItem(pattern);
        }

        public bool DeleteIgnorePattern(string pattern)
        {
            ResetIgnoreMatcher();
            return db.DeleteIgnorePattern(pattern);
        }

        private void SetupIgnoreMatcher()
        {
            if (ignoreMatcher == null)
            {
                ignoreMatcher = new IgnorePatternMatcher();
                ignoreMatcher.LoadIgnoreList(db);
            }
        }

        private void ResetIgnoreMatcher()
        {
            ignoreMatcher = null;
        }

        public Int64 FindOrCreatParent(string path)
        {
            return db.FindOrCreatParent(path);
        }

        public string RemoveLastFolder(string path)
        {
            int index = path.LastIndexOf(Path.DirectorySeparatorChar);
            if (index < 0)
            {
                return "";
            }
            return path.Substring(0, index);
        }

        private string RemoveRoot(string path)
        {
            string root = Directory.GetDirectoryRoot(path);
            if (string.IsNullOrEmpty(root))
            {
                return path;
            }

            return path.Substring(root.Length);
        }

        public void ChangePassword(string newPwd)
        {
            RijndaelManaged aes = FileEncryption.ChangePassword(GenerateControlPath(), password, newPwd);

            if (aes != null)
            {
                password = newPwd;
                encryptionKey = aes;
            }
        }

        public void ShowFile(FileDetail item)
        {
            // Create a temp file but keep the extension from the original file
            string tempFile = Path.GetTempFileName();
            string typedFile = Path.ChangeExtension(tempFile, Path.GetExtension(item.Path));
            File.Move(tempFile, typedFile);

            FileEncryption.FileDecrypt(GenerateEncryptedFilePath(item.EncryptedName), typedFile, encryptionKey);

            Task.Run(() => StartProcessForShow(typedFile) );
        }

        private static async void StartProcessForShow(string typedFile)
        {
            try
            {
                Process p = Process.Start(typedFile);
                if (p != null)
                {
                    // Windows started a new process to view the file.
                    // Wait for it to terminate and delete the temp file.

                    p.WaitForExit();
                    File.Delete(typedFile);
                }
                else
                {
                    // Windows has reused an existing process to view the file and we don't have a reference. 
                    // Periodically try to delete the temp file

                    await Task.Delay(2000); // Give the process time to load the file
                    while (true)
                    {
                        await Task.Delay(500);
                        try
                        {
                            File.Delete(typedFile);
                            break;
                        }
                        catch (Exception)
                        {
                            // Ignore the exception
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBoxForm.Show(string.Format("Error showing the file: {0}", e.Message));
            }
        }

        internal void MoveHost(string oldHost, string newHost)
        {
            DB.MoveHost(oldHost, newHost);
        }

        public void GetThumbnail(FileDetail item, Action<FileDetail, Bitmap> onLoaded)
        {
            // Create a temp file but keep the extension from the original file
            string tempFile = Path.GetTempFileName();
            string typedFile = Path.ChangeExtension(tempFile, Path.GetExtension(item.Path));
            File.Move(tempFile, typedFile);

            FileEncryption.FileDecrypt(GenerateEncryptedFilePath(item.EncryptedName), typedFile, encryptionKey);

            Task.Run(() => AskShellForThumbnail(item, typedFile, onLoaded));
        }

        private static void AskShellForThumbnail(FileDetail item, string typedFile, Action<FileDetail, Bitmap> onLoaded)
        {
            try
            {
                var shellFile = Microsoft.WindowsAPICodePack.Shell.ShellFile.FromFilePath(typedFile);
                var largeThumb = shellFile.Thumbnail.LargeBitmap;
                onLoaded(item, largeThumb);
            }
            catch (Exception e)
            {
                MessageBoxForm.Show(string.Format("Error getting a thumbnail: {0}", e.Message));
            }
        }
    }
}

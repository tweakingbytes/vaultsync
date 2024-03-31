using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Setup
{
    public partial class SetupForm : Form
    {
        private const string ErrorCaption = "VaultSync instalation problem";
        private const string CompletionMessage = "Installation is complete";
        private const string vaultSync = "VaultSync";
        private readonly string archivePath = Path.GetTempFileName();
        private readonly string installDir = "VaultSyncFiles";
        private readonly bool deleteExistingFiles = true;

        public SetupForm()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void InstallButton_Click(object sender, EventArgs e)
        {
            if (!installLocation.Text.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                installLocation.Text += Path.DirectorySeparatorChar;
            }


            InstallButton.Enabled = false;
            string root = Path.GetPathRoot(installLocation.Text).ToUpper();

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name.Equals(root))
                {
                    HandleExistingInstall();
                    ExtractArchive();
                    ExtractFiles();
                    CreateLink();
                    return;
                }
            }
            MessageBox.Show("Drive is not ready", ErrorCaption);

        }

        private void CreateLink()
        {
            // https://stackoverflow.com/questions/1501608/how-do-you-create-an-application-shortcut-lnk-file-in-c-sharp-with-command-li/1501727#1501727
            string filesPath = GetFilesPath();
            string exe = filesPath + "\\VaultSync.exe";

            IWshRuntimeLibrary.WshShell wsh = new IWshRuntimeLibrary.WshShell();
            IWshRuntimeLibrary.IWshShortcut shortcut = wsh.CreateShortcut(
               installLocation.Text + "\\VaultSync.lnk") as IWshRuntimeLibrary.IWshShortcut;
            shortcut.Arguments = "";
            shortcut.TargetPath = exe;
            shortcut.WindowStyle = 1;
            shortcut.Description = vaultSync;
            shortcut.WorkingDirectory = filesPath;
            shortcut.IconLocation = exe;
            shortcut.Save();
        }

        private string GetFilesPath()
        {
            return installLocation.Text + installDir;
        }

        private void ExtractArchive()
        {
            string exeName = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
            Uri fileUri = new Uri(exeName);
            string path = fileUri.LocalPath;

            using (FileStream fs = System.IO.File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte[] b = new byte[2048];

                long archiveLocation = 0;
                fs.Seek(-4, SeekOrigin.End);
                if (fs.Read(b, 0, 4) == 4)
                {
#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
                    archiveLocation = b[0] << 32;
                    archiveLocation |= b[1] << 16;
                    archiveLocation |= b[2] << 8;
                    archiveLocation |= b[3] << 8;
#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand

                    fs.Seek(archiveLocation, SeekOrigin.Begin);
                    using (FileStream archive = File.Open(archivePath, FileMode.Open, FileAccess.Write))
                    {
                        int bytes = 0;
                        while ((bytes = fs.Read(b, 0, b.Length)) > 0)
                        {
                            archive.Write(b, 0, bytes);
                        }
                    }
                }
            }

        }

        private void ExtractFiles()
        {
            try
            {
                string extractPath = GetFilesPath();

                // Normalizes the path.
                extractPath = Path.GetFullPath(extractPath);

                // Ensures that the last character on the extraction path is the directory separator char.
                // Without this, a malicious zip file could try to traverse outside of the expected extraction path.
                if (!extractPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                {
                    extractPath += Path.DirectorySeparatorChar;
                }

                using (ZipArchive archive = ZipFile.OpenRead(archivePath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        // Gets the full path to ensure that relative segments are removed.
                        string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));

                        if (entry.FullName.EndsWith("/"))
                        {
                            Directory.CreateDirectory(destinationPath);
                        }
                        else
                        {

                            // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
                            // are case-insensitive.
                            if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
                            {
                                if (deleteExistingFiles && File.Exists(destinationPath))
                                {
                                    File.Delete(destinationPath);
                                }
                                entry.ExtractToFile(destinationPath);
                            }
                        }
                    }
                }

                ErrorMessage.Text = CompletionMessage;
                ErrorMessage.Visible = true;
                InstallButton.Enabled = false;
                browseButton.Enabled = false;
                installLocation.Enabled = false;
                cancelButton.Text = "Close";
                File.Delete(archivePath);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("is denied")) {
                    MessageBox.Show("Installation problem: " + e.Message + "\n\nMaybe SecureVault still running?", ErrorCaption);
                }
                else {
                    MessageBox.Show("Installation problem: " + e.Message, ErrorCaption);
                }
                File.Delete(archivePath);
            }
        }

        private void HandleExistingInstall()
        {
            // If there is an existing install then move the vault
            string vaultDir = installLocation.Text + vaultSync;
            if (Directory.Exists(vaultDir))
            {
                string filesPath = GetFilesPath();
                if (!Directory.Exists(filesPath)) {
                    Directory.CreateDirectory(filesPath);
                }

                Directory.Move(vaultDir, filesPath + "\\" + vaultSync);
                string[] obsoleteFiles = { 
                    "runtimes",
                    "x64",
                    "x86",
                    "Help",
                    "Microsoft.Data.Sqlite.dll",
                    "Microsoft.Data.Sqlite.xml",
                    "SQLitePCLRaw.batteries_v2.dll",
                    "SQLitePCLRaw.core.dll",
                    "SQLitePCLRaw.nativelibrary.dll",
                    "SQLitePCLRaw.provider.dynamic_cdecl.dll",
                    "System.Buffers.dll",
                    "System.Buffers.xml",
                    "System.Memory.dll",
                    "System.Memory.xml",
                    "System.Numerics.Vectors.dll",
                    "System.Numerics.Vectors.xml",
                    "System.Runtime.CompilerServices.Unsafe.dll",
                    "System.Runtime.CompilerServices.Unsafe.xml",
                    "SQLite.Interop.dll",
                    "System.Data.SQLite.dll",
                    "VaultSync.exe",
                    "VaultSync.exe.config",
                    "VaultSync.pdb" };

                foreach (string name in obsoleteFiles)
                {
                    string path = installLocation.Text + name;
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                    else
                    {
                        File.Delete(path);
                    }
                }
            }
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog
            {
                SelectedPath = installLocation.Text
            };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                installLocation.Text = dlg.SelectedPath;
            }
        }

        private void SetupForm_Load(object sender, EventArgs e)
        {
             // Install on the first removable drive found
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveType == DriveType.Removable)
                {
                    installLocation.Text = drive.Name;
                    return;
                }
            }

            // If no removable drives are found then install on the first drive and set the warning that it isn't removable
            installLocation.Text = "C:\\VaultSync\\";
            ErrorMessage.Visible = true;
            return;
        }

        private void InstallLocation_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string root = Path.GetPathRoot(installLocation.Text).ToUpper();

                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    if (drive.Name.Equals(root))
                    {
                        ErrorMessage.Visible = drive.DriveType != DriveType.Removable;
                        return;
                    }
                }
                ErrorMessage.Visible = true;
            }
            catch (Exception)
            {
                // do nothing
            }
        }
     }
}

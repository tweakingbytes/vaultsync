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
using System.Security.Cryptography;

namespace VaultSync
{
    class FileEncryption
    {

        private static byte[] GenerateRandomSalt()
        {
            byte[] data = new byte[32];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                for (int i = 0; i < 10; i++)
                {
                    // Fill the buffer with the generated data
                    rng.GetBytes(data);
                }
            }

            return data;
        }

        public static void FileEncrypt(string inputFile, string outputFile, RijndaelManaged AES)
        {
            try
            {
                // create output file name
                using (FileStream fsCrypt = new FileStream(outputFile, FileMode.Create))
                {
                    using (CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {

                        using (FileStream fsIn = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {

                            // create a big buffer (1mb)
                            byte[] buffer = new byte[1048576];
                            int read;

                            while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                cs.Write(buffer, 0, read);
                            }
                        }
                    }
                }
            }
            catch (CryptographicException e)
            {
                MessageBoxForm.Show(Strings.CryptographicException + e);
            }
            catch (Exception ex)
            {
                MessageBoxForm.Show(Strings.Error + ex.Message);
            }
        }

        public static void FileDecrypt(string inputFile, string outputFile, RijndaelManaged AES)
        {
            try
            {
                using (FileStream fsCrypt = new FileStream(inputFile, FileMode.Open))
                {
                    using (CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read))
                    {

                        using (FileStream fsOut = new FileStream(outputFile, FileMode.Create))
                        {
                            int read;
                            byte[] buffer = new byte[1048576];

                            while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                fsOut.Write(buffer, 0, read);
                            }
                        }
                    }
                }
            }
            catch (CryptographicException e)
            {
                MessageBoxForm.Show(Strings.CryptographicException + e);
            }
            catch (Exception ex)
            {
                MessageBoxForm.Show(Strings.Error + ex.Message);
            }
        }

        public static byte[] StringEncrypt(byte[] inputString, RijndaelManaged AES)
        {
            using (ICryptoTransform encryptor = AES.CreateEncryptor())
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(inputString, 0, inputString.Length);
                        cs.FlushFinalBlock();
                        return ms.ToArray();
                    }
                }
            }
        }

        public static byte[] StringDecrypt(byte[] inputString, RijndaelManaged AES)
        {
            using (ICryptoTransform decryptor = AES.CreateDecryptor())
            {
                using (MemoryStream ms = new MemoryStream(inputString))
                {
                    using (CryptoStream reader = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        int decryptedByteCount = 0;
                        byte[] buffer = new byte[100];
                        decryptedByteCount = reader.Read(buffer, 0, buffer.Length);

                        byte[] decrypted = new byte[decryptedByteCount];

                        for (int i = 0; i < decryptedByteCount; ++i)
                        {
                            decrypted[i] = buffer[i];
                        }
                        return decrypted;
                    }
                }
            }
        }

        public static RijndaelManaged KeyManagement(string controlFile, string password)
        {
            if (File.Exists(controlFile))
            {
                return GetExistingKey(controlFile, password);
            }

            return CreateNewKey(controlFile, password);
        }

        //private static void TestStub()
        //{
        //    byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes("test this");
        //    byte[] salt = GenerateRandomSalt();
        //    RijndaelManaged aes = GetKey(salt, passwordBytes);

        //    byte[] e = StringEncrypt(passwordBytes, aes);
        //    byte[] r = StringDecrypt(e, aes);
        //    string x = System.Text.Encoding.UTF8.GetString(r);
        //}

        private static RijndaelManaged CreateNewKey(string controlFile, string password)
        {
            using (FileStream fsControl = new FileStream(controlFile, FileMode.Create))
            {
                byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
                byte[] salt = GenerateRandomSalt();
                byte[] actualPwd = GenerateRandomSalt(); // 32 random bytes

                RijndaelManaged aes = GetKey(salt, passwordBytes);
                byte[] encryptedlPwd = StringEncrypt(actualPwd, aes);

                fsControl.Write(salt, 0, salt.Length);
                fsControl.Write(encryptedlPwd, 0, encryptedlPwd.Length);
                return GetKey(salt, actualPwd);
            }
        }

        private static RijndaelManaged GetExistingKey(string controlFile, string password)
        {
            byte[] salt;
            byte[] actualPwd;
            try
            {
                GetActualPassword(controlFile, password, out salt, out actualPwd);
            }
            catch (Exception)
            {
                return null;
            }

            return GetKey(salt, actualPwd);
        }

        private static void GetActualPassword(string controlFile, string password, out byte[] salt, out byte[] actualPwd)
        {
            salt = new byte[32];
            using (FileStream fsControl = new FileStream(controlFile, FileMode.Open))
            {
                byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
                byte[] encryptedPwd = new byte[48]; // Buffer size required for read

                fsControl.Read(salt, 0, salt.Length);
                int read = fsControl.Read(encryptedPwd, 0, encryptedPwd.Length);

                RijndaelManaged aes = GetKey(salt, passwordBytes);
                actualPwd = StringDecrypt(encryptedPwd, aes);
            }
        }

        public static RijndaelManaged GetKey(byte[] salt, byte[] password)
        {
            // Set Rijndael symmetric encryption algorithm
            RijndaelManaged AES = new RijndaelManaged
            {
                KeySize = 256,
                BlockSize = 128,
            };

            var key = new Rfc2898DeriveBytes(password, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            AES.Mode = CipherMode.CFB;

            return AES;
        }

        public static RijndaelManaged ChangePassword(string controlFile, string oldPwd, string newPwd)
        {
            string backupName = Path.ChangeExtension(controlFile, "bak");
            try
            {
                // This is an inheritly risky operation. If anything goes wrong then the keys are lost and the vault is ruined.
                File.Copy(controlFile, backupName); // Keep the original control file

                GetActualPassword(controlFile, oldPwd, out byte[] salt, out byte[] actualPwd); // Need to keep the same actual password and salt

                // Creat a new control file
                using (FileStream fsControl = new FileStream(controlFile, FileMode.Create))
                {
                    byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(newPwd);
                    RijndaelManaged aes = GetKey(salt, passwordBytes);
                    byte[] encryptedlPwd = StringEncrypt(actualPwd, aes);

                    fsControl.Write(salt, 0, salt.Length);
                    fsControl.Write(encryptedlPwd, 0, encryptedlPwd.Length);
                }

                File.Delete(backupName);
                return GetKey(salt, actualPwd);
            }
            catch (Exception e)
            {
                MessageBoxForm.Show(string.Format(Strings.PasswordChangeFailed, e.Message));
                if (File.Exists(backupName))
                {
                    File.Move(backupName, controlFile);
                }
            }
            return null;
        }
    }
}

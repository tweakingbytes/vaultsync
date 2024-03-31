using System;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace VaultSync
{
    class UpdateCheck
    {
        public enum VersionResult { UpdateAvailable, NoChange, Error }
        public delegate void VersionResultHandler(VersionResult result);
        public event VersionResultHandler OnVersionResult;

        public void CheckForUdate()
        {
            new Task(()=>ParseData(DownLoadUpdateStatus())).Start();
        }

        private string DownLoadUpdateStatus()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Strings.VersionCheckURL);
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (Stream receiveStream = response.GetResponseStream())
                        {
                            StreamReader readStream = null;

                            if (String.IsNullOrWhiteSpace(response.CharacterSet))
                            {
                                readStream = new StreamReader(receiveStream);
                            }
                            else
                            {
                                readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                            }
                            string html = readStream.ReadToEnd();
                            readStream.Close();
                            return html;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception checking for update: " + e.Message);
            }
            return "";
        }

        private void ParseData(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                DoEvent(VersionResult.Error);
                return;
            }

            // this is parsed using regex so that there isn't another library dependency
            var regex = new Regex("> *Version *?([0-9]+?\\.[0-9]+?\\.?[0-9]*\\.?[0-9]*) *<");
            var groups = regex.Match(html).Groups;
            if (groups.Count > 1)
            {
                CompareVersions(groups[1].Value);
            }
            else
            {
                DoEvent(VersionResult.Error);
            }
        }

        private void CompareVersions(string version)
        {
            string exeName = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
            Uri fileUri = new Uri(exeName);
            string path = fileUri.LocalPath;
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(path);

            var versionSplit = version.Split('.');
            var infoSplit = info.ProductVersion.Split('.');

            // anticipate less digits in the version than the product
            bool different = false;
            for (int i = 0; i < versionSplit.Length; ++i)
            {
                if (versionSplit[i] != infoSplit[i])
                {
                    different = true;
                    break;
                }
            }

            if (different)
            {
                DoEvent(VersionResult.UpdateAvailable);
            }
            else
            {
                DoEvent(VersionResult.NoChange);
            }
        }

        private void DoEvent(VersionResult result)
        {
            OnVersionResult?.Invoke(result);
        }
    }
}

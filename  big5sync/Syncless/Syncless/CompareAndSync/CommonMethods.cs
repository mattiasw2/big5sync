using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using Syncless.Core;

namespace Syncless.CompareAndSync
{
    public static class CommonMethods
    {
        public static string CalculateMD5Hash(FileInfo fileInput)
        {
            if (!fileInput.Exists)
                throw new FileNotFoundException(fileInput.Name + ": Unable to hash non-existent file.");

            try
            {
                FileStream fileStream = fileInput.OpenRead();
                byte[] fileHash = MD5.Create().ComputeHash(fileStream);
                fileStream.Close();
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < fileHash.Length; i++)
                    sb.Append(fileHash[i].ToString("X2"));

                return sb.ToString();
            }
            catch (UnauthorizedAccessException e)
            {
                return string.Empty;
            }
            catch (DirectoryNotFoundException e)
            {
                return string.Empty;
            }
            catch (IOException e)
            {
                return string.Empty;
            }
        }

        public static void DeleteFileToRecycleBin(string path)
        {
            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(path, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
        }

        public static void ArchiveFile(string path, string archiveName, int archiveLimit)
        {
            FileInfo f = new FileInfo(path);
            string parent = f.DirectoryName;
            string archiveDir = Path.Combine(parent, archiveName);
            if (!Directory.Exists(archiveDir))
                Directory.CreateDirectory(archiveDir);
            string currTime = String.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + "_";

            File.Copy(path, Path.Combine(archiveDir, currTime + f.Name), true); //Very rare to have same time

            DirectoryInfo d = new DirectoryInfo(archiveDir);
            FileInfo[] files = d.GetFiles();
            List<string> archivedFiles = new List<string>();

            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Name.EndsWith(f.Name))
                {
                    archivedFiles.Add(files[i].Name);
                }
            }

            var sorted = (from element in archivedFiles orderby element descending select element).Skip(archiveLimit);

            foreach (string s in sorted)
            {
                File.Delete(Path.Combine(archiveDir, s));
            }

        }
    }
}

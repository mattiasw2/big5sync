using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace SynclessSeamlessTester
{
    public class TestLogic
    {
        // Source or dest
        private const int SRC = 1, DEST = 2;

        // Source possiblities
        private const int CREATE = 100;

        // Dest possibilities
        private const int DELETE = 200, RENAME = 201, UPDATE = 202;

        // Max and min time
        private const int MINTIME = 0, MAXTIME = 5000; //180000;

        private const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private List<string> sourcePaths, destPaths;

        public TestLogic()
        {
            sourcePaths = new List<string>();
            destPaths = new List<string>();
        }

        public List<string> AddToSource(string s)
        {
            sourcePaths.Add(s);
            return sourcePaths;
        }

        public List<string> AddToDest(string s)
        {
            destPaths.Add(s);
            return destPaths;
        }

        public void StartTest(int duration)
        {
            long timeToEnd = DateTime.Now.AddSeconds(duration).Ticks;

            while (DateTime.Now.Ticks < timeToEnd)
            {
                Thread.Sleep(TimerGenerator());

                switch (RandomSourceOrDest())
                {
                    case SRC:
                        Create();
                        break;
                    case DEST:
                        Create();
                        //Rename();
                        //switch (RandomDestAction())
                        //{
                        //    case DELETE:
                        //        Delete();
                        //        break;
                        //    case RENAME:
                        //        Rename();
                        //        break;
                        //    case UPDATE:
                        //        break;
                        //}

                        break;
                }
            }

        }

        #region FileSystem Operations

        private void Create()
        {
            int sourceIndex = new Random().Next(0, sourcePaths.Count);
            Thread.Sleep(1);
            int destIndex = new Random().Next(0, destPaths.Count);
            Thread.Sleep(1);
            string[] sourceObjects = Directory.GetFileSystemEntries(sourcePaths[sourceIndex]);
            string[] destSubFolders = Directory.GetDirectories(destPaths[destIndex], "*", SearchOption.AllDirectories);
            List<string> destFolders = new List<string>();
            destFolders.Add(destPaths[destIndex]);
            destFolders.AddRange(destSubFolders);

            int srcObjIndex = new Random().Next(0, sourceObjects.Length);
            Thread.Sleep(1);
            int destFldrIndex = new Random().Next(0, destFolders.Count);

            try
            {
                if (File.Exists(sourceObjects[srcObjIndex]))
                    File.Copy(sourceObjects[srcObjIndex],
                              Path.Combine(destFolders[destFldrIndex], new FileInfo(sourceObjects[srcObjIndex]).Name));
                else if (Directory.Exists(sourceObjects[srcObjIndex]))
                    CopyDirectory(sourceObjects[srcObjIndex],
                                  Path.Combine(destFolders[destFldrIndex],
                                               new DirectoryInfo(sourceObjects[srcObjIndex]).Name));
            }
            catch (IOException) { }
        }

        private void Delete()
        {
            int destIndex = new Random().Next(0, destPaths.Count);
            string[] fsi = Directory.GetFileSystemEntries(destPaths[destIndex]);
            int fsiIndex = new Random().Next(0, fsi.Length);

            try
            {
                if (File.Exists(fsi[fsiIndex]))
                    File.Delete(fsi[fsiIndex]);
                else if (Directory.Exists(fsi[fsiIndex]))
                    Directory.Delete(fsi[fsiIndex]);
            }
            catch (IOException) { }
        }

        private void Rename()
        {
            int destIndex = new Random().Next(0, destPaths.Count);
            string[] fsi = Directory.GetFileSystemEntries(destPaths[destIndex]);
            int fsiIndex = new Random().Next(0, fsi.Length);

            try
            {
                if (File.Exists(fsi[fsiIndex]))
                    File.Move(fsi[fsiIndex],
                              Path.Combine(new FileInfo(fsi[fsiIndex]).Directory.FullName, RandomString()));
                else if (Directory.Exists(fsi[fsiIndex]))
                    Directory.Move(fsi[fsiIndex],
                                   Path.Combine(new DirectoryInfo(fsi[fsiIndex]).Parent.FullName, RandomString()));
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine(e.ToString());
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion

        #region Random Generators

        private int RandomSourceOrDest()
        {
            return new Random().Next(1, 3);
        }

        private int RandomDestAction()
        {
            return new Random().Next(200, 203);
        }

        private int TimerGenerator()
        {
            Random rand = new Random();
            double x = rand.NextDouble() * MAXTIME;
            Thread.Sleep(1);
            double y = rand.NextDouble() * MAXTIME;

            return Convert.ToInt32(x + y);
        }

        private string RandomString()
        {
            char[] buffer = new char[new Random().Next(5, 20)];

            for (int i = 0; i < buffer.Count(); i++)
            {
                buffer[i] = CHARS[new Random().Next(CHARS.Length)];
            }

            return new string(buffer);
        }

        #endregion

        public static void CopyDirectory(string source, string destination)
        {
            try
            {
                DirectoryInfo sourceInfo = new DirectoryInfo(source);
                DirectoryInfo[] directoryInfos = sourceInfo.GetDirectories();
                DirectoryInfo destinationInfo = new DirectoryInfo(destination);
                if (!destinationInfo.Exists)
                {
                    Directory.CreateDirectory(destination);
                }
                foreach (DirectoryInfo tempInfo in directoryInfos)
                {
                    DirectoryInfo newDirectory = destinationInfo.CreateSubdirectory(tempInfo.Name);
                    CopyDirectory(tempInfo.FullName, newDirectory.FullName);
                }
                FileInfo[] fileInfos = sourceInfo.GetFiles();
                foreach (FileInfo fileInfo in fileInfos)
                {
                    fileInfo.CopyTo(Path.Combine(destination, fileInfo.Name), true);
                }
            }
            catch (IOException e)
            {
                throw e;
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace SynclessSeamlessTester
{
    public class TestWorkerClass
    {
        // Source or dest
        private const int SRC = 1, DEST = 2;

        // Source or dest
        private const int FILE = 3, FOLDER = 4;

        // Source possiblities
        private const int CREATE = 100;

        // Dest possibilities
        private const int DELETE = 200, RENAME = 201, UPDATE = 202;

        // Max and min time
        private int _minTime, _maxTime; //180000;

        private const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890-";

        private bool fired = true;

        private readonly List<string> _sourcePaths, _destPaths;
        private int _duration;

        private TestInfo _testInfo;

        private BackgroundWorker _bgWorker;

        private DateTime _timeToEnd, _timeToStart;
        private TimeSpan _totalTimeNeeded;

        public TestWorkerClass(int duration, int minTime, int maxTime, List<string> sourcePaths, List<string> destPaths, TestInfo testInfo, BackgroundWorker bgWorker)
        {
            _sourcePaths = sourcePaths;
            _destPaths = destPaths;
            _testInfo = testInfo;
            _duration = duration;
            _bgWorker = bgWorker;
            _minTime = minTime * 1000;
            _maxTime = maxTime * 1000;
            StartTest();
        }

        private void StartTest()
        {
            _timeToStart = DateTime.Now;
            _timeToEnd = _timeToStart.AddSeconds(_duration);
            _totalTimeNeeded = TimeSpan.FromSeconds(_duration);

            Timer timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.AutoReset = false;

            while (DateTime.Now < _timeToEnd)
            {
                if (_bgWorker.CancellationPending)
                {
                    _testInfo.Propagated = true;
                    timer.Stop();
                    return;
                }

                if (fired)
                {
                    timer.Interval = TimerGenerator();
                    timer.Start();
                    fired = false;
                }
            }

        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            switch (RandomSourceOrDest())
            {
                case SRC:
                    Create();
                    break;
                case DEST:
                    switch (RandomDestAction())
                    {
                        case DELETE:
                            Delete();
                            break;
                        case RENAME:
                            Rename();
                            break;
                        case UPDATE:
                            Update();
                            break;
                    }

                    break;
            }
            fired = true;
            TimeSpan completed = DateTime.Now.Subtract(_timeToStart);
            double percent = completed.TotalSeconds / _totalTimeNeeded.TotalSeconds * 100;
            Console.WriteLine("PERCENT DONE: " + percent);

            if (!_testInfo.Propagated)
                _bgWorker.ReportProgress(Convert.ToInt32(percent));
        }

        private void Update()
        {
            int destIndex = new Random().Next(0, _destPaths.Count);
            string[] fsi = Directory.GetFiles(_destPaths[destIndex], "*", SearchOption.AllDirectories);
            Debug.Assert(fsi != null);

            try
            {
                if (fsi[destIndex].Contains(".syncless") || fsi[destIndex].Contains("_synclessArchive"))
                    return;

                if (File.Exists(fsi[destIndex]))
                    ChangeFileContent(new FileInfo(fsi[destIndex]));
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

        #region FileSystem Operations

        private void Create()
        {
            int sourceIndex = new Random().Next(0, _sourcePaths.Count);
            Thread.Sleep(1);
            int destIndex = new Random().Next(0, _destPaths.Count);
            Thread.Sleep(1);

            //Decide on whether to create a file or folder
            string[] sourceObjects = null;
            switch (RandomFileOrFolder())
            {
                case FILE:
                    sourceObjects = Directory.GetFiles(_sourcePaths[sourceIndex], "*", SearchOption.AllDirectories);
                    break;
                case FOLDER:
                    sourceObjects = Directory.GetDirectories(_sourcePaths[sourceIndex], "*", SearchOption.AllDirectories);
                    break;
            }
            Debug.Assert(sourceObjects != null);

            //Handle directories
            string[] destSubFolders = Directory.GetDirectories(_destPaths[destIndex], "*", SearchOption.AllDirectories);
            List<string> destFolders = new List<string>();
            destFolders.Add(_destPaths[destIndex]);
            destFolders.AddRange(destSubFolders);

            int srcObjIndex = new Random().Next(0, sourceObjects.Length);
            Thread.Sleep(1);
            int destFldrIndex = new Random().Next(0, destFolders.Count);

            try
            {
                //Temporary
                if (sourceObjects[srcObjIndex].Contains(".syncless") || sourceObjects[srcObjIndex].Contains("_synclessArchive") || destFolders[destFldrIndex].Contains(".syncless") || destFolders[destFldrIndex].Contains("_synclessArchive"))
                    return;

                if (File.Exists(sourceObjects[srcObjIndex]))
                    File.Copy(sourceObjects[srcObjIndex],
                              Path.Combine(destFolders[destFldrIndex], new FileInfo(sourceObjects[srcObjIndex]).Name), true);
                else if (Directory.Exists(sourceObjects[srcObjIndex]))
                    CopyDirectory(sourceObjects[srcObjIndex],
                                  Path.Combine(destFolders[destFldrIndex],
                                               new DirectoryInfo(sourceObjects[srcObjIndex]).Name));
                Console.WriteLine("CREATE: " + Path.Combine(destFolders[destFldrIndex],
                                                            new DirectoryInfo(sourceObjects[srcObjIndex]).Name));
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Delete()
        {
            int destIndex = new Random().Next(0, _destPaths.Count);
            string[] fsi = null;

            switch (RandomFileOrFolder())
            {
                case FILE:
                    fsi = Directory.GetFiles(_destPaths[destIndex], "*", SearchOption.AllDirectories);
                    break;
                case FOLDER:
                    fsi = Directory.GetDirectories(_destPaths[destIndex], "*", SearchOption.AllDirectories);
                    break;
            }
            Debug.Assert(fsi != null);

            int fsiIndex = new Random().Next(0, fsi.Length);

            try
            {
                if (fsi[fsiIndex].Contains(".syncless") || fsi[fsiIndex].Contains("_synclessArchive"))
                    return;

                if (File.Exists(fsi[fsiIndex]))
                    File.Delete(fsi[fsiIndex]);
                else if (Directory.Exists(fsi[fsiIndex]))
                    Directory.Delete(fsi[fsiIndex]);
                Console.WriteLine("DELETE: " + fsi[fsiIndex]);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Rename()
        {
            int destIndex = new Random().Next(0, _destPaths.Count);
            string[] fsi = null;

            switch (RandomFileOrFolder())
            {
                case FILE:
                    fsi = Directory.GetFiles(_destPaths[destIndex], "*", SearchOption.AllDirectories);
                    break;
                case FOLDER:
                    fsi = Directory.GetDirectories(_destPaths[destIndex], "*", SearchOption.AllDirectories);
                    break;
            }
            Debug.Assert(fsi != null);

            int fsiIndex = new Random().Next(0, fsi.Length);

            try
            {
                if (fsi[fsiIndex].Contains(".syncless") || fsi[fsiIndex].Contains("_synclessArchive"))
                    return;

                if (File.Exists(fsi[fsiIndex]))
                {
                    File.Move(fsi[fsiIndex],
                              Path.Combine(new FileInfo(fsi[fsiIndex]).Directory.FullName, RandomString()));
                    Console.WriteLine("RENAME: " + fsi[fsiIndex] + " TO " +
                                      Path.Combine(new FileInfo(fsi[fsiIndex]).Directory.FullName, RandomString()));
                }
                else if (Directory.Exists(fsi[fsiIndex]))
                {
                    Directory.Move(fsi[fsiIndex],
                                   Path.Combine(new DirectoryInfo(fsi[fsiIndex]).Parent.FullName, RandomString()));
                    Console.WriteLine("RENAME: " + fsi[fsiIndex] + " TO " +
                                      Path.Combine(new DirectoryInfo(fsi[fsiIndex]).Parent.FullName, RandomString()));
                }

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

        private int RandomFileOrFolder()
        {
            return new Random().Next(3, 5);
        }

        private int RandomDestAction()
        {
            return new Random().Next(200, 203);
        }

        //Averages out to get normal distribution
        private int TimerGenerator()
        {
            Random rand = new Random();
            int time;

            do
            {
                double x = rand.NextDouble() * _maxTime;
                Thread.Sleep(1);
                double y = rand.NextDouble() * _maxTime;
                time = Convert.ToInt32((x + y) / 2);
            }
            while (time < _minTime);

            return time;
        }

        private string RandomString()
        {
            Random rng = new Random();
            char[] buffer = new char[rng.Next(5, 20)];
            Thread.Sleep(1);

            for (int i = 0; i < buffer.Count(); i++)
            {
                buffer[i] = CHARS[rng.Next(CHARS.Length)];
                Thread.Sleep(1);
            }

            return new string(buffer);
        }

        #endregion

        private void ChangeFileContent(FileInfo info)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(info.FullName, true);
                sw.WriteLine(RandomString());
                sw.Flush();
                Console.WriteLine("UPDATE: " + info.FullName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                if (sw != null)
                    sw.Close();
            }
        }

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
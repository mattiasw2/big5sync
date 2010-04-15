using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SynclessTaggingTester
{
    public static class TestReadWriteHelper
    {
        public static void Write(List<TestCase> testcases, string outputfile)
        {
            FileStream ofs = new FileStream(outputfile, FileMode.Create);
            StreamWriter sw = new StreamWriter(ofs);
            foreach (TestCase testcase in testcases)
            {
                sw.WriteLine(testcase.ToString());
            }
            sw.Close();
            ofs.Close();
        }

        public static List<TestCase> Read(string inputfile)
        {
            FileStream ifs = new FileStream(inputfile, FileMode.Open);
            StreamReader sr = new StreamReader(ifs);
            List<TestCase> testcases = new List<TestCase>();
            string inputline = "";
            while ((inputline = sr.ReadLine()) != null)
            {
                if (!inputline.Trim().Equals(""))
                {
                    if (!inputline.StartsWith(@"//"))
                    {
                        testcases.Add(new TestCase(inputline));
                    }
                }
            }
            sr.Close();
            ifs.Close();
            return testcases;
        }
    }
}

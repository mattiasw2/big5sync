using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using Syncless.Tagging;
using Syncless.Tagging.Exceptions;
using System.Threading;
using Syncless.Helper;

namespace SynclessTaggingTester
{
    public class TestDriver
    {
        TaggingLayer _logic;
        List<TestCase> _testcases;
        private string _inputfile, _outputfile;

        public TestDriver(string inputfile, string outputfile)
        {
            _logic = TaggingLayer.Instance;
            _logic.TaggingProfile.ProfileName = "profile1";
            _testcases = new List<TestCase>();
            _inputfile = inputfile;
            _outputfile = outputfile;
        }

        public void ReadTestCases()
        {
            FileStream ifs = new FileStream(_inputfile, FileMode.Open);
            StreamReader sr = new StreamReader(ifs);
            string inputline = "";
            while ((inputline = sr.ReadLine()) != null)
            {
                if (!inputline.Trim().Equals(""))
                {
                    if (!inputline.StartsWith(@"//"))
                    {
                        _testcases.Add(new TestCase(inputline));
                    }
                }
            }
            sr.Close();
            ifs.Close();
        }

        public void ExecuteTestCases()
        {
            foreach (TestCase testcase in _testcases)
            {
                if (testcase.Method.Equals("CreateTag"))
                {
                    TestCreateTag(testcase);
                }
                else if (testcase.Method.Equals("TagFolder"))
                {
                    TestTagFolder(testcase);
                }
                else if (testcase.Method.Equals("UntagFolder"))
                {
                    TestUntagFolder(testcase);
                }
                else if (testcase.Method.Equals("RenameTag"))
                {
                    TestRenameTag(testcase);
                }
                else if (testcase.Method.Equals("RenameFolder"))
                {
                    TestRenameFolder(testcase);
                }
                else if (testcase.Method.Equals("RetrieveTag"))
                {
                    TestRetrieveTag(testcase);
                }
                else if (testcase.Method.Equals("RetrieveAllTags"))
                {
                    TestRetrieveAllTags(testcase);
                }
                else if (testcase.Method.Equals("RetrieveTagByPath"))
                {
                    TestRetrieveTagByPath(testcase);
                }
                else if (testcase.Method.Equals("RetrievePathByLogicalId"))
                {
                    TestRetrievePathByLogicalId(testcase);
                }
                else if (testcase.Method.Equals("RetrieveTagByLogicalId"))
                {
                    TestRetrieveTagByLogicalId(testcase);
                }
                else if (testcase.Method.Equals("FindSimilarPathForFolder"))
                {
                    TestFindSimilarPathForFolder(testcase);
                }
                else if (testcase.Method.Equals("RetrieveParentTagByPath"))
                {
                    TestRetrieveParentTagByPath(testcase);
                }
                else if (testcase.Method.Equals("RetrieveAncestors"))
                {
                    TestRetrieveAncestors(testcase);
                }
                else if (testcase.Method.Equals("RetrieveDescendants"))
                {
                    TestRetrieveDescendants(testcase);
                }
                else if (testcase.Method.Equals("SaveToXml"))
                {
                    TestSaveToXml(testcase);
                }
                else if (testcase.Method.Equals("AppendProfile"))
                {
                    TestAppendProfile(testcase);
                }
                else if (testcase.Method.Equals("DeleteTag"))
                {
                    TestDeleteTag(testcase);
                }
                else
                {
                    throw new Exception(string.Format("Unsupported method : {0}", testcase.Method));
                }
            }
        }

        public void WriteTestResults()
        {
            FileStream ofs = new FileStream(_outputfile, FileMode.Create);
            StreamWriter sw = new StreamWriter(ofs);
            foreach (TestCase testcase in _testcases)
            {
                sw.WriteLine(testcase.ToString());
            }
            sw.Close();
            ofs.Close();
        }

        private void TestAppendProfile(TestCase testcase)
        {
            string[] parameters = testcase.Parameters.Split(',');
            List<string> locations = new List<string>();
            for (int i = 0; i < parameters.Length; i++)
            {
                locations.Add(parameters[i].Trim());
            }
            try
            {
                _logic.AppendProfile(locations);
                testcase.Actual = "";
            }
            catch (Exception)
            {
                testcase.Actual = "Exception";
            }
            testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
        }

        private void TestSaveToXml(TestCase testcase)
        {
            string[] parameters = testcase.Parameters.Split(',');
            List<string> locations = new List<string>();
            for (int i = 0; i < parameters.Length; i++)
            {
                locations.Add(parameters[i].Trim());
            }
            try
            {
                _logic.SaveTo(locations);
                testcase.Actual = "";
            }
            catch (IOException ioe)
            {
                testcase.Actual = "IOException";
            }
            catch (Exception e)
            {
                testcase.Actual = "Exception";
            }
            testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
        }

        private void TestRetrieveDescendants(TestCase testcase)
        {
            string path = testcase.Parameters;
            List<string> descendants = _logic.RetrieveDescendants(path);
            descendants.Sort();
            string result = "";
            foreach (string descendant in descendants)
            {
                result += (descendant + " ");
            }
            testcase.Actual = result.Trim();
            testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
        }

        private void TestRetrieveAncestors(TestCase testcase)
        {
            string path = testcase.Parameters;
            List<string> ancestors = _logic.RetrieveAncestors(path);
            ancestors.Sort();
            string result = "";
            foreach (string ancestor in ancestors)
            {
                result += (ancestor + " ");
            }
            testcase.Actual = result.Trim();
            testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
        }

        private void TestRetrieveParentTagByPath(TestCase testcase)
        {
            string path = testcase.Parameters;
            List<Tag> taglist = _logic.RetrieveParentTagByPath(path);
            List<string> tagnamelist = new List<string>();
            foreach (Tag tag in taglist)
            {
                tagnamelist.Add(tag.TagName);
            }
            tagnamelist.Sort();
            string result = "";
            foreach (string tagname in tagnamelist)
            {
                result += (tagname + " ");
            }
            testcase.Actual = result.Trim();
            testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
        }

        //private void TestRetrieveParentByPath(TestCase testcase)
        //{
        //    string path = testcase.Parameters;
        //    List<string> parentpathlist = _logic.RetrieveParentByPath(path);
        //    parentpathlist.Sort();
        //    string result = "";
        //    foreach (string parentpath in parentpathlist)
        //    {
        //        result += (parentpath + " ");
        //    }
        //    testcase.Actual = result.Trim();
        //    testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
        //}

        private void TestFindSimilarPathForFolder(TestCase testcase)
        {
            string path = testcase.Parameters;
            List<string> pathlist = _logic.FindSimilarPathForFolder(path);
            pathlist.Sort();
            string result = "";
            foreach (string p in pathlist)
            {
                result += (p + " ");
            }
            testcase.Actual = result.Trim();
            testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
        }

        private void TestRetrieveTagByLogicalId(TestCase testcase)
        {
            string logicalid = testcase.Parameters;
            List<Tag> taglist = _logic.RetrieveTagByLogicalId(logicalid);
            List<string> tagnamelist = new List<string>();
            foreach (Tag tag in taglist)
            {
                tagnamelist.Add(tag.TagName);
            }
            tagnamelist.Sort();
            string result = "";
            foreach (string tagname in tagnamelist)
            {
                result += (tagname + " ");
            }
            testcase.Actual = result.Trim();
            testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
        }

        private void TestRetrievePathByLogicalId(TestCase testcase)
        {
            string logicalid = testcase.Parameters;
            List<string> pathlist = _logic.RetrievePathByLogicalId(logicalid);
            testcase.Actual = pathlist.Count.ToString();
            testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
        }

        private void TestRetrieveTagByPath(TestCase testcase)
        {
            string path = testcase.Parameters;
            List<Tag> taglist = _logic.RetrieveTagByPath(path);
            string result = "";
            List<string> tagnamelist = new List<string>();
            foreach (Tag tag in taglist)
            {
                tagnamelist.Add(tag.TagName);
            }
            tagnamelist.Sort();
            foreach (string tagname in tagnamelist)
            {
                result += (tagname + " ");
            }
            testcase.Actual = result.Trim();
            testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
        }

        private void TestRetrieveAllTags(TestCase testcase)
        {
            bool getdeleted = bool.Parse(testcase.Parameters);
            List<Tag> taglist = _logic.RetrieveAllTags(getdeleted);
            testcase.Actual = taglist.Count.ToString();
            testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
        }

        private void TestRetrieveTag(TestCase testcase)
        {
            string tagname = testcase.Parameters;
            Tag tag = _logic.RetrieveTag(tagname);
            testcase.Actual = (tag != null ? "Not null" : "Null");
            testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
        }

        private void TestRenameFolder(TestCase testcase)
        {
            string[] parameters = testcase.Parameters.Split(',');
            string oldpath = parameters[0].Trim();
            string newpath = parameters[1].Trim();
            List<string> renamedpathlist = new List<string>();
            _logic.RenameFolder(oldpath, newpath);
            foreach (Tag tag in _logic.TagList)
            {
                foreach (string path in tag.FilteredPathListString)
                {
                    if (PathHelper.AddTrailingSlash(path).StartsWith(PathHelper.AddTrailingSlash(newpath)))
                    {
                        renamedpathlist.Add(path);
                    }
                }
            }
            testcase.Actual = renamedpathlist.Count.ToString();
            testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
        }

        private void TestRenameTag(TestCase testcase)
        {
            string[] parameters = testcase.Parameters.Split(',');
            string oldname = parameters[0].Trim();
            string newname = parameters[1].Trim();
            try
            {
                _logic.RenameTag(oldname, newname);
                testcase.Actual = "";
            }
            catch (TagAlreadyExistsException)
            {
                testcase.Actual = "TagAlreadyExistsException";
            }
            catch (TagNotFoundException)
            {
                testcase.Actual = "TagNotFoundException";
            }
            testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
        }

        private void TestDeleteTag(TestCase testcase)
        {
            string tagname = testcase.Parameters;
            try
            {
                Tag tag = _logic.DeleteTag(tagname);
                testcase.Actual = string.Format("{0}", tag.IsDeleted.ToString());
            }
            catch (TagNotFoundException)
            {
                testcase.Actual = "TagNotFoundException";
            }
            testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
        }

        private void TestUntagFolder(TestCase testcase)
        {
            string[] parameters = testcase.Parameters.Split(',');
            string path, tagname;
            path = parameters[0].Trim();
            int result;
            switch (parameters.Length)
            {
                case 1:
                    try
                    {
                        result = _logic.UntagFolder(path);
                        testcase.Actual = result.ToString();
                    }
                    catch (TagNotFoundException)
                    {
                        testcase.Actual = "TagNotFoundException";
                    }
                    break;
                case 2:
                    tagname = parameters[1].Trim();
                    try
                    {
                        result = _logic.UntagFolder(path, tagname);
                        testcase.Actual = result.ToString();
                    }
                    catch (TagNotFoundException)
                    {
                        testcase.Actual = "TagNotFoundException";
                    }
                    break;
                default:
                    throw new Exception("Invalid parameters.");
            }
            testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
        }

        private void TestTagFolder(TestCase testcase)
        {
            string[] parameters = testcase.Parameters.Split(',');
            string path = parameters[0].Trim();
            string tagname = parameters[1].Trim();
            try
            {
                Tag tag = _logic.TagFolder(path, tagname);
                testcase.Actual = string.Format("{0}, {1}", tag.TagName, tag.UnfilteredPathList.Count.ToString());
            }
            catch (PathAlreadyExistsException)
            {
                testcase.Actual = "PathAlreadyExistsException";
            }
            testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
        }

        private void TestCreateTag(TestCase testcase)
        {
            string tagname = testcase.Parameters;
            try
            {
                Tag tag = _logic.CreateTag(tagname);
                testcase.Actual = string.Format("{0}, {1}, {2}, {3}, {4}", tag.TagName, tag.IsDeleted.ToString(), tag.DeletedDate.ToString(), tag.UnfilteredPathList.Count.ToString(), tag.Filters.Count.ToString());
            }
            catch (TagAlreadyExistsException)
            {
                testcase.Actual = "TagAlreadyExistsException";
            }
            testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
        }

        private void WriteLog(string logmessage)
        {
            if (!File.Exists("TaggingLog.txt"))
            {
                File.Create("TaggingLog.txt");
            }
            FileStream fs = new FileStream("TaggingLog.txt", FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(string.Format("{0} : {1}", DateTime.Now.ToString(), logmessage));
            sw.Close();
        }
    }
}

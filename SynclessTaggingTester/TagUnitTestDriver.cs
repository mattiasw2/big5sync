using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Tagging;
using Syncless.Filters;

namespace SynclessTaggingTester
{
    public class TagUnitTestDriver : ITestDriverInterface
    {
        List<TestCase> _testcases;
        private string _inputfile, _outputfile;
        List<Tag> _taglist;

        public TagUnitTestDriver(string inputfile, string outputfile)
        {
            _inputfile = inputfile;
            _outputfile = outputfile;
            _taglist = new List<Tag>();
        }

        #region ITestDriverInterface Members

        public void Start()
        {
            ReadTestCases();
            ExecuteTestCases();
            WriteTestResults();
        }

        public void ReadTestCases()
        {
            _testcases = TestReadWriteHelper.Read(_inputfile);
        }

        public void ExecuteTestCases()
        {
            foreach (TestCase testcase in _testcases)
            {
                if (testcase.Method.Equals("Create"))
                {
                    TestCreateTag(testcase);
                }
                else if (testcase.Method.Equals("Remove"))
                {
                    TestRemoveTag(testcase);
                }
                else if (testcase.Method.Equals("AddPathString"))
                {
                    TestAddPathString(testcase);
                }
                else if (testcase.Method.Equals("AddPathObject"))
                {
                    TestAddPathObject(testcase);
                }
                else if (testcase.Method.Equals("RenamePath"))
                {
                    TestRenamePath(testcase);
                }
                else if (testcase.Method.Equals("RemovePathString"))
                {
                    TestRemovePathString(testcase);
                }
                else if (testcase.Method.Equals("RemovePathObject"))
                {
                    TestRemovePathObject(testcase);
                }
                else if (testcase.Method.Equals("RemoveAllPaths"))
                {
                    TestRemoveAllPaths(testcase);
                }
                else if (testcase.Method.Equals("ContainsParent"))
                {
                    TestContainsParent(testcase);
                }
                else if (testcase.Method.Equals("Contains"))
                {
                    TestContains(testcase);
                }
                else if (testcase.Method.Equals("CreateTrailingPath"))
                {
                    TestCreateTrailingPath(testcase);
                }
                else if (testcase.Method.Equals("AddFilter"))
                {
                    TestAddFilter(testcase);
                }
                else if (testcase.Method.Equals("RemoveFilter"))
                {
                    TestRemoveFilter(testcase);
                }
                else if (testcase.Method.Equals("FindPath"))
                {
                    TestFindPath(testcase);
                }
                else if (testcase.Method.Equals("FindAncestorsDescendants"))
                {
                    TestFindAncestorsDescendants(testcase);
                }
                else
                {
                    throw new Exception(string.Format("Method {0} not supported.", testcase.Method));
                }
            }
        }

        public void WriteTestResults()
        {
            TestReadWriteHelper.Write(_testcases, _outputfile);
        }

        #endregion

        private void TestFindAncestorsDescendants(TestCase testcase)
        {
            string[] parameters = testcase.Parameters.Split(',');
            string tagname = parameters[0].Trim();
            string path = parameters[1].Trim();
            bool ancestors = bool.Parse(parameters[2].Trim());
            Tag tag = FindTag(tagname);
            if (tag != null)
            {
                List<string> results = new List<string>();
                if (ancestors)
                {
                    results = tag.FindAncestors(path);
                }
                else
                {
                    results = tag.FindDescendants(path);
                }
                testcase.Actual = results.Count.ToString();
                testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
            }
            else
            {
                testcase.Actual = "Invalid parameters";
                testcase.Passed = false;
            }
        }

        private void TestFindPath(TestCase testcase)
        {
            string[] parameters = testcase.Parameters.Split(',');
            string tagname = parameters[0].Trim();
            string path = parameters[1].Trim();
            bool filtered = bool.Parse(parameters[2].Trim());
            Tag tag = FindTag(tagname);
            if (tag != null)
            {
                TaggedPath taggedpath = tag.FindPath(path, filtered);
                testcase.Actual = ((taggedpath != null) ? "Not null" : "Null");
                testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
            }
            else
            {
                testcase.Actual = "Invalid parameters";
                testcase.Passed = false;
            }
        }

        private void TestRemoveFilter(TestCase testcase)
        {
            string[] parameters = testcase.Parameters.Split(',');
            string tagname = parameters[0].Trim();
            string pattern = parameters[1].Trim();
            FilterMode mode = (bool.Parse(parameters[2].Trim()) ? FilterMode.INCLUDE : FilterMode.EXCLUDE);
            Tag tag = FindTag(tagname);
            if (tag != null)
            {
                Filter filter = tag.RemoveFilter(FilterFactory.CreateExtensionFilter(pattern, mode), DateTime.Now.Ticks);
                testcase.Actual = ((filter != null) ? "Not null" : "Null");
                testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
            }
            else
            {
                testcase.Actual = "Invalid parameters";
                testcase.Passed = false;
            }
        }

        private void TestAddFilter(TestCase testcase)
        {
            string[] parameters = testcase.Parameters.Split(',');
            string tagname = parameters[0].Trim();
            string pattern = parameters[1].Trim();
            FilterMode mode = (bool.Parse(parameters[2].Trim()) ? FilterMode.INCLUDE : FilterMode.EXCLUDE);
            Tag tag = FindTag(tagname);
            if (tag != null)
            {
                Filter filter = FilterFactory.CreateExtensionFilter(pattern, mode);
                tag.AddFilter(filter, DateTime.Now.Ticks);
                testcase.Actual = tag.Filters.Count.ToString();
                testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
            }
            else
            {
                testcase.Actual = "Invalid parameters";
                testcase.Passed = false;
            }
        }

        private void TestCreateTrailingPath(TestCase testcase)
        {
            string[] parameters = testcase.Parameters.Split(',');
            string tagname = parameters[0].Trim();
            string path = parameters[1].Trim();
            Tag tag = FindTag(tagname);
            if (tag != null)
            {
                string result = tag.CreateTrailingPath(path);
                testcase.Actual = ((result != null) ? result : "Null");
                testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
            }
            else
            {
                testcase.Actual = "Invalid parameters";
                testcase.Passed = false;
            }
        }

        private void TestContains(TestCase testcase)
        {
            string[] parameters = testcase.Parameters.Split(',');
            string tagname = parameters[0].Trim();
            string path = parameters[1].Trim();
            bool ignores = bool.Parse(parameters[2].Trim());
            Tag tag = FindTag(tagname);
            string success;
            if (tag != null)
            {
                if (ignores)
                {
                    success = tag.ContainsIgnoreDeleted(path).ToString();
                }
                else
                {
                    success = tag.Contains(path).ToString();
                }
                testcase.Actual = success;
                testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
            }
            else
            {
                testcase.Actual = "Invalid parameters";
                testcase.Passed = false;
            }
        }

        private void TestContainsParent(TestCase testcase)
        {
            string[] parameters = testcase.Parameters.Split(',');
            string tagname = parameters[0].Trim();
            string path = parameters[1].Trim();
            bool ignores = bool.Parse(parameters[2].Trim());
            Tag tag = FindTag(tagname);
            string success;
            if (tag != null)
            {
                if (ignores)
                {
                    success = tag.ContainsParentIgnoreDeleted(path).ToString();
                }
                else
                {
                    success = tag.ContainsParent(path).ToString();
                }
                testcase.Actual = success;
                testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
            }
            else
            {
                testcase.Actual = "Invalid parameters";
                testcase.Passed = false;
            }
        }

        private void TestRemoveAllPaths(TestCase testcase)
        {
            string tagname = testcase.Parameters;
            Tag tag = FindTag(tagname);
            bool allremoved = true;
            if (tag != null)
            {
                tag.RemoveAllPaths();
                foreach (TaggedPath path in tag.UnfilteredPathList)
                {
                    if (!path.IsDeleted)
                    {
                        allremoved = false;
                        break;
                    }
                }
                testcase.Actual = allremoved.ToString();
                testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
            }
            else
            {
                testcase.Actual = "Invalid parameters";
                testcase.Passed = false;
            }
        }

        private void TestRemovePathObject(TestCase testcase)
        {
            string[] parameters = testcase.Parameters.Split(',');
            string tagname = parameters[0].Trim();
            string path = parameters[1].Trim();
            TaggedPath taggedpath = new TaggedPath(path, DateTime.Now.Ticks);
            Tag tag = FindTag(tagname);
            if (tag != null)
            {
                string success = tag.RemovePath(taggedpath).ToString();
                TaggedPath removedpath = tag.FindPath(path, false);
                string isdeleted = removedpath.IsDeleted.ToString();
                testcase.Actual = string.Format("{0}, {1}", success, isdeleted);
                testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
            }
            else
            {
                testcase.Actual = "Invalid parameters";
                testcase.Passed = false;
            }
        }

        private void TestRemovePathString(TestCase testcase)
        {
            string[] parameters = testcase.Parameters.Split(',');
            string tagname = parameters[0].Trim();
            string path = parameters[1].Trim();
            Tag tag = FindTag(tagname);
            if (tag != null)
            {
                string success = tag.RemovePath(path, DateTime.Now.Ticks).ToString();
                TaggedPath removedpath = tag.FindPath(path, false);
                string isdeleted = removedpath.IsDeleted.ToString();
                testcase.Actual = string.Format("{0}, {1}", success, isdeleted);
                testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
            }
            else
            {
                testcase.Actual = "Invalid parameters";
                testcase.Passed = false;
            }
        }

        private void TestRenamePath(TestCase testcase)
        {
            string[] parameters = testcase.Parameters.Split(',');
            string tagname = parameters[0].Trim();
            string oldpath = parameters[1].Trim();
            string newpath = parameters[2].Trim();
            Tag tag = FindTag(tagname);
            if (tag != null)
            {
                string renamecount = tag.RenamePath(oldpath, newpath, DateTime.Now.Ticks).ToString();
                testcase.Actual = renamecount;
                testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
            }
            else
            {
                testcase.Actual = "Invalid parameters";
                testcase.Passed = false;
            }
        }

        private void TestAddPathObject(TestCase testcase)
        {
            string[] parameters = testcase.Parameters.Split(',');
            string tagname = parameters[0].Trim();
            string path = parameters[1].Trim();
            TaggedPath taggedpath = new TaggedPath(path, DateTime.Now.Ticks);
            Tag tag = FindTag(tagname);
            if (tag != null)
            {
                string success = tag.AddPath(taggedpath).ToString();
                testcase.Actual = success;
                testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
            }
            else
            {
                testcase.Actual = "Invalid parameters";
                testcase.Passed = false;
            }
        }

        private void TestAddPathString(TestCase testcase)
        {
            string[] parameters = testcase.Parameters.Split(',');
            string tagname = parameters[0].Trim();
            string path = parameters[1].Trim();
            Tag tag = FindTag(tagname);
            if (tag != null)
            {
                string success = tag.AddPath(path, DateTime.Now.Ticks).ToString();
                testcase.Actual = success;
                testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
            }
            else
            {
                testcase.Actual = "Invalid parameters";
                testcase.Passed = false;
            }
        }

        private void TestRemoveTag(TestCase testcase)
        {
            string tagname = testcase.Parameters;
            Tag tag = FindTag(tagname);
            if (tag != null)
            {
                tag.Remove(DateTime.Now.Ticks);
                string isdeleted = tag.IsDeleted.ToString();
                string pathcount = tag.UnfilteredPathList.Count.ToString();
                testcase.Actual = string.Format("{0}, {1}", isdeleted, pathcount);
                testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
            }
            else
            {
                testcase.Actual = "Invalid parameters";
                testcase.Passed = false;
            }
        }

        private void TestCreateTag(TestCase testcase)
        {
            string tagname = testcase.Parameters;
            Tag tag = new Tag(tagname, DateTime.Now.Ticks);
            _taglist.Add(tag);
            testcase.Actual = _taglist.Count.ToString();
            testcase.Passed = (testcase.Expected.Equals(testcase.Actual));
        }

        private void AddTag(Tag tag)
        {
            _taglist.Add(tag);
        }

        private Tag FindTag(string tagname)
        {
            foreach (Tag tag in _taglist)
            {
                if (tag.TagName.Equals(tagname))
                {
                    return tag;
                }
            }
            return null;
        }
    }
}

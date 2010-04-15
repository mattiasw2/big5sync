using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SynclessTaggingTester
{
    public class TestCase
    {
        private string _line, _testid, _method, _parameters, _expected, _actual, _comment;
        private bool _passed;

        public string Line
        {
            get { return _line; }
            set { _line = value; }
        }

        public string Testid
        {
            get { return _testid; }
            set { _testid = value; }
        }

        public string Method
        {
            get { return _method; }
            set { _method = value; }
        }

        public string Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

        public string Expected
        {
            get { return _expected; }
            set { _expected = value; }
        }

        public string Actual
        {
            get { return _actual; }
            set { _actual = value; }
        }

        public string Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }
        
        public bool Passed
        {
            get { return _passed; }
            set { _passed = value; }
        }

        public TestCase(string line)
        {
            string[] caseinfo = line.Split('@');
            Debug.Assert(caseinfo.Length >= 4);
            this._line = line;
            this._testid = caseinfo[0].Trim();
            this._method = caseinfo[1].Trim();
            this._parameters = caseinfo[2].Trim();
            this._expected = caseinfo[3].Trim();
            if (caseinfo.Length > 4)
            {
                this._comment = caseinfo[4].Trim();
            }
            else
            {
                this._comment = "";
            }
            this._actual = "";
        }

        public override string ToString()
        {
            string result = _passed ? "passed" : "failed";
            return result + " [" + _line + "] " + (_passed ? "" : _actual);
        }
    }
}

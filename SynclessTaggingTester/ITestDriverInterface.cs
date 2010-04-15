using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SynclessTaggingTester
{
    public interface ITestDriverInterface
    {
        void Start();
        void ReadTestCases();
        void ExecuteTestCases();
        void WriteTestResults();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SynclessTaggingTester
{
    class Program
    {
        static void Main(string[] args)
        {
            //test case format
            //Case id @ Method name @ Parameter1, Parameter2 @ Expected result @ Option comment
            //to make comment in test case file that is not considered by the test driver, use //
            //save test cases in testcases.txt in the folder where SynclessTaggingTester.exe is running
            //test results are saved in testresults.txt in the folder where SynclessTaggingTester.exe is running
            //a sample testresults.txt is given in the project folder
            Console.WriteLine("Starting test driver...");
            TestDriver driver = new TestDriver("testcases.txt", "testresults.txt");
            Thread.Sleep(1000);
            Console.WriteLine("Testing...");
            driver.ReadTestCases();
            driver.ExecuteTestCases();
            driver.WriteTestResults();
            Thread.Sleep(3000);
            Console.WriteLine("Test completed.");
            Console.WriteLine("Press any key to continue...");
            Console.Read();
        }
    }
}

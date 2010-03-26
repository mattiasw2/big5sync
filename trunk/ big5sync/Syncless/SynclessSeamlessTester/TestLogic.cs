using System.Text;

namespace SynclessSeamlessTester
{
    public class TestInfo
    {
        private bool _pass;

        public TestInfo()
        {
            _pass = true;
        }

        public bool Pass
        {
            get { return _pass; }
            set { _pass = value; }
        }
    }
}

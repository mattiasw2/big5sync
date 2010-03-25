
namespace Syncless.Tagging
{
    internal class TagConfig
    {
        private bool _isSeamless;

        public bool IsSeamless
        {
            get { return _isSeamless; }
            set { _isSeamless = value; }
        }
        
        public TagConfig()
        {
            _isSeamless = true;
        }
    }
}

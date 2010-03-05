using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace Syncless.Filters
{
    public class FileSizeFilter : AbstractFileFilter
    {
        private SizeMode _sizeMode;
        private long _size;
        public FileSizeFilter(SizeMode sizeMode, long size, Mode mode)
            : base(mode)
        {
            this._sizeMode = sizeMode;
            this._size = size;
        }

        public override bool Match(string path)
        {
            if (path == null)
            {
                return false;
            }
            if (!File.Exists(path))
            {
                return false;
            }
            FileInfo file = new FileInfo(path);
            if (_sizeMode == SizeMode.MORE_THAN_EQUAL)
            {
                if (file.Length >= _size)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (file.Length <= _size)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }


            return false;
        }
        public enum SizeMode {
            MORE_THAN_EQUAL , LESS_THAN_EQUAL
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestNewComparer.CompareAndSync
{
    public abstract class CompareObject
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { this._name = value; }
        }

            
        private int[] _state;
        //0 = not exist
        //1 = exist.
        public int[] State
        {
            get { return _state; }
            set { _state = value; }
        }
        public CompareObject(string name , int count)
        {
            _name = name;
            _state = new int[count];
        }
        
        

    }
}

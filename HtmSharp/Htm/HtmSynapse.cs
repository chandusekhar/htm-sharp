using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Htm
{
    public class HtmSynapse
    {
        public bool SourceInput
        {
            get;
            set;
        }

        public double Permanance 
        { 
            get; 
            set; 
        }

        public int X { get; set; }

        public int Y { get; set; }
    }
}

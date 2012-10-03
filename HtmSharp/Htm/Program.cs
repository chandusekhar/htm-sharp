using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Htm
{
    class Program
    {
        static void Main(string[] args)
        {
            var pooler = new HtmSpatialPooler(new HtmInput { Matrix = new bool[10,5]});




            while (true)
            {
                pooler.Run();    
            }
            
            
        }
    }
}

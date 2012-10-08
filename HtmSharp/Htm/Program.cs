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
            var ran = new Random();

            var mat = new bool[20,20];

            for (int i = 0; i < mat.GetLength(0); i++)
            {
                for (int j = 0; j < mat.GetLength(1); j++)
                {
                    mat[i, j] = ran.Next(4) == 0;
                }
            }

            var pooler = new HtmSpatialPooler(new HtmInput { Matrix = mat});
            


            while (true)
            {
                pooler.Run();    
            }
            
            
        }
    }
}

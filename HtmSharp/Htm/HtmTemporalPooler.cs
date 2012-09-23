using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Htm
{
    public class HtmTemporalPooler
    {
        public void Run(IEnumerable<HtmColumn> activeColumns)
        {
            ComputeInference(activeColumns);
        }

        private void ComputeInference(IEnumerable<HtmColumn> activeColumns)
        {
            foreach (var column in activeColumns)
            {
                bool buPredicted = false;

                column.
            }
        }
    }
}

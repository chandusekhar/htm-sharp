using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Htm
{
    public class HtmDendriteSegment
    {
        #region Properties

        public IEnumerable<HtmSynapse> Synapses
        {
            get;
            private set;
        }

        #endregion

        #region Instance

        public HtmDendriteSegment(IEnumerable<HtmSynapse> synapses)
        {
            Synapses = synapses;
        }

        #endregion
    }
}

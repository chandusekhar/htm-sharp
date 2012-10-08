using System.Collections.Generic;

namespace Htm
{
    public class HtmDendriteSegment
    {
        #region Properties
        
        public bool IsSequenceSegment
        {
            get; 
            set;
        }

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

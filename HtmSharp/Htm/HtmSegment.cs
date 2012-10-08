using System.Collections.Generic;

namespace Htm
{
    public class HtmSegment
    {
        #region Properties

        public bool IsSequenceSegment
        {
            get; 
            set;
        }

        public IEnumerable<HtmLateralSynapse> Synapses
        {
            get; 
            private set;
        }

        #endregion

        #region Instance

        public HtmSegment(IEnumerable<HtmLateralSynapse> synapses)
        {
            Synapses = synapses;
        }

        #endregion
    }
}
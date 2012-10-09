using System;
using System.Collections.Generic;
using System.Linq;

namespace Htm
{
    public class HtmSegment 
    {
        private readonly int _activationTreshold;

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

        #region Methods

        
        /// <summary>
        /// This routine returns true if the number of
        /// connected synapses on segment s that are active due to the given state at
        /// time t is greater than activationThreshold. The parameter state can be
        /// activeState, or learnState.     
        /// </summary>
        public bool IsActive(HtmTime time)
        {
            var ammountConnected = Synapses.Count(synapse => synapse.IsConnected() && synapse.InputCell.GetByTime(time).ActiveState);
            return ammountConnected > _activationTreshold;
        }
      
        #endregion


        #region Instance

        public HtmSegment(IEnumerable<HtmLateralSynapse> synapses, int activationTreshold = 1)
        {
            _activationTreshold = activationTreshold;
            Synapses = synapses;
        }

        #endregion

        
    }
}
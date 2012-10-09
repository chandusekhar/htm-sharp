using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Htm.Common
{
    public class SegmentComparer : IComparer<HtmSegment>
    {
        private readonly HtmTime _time;

        public int Compare(HtmSegment firstSegment, HtmSegment secondSegment)
        {
            // 1 sequence most activity
            // 2 sequence and active
            // 3 most activity
            // 4 least activity
            
            int firstSegmentActiveCells = firstSegment.Synapses.Count(synapse => synapse.InputCell.GetByTime(_time).ActiveState);
            int secondSegmentActiveCells = secondSegment.Synapses.Count(synapse => synapse.InputCell.GetByTime(_time).ActiveState);
            
            if (firstSegment.IsSequenceSegment == secondSegment.IsSequenceSegment &&
                firstSegmentActiveCells == secondSegmentActiveCells)
            {
                return 0;
            }
            if ((firstSegment.IsSequenceSegment && !secondSegment.IsSequenceSegment) ||
                (firstSegment.IsSequenceSegment == secondSegment.IsSequenceSegment) && firstSegmentActiveCells > secondSegmentActiveCells)
            {
                return -1;
            }
            
            return 1;
        }


        #region Instance

        public SegmentComparer(HtmTime time)
        {
            _time = time;
        }

        #endregion
    }
}

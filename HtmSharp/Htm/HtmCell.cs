using System;
using System.Collections.Generic;

namespace Htm
{
    public enum HtmTime
    {
        Before = 1,
        Now = 2
    }

    public class HtmCell
    {
        private HtmCellState _oldState;
        private HtmCellState _newState;
        
        public HtmCell()
        {
            _oldState = new HtmCellState();
            _newState = new HtmCellState();
        }


        public HtmCellState GetStateByTime(HtmTime time)
        {
            switch (time)
            {
                case HtmTime.Before:
                    return _oldState;
                case HtmTime.Now:
                    return _newState;
                default:
                    throw new ArgumentOutOfRangeException("time");
            }
        }
    }

    public class HtmCellState
    {
        #region Properties


        public bool ActiveState
        {
            get;
            set;
        }

        public bool PredictiveState
        {
            get;
            set;
        }

        public IEnumerable<HtmSegment> DendriteSegments
        {
            get;
            private set;
        }

        #endregion

        #region Instance

        public HtmCellState(IEnumerable<HtmSegment> dendriteSegments)
        {
            DendriteSegments = dendriteSegments;
        }

        public HtmCellState()
        {
            DendriteSegments = new List<HtmSegment>();
        }

        #endregion
 
    }


}

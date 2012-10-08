using System.Collections.Generic;

namespace Htm
{
    public class HtmCell
    {
        #region Properties


        public HtmCellState Old
        {
            get; 
            set;
        }

        public HtmCellState New
        {
            get; 
            set;
        }

        public IEnumerable<HtmDendriteSegment> DendriteSegments
        {
            get;
            private set;
        }

        #endregion

        #region Instance

        public HtmCell(IEnumerable<HtmDendriteSegment> dendriteSegments)
        {
            DendriteSegments = dendriteSegments;
        }

        #endregion
        
    }




    public class HtmCellState
    {
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
    }
}

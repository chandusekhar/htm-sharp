using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Htm
{
    public class HtmCell
    {
        #region Properties

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
}

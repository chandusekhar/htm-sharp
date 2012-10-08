using System;
using System.Collections.Generic;

namespace Htm
{
    public class HtmTemporalPooler
    {
        #region Methods

        /**
         * Phase 1 The first phase calculates the activeState for each cell that is
         * in a winning column. For those columns, the code further selects one cell
         * per column as the learning cell (learnState). The logic is as follows: if
         * the bottom-up input was predicted by any cell (i.e. its predictiveState
         * output was 1 due to a sequence segment), then those cells become active. 
         * If that segment became active from cells chosen with learnState on, this 
         * cell is selected as the learning cell. If the bottom-up input was not 
         * predicted, then all cells in the column become active. In addition, 
         * the best matching cell is chosen as the learning cell and a new segment 
         * is added to that cell.
         */

        public void ComputeActiveState(IEnumerable<HtmColumn> activeColumns)
        {
            foreach (HtmColumn column in activeColumns)
            {
                bool buPredicted = false;

                foreach (HtmCell cell in column.Cells)
                {
                    if (cell.Old.PredictiveState)
                    {
                        HtmDendriteSegment segment = /*TODO -> cell.*/GetActiveSegment();

                        if (segment != null && segment.IsSequenceSegment)
                        {
                            buPredicted = true;
                            cell.New.ActiveState = true;
                        }
                    }
                }

                if (buPredicted == false)
                {
                    foreach (HtmCell cell in column.Cells)
                    {
                        cell.New.ActiveState = true;
                    }
                }
            }
        }

        public HtmDendriteSegment GetActiveSegment()
        {
            //TODO Implement
            throw new NotImplementedException();
        }

        /**
         * Phase 2 The second phase calculates the predictive state for each cell. A
         * cell will turn on its predictive state output if one of its segments
         * becomes active, i.e. if enough of its lateral inputs are currently active
         * due to feed-forward input. In this case, the cell queues up the following
         * changes: a) reinforcement of the currently active segment, and 
         *          b) reinforcement of a segment that could have predicted this
         * activation, i.e. a segment that has a (potentially weak) match to
         * activity during the previous time step.         
         */

        public void ComputePredictiveState(IEnumerable<HtmColumn> columns)
        {
            foreach (HtmColumn column in columns)
            {
                foreach (HtmCell cell in column.Cells)
                {
                    foreach (HtmDendriteSegment segment in cell.DendriteSegments)
                    {
                        if (/*TODO -> cell.*/IsSegmentActive(segment))
                        {
                            cell.New.PredictiveState = true;
                        }
                    }
                }
            }
        }

        private bool IsSegmentActive(HtmDendriteSegment segment)
        {
            //TODO Implement
            throw new NotImplementedException();
        }

        #endregion
    }
}
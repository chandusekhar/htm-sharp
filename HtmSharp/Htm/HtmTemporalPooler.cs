using System;
using System.Collections.Generic;
using System.Linq;

namespace Htm
{
    public class HtmTemporalPooler
    {
        #region Fields

        private readonly int _activationTreshold;

        #endregion


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
                    if (cell.GetStateByTime(HtmTime.Before).PredictiveState)
                    {
                        HtmSegment segment = /*TODO -> cell.*/GetActiveSegment(cell.GetStateByTime(HtmTime.Before));

                        if (segment != null && segment.IsSequenceSegment)
                        {
                            buPredicted = true;
                            cell.GetStateByTime(HtmTime.Now).ActiveState = true;
                        }
                    }
                }

                if (buPredicted == false)
                {
                    foreach (HtmCell cell in column.Cells)
                    {
                        cell.GetStateByTime(HtmTime.Now).ActiveState = true;
                    }
                }
            }
        }

        public HtmSegment GetActiveSegment(HtmCellState cell)
        {
            HtmSegment result = null;

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
                    foreach (HtmSegment segment in cell.GetStateByTime(HtmTime.Now).DendriteSegments)
                    {
                        if (/*TODO -> segment.*/IsSegmentActive(segment, HtmTime.Now))
                        {
                            cell.GetStateByTime(HtmTime.Now).PredictiveState = true;
                        }
                    }
                }
            }
        }


        
        /// <summary>
        /// segmentActive(s, t, state) This routine returns true if the number of
        /// connected synapses on segment s that are active due to the given state at
        /// time t is greater than activationThreshold. The parameter state can be
        /// activeState, or learnState.     
        /// </summary>
        private bool IsSegmentActive(HtmSegment segment, HtmTime time)
        {
            var ammountConnected = segment.Synapses.Count(synapse => synapse.IsConnected() && synapse.InputCell.GetStateByTime(time).ActiveState);
            return ammountConnected > _activationTreshold;
        }

        #endregion


        #region Instance

        public HtmTemporalPooler(int activationTreshold = 1)
        {
            _activationTreshold = activationTreshold;
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Htm.Common;

namespace Htm
{
    public class HtmTemporalPooler
    {
        #region Fields

        private readonly HtmSpatialPooler _spatialPooler;

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

        private void ComputeActiveState()
        {
            foreach (HtmColumn column in _spatialPooler.ActiveColumns)
            {
                bool buPredicted = false;

                foreach (HtmCell cell in column.Cells)
                {
                    if (cell.GetByTime(HtmTime.Before).PredictiveState)
                    {
                        HtmSegment segment = /*TODO -> cell.*/GetActiveSegment(cell.GetByTime(HtmTime.Before), HtmTime.Before);

                        if (segment != null && segment.IsSequenceSegment)
                        {
                            buPredicted = true;
                            cell.GetByTime(HtmTime.Now).ActiveState = true;
                        }
                    }
                }

                if (buPredicted == false)
                {
                    foreach (HtmCell cell in column.Cells)
                    {
                        cell.GetByTime(HtmTime.Now).ActiveState = true;
                    }
                }
            }
        }


        /// <summary>
        /// getActiveSegment(c, i, t, state) 
        /// For the given column c cell i, return a segment index such that 
        /// segmentActive(s,t, state) is true. If multiple segments are active, sequence 
        /// segments are given preference. Otherwise, segments with most activity 
        /// are given preference.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="time"> </param>
        /// <returns></returns>
        public HtmSegment GetActiveSegment(HtmCellState cell, HtmTime time)
        {
            var activeSegments = cell.Segments.Where(segment => segment.IsActive(time)).ToList();

            if (activeSegments.Count == 0)
            {
                return null;
            }

            activeSegments.Sort(new SegmentComparer(time));
            return activeSegments.First();
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

        private void ComputePredictiveState()
        {
            foreach (HtmColumn column in _spatialPooler.Columns)
            {
                foreach (HtmCell cell in column.Cells)
                {
                    foreach (HtmSegment segment in cell.GetByTime(HtmTime.Now).Segments)
                    {
                        if (segment.IsActive(HtmTime.Now))
                        {
                            cell.GetByTime(HtmTime.Now).PredictiveState = true;
                        }
                    }
                }
            }
        }



        private void Init()
        {
            var ran = new Random();

            foreach (var column in _spatialPooler.Columns)
            {
                var cells = new List<HtmCell>();
                for (int i = 0; i < HtmParameters.CellsPerColumns; i++)
                {
                    var newCell = new HtmCell();

                    var segments = new List<HtmSegment>();

                    for (int j = 0; j < HtmParameters.AmountOfSegments; j++)
                    {
                        var lateralSynapses = new List<HtmLateralSynapse>();

                        //var randomSynapses = new HashSet<HtmCell>();

                        //while (randomSynapses.Count <= HtmParameters.AmountOfSynapses)
                        //{
                        //    randomSynapses.Add(  ran.Next(_spatialPooler.Columns.Count()) )
                        //}

                        for (int k = 0; k < HtmParameters.AmountOfSynapses; k++)
                        {
                            lateralSynapses.Add(new HtmLateralSynapse(HtmParameters.LateralSynapseConnectedPermanance)
                                                {
                                                    Permanance = HtmParameters.InitialPermanence,
                                                });
                        }

                        segments.Add(new HtmSegment(lateralSynapses));
                    }

                    cells.Add(newCell);
                }
                column.Cells = cells;
            }

        }


        public void Run()
        {
            _spatialPooler.Run();
            ComputeActiveState();
            ComputePredictiveState();
        }

        #endregion


        #region Instance

        public HtmTemporalPooler(HtmSpatialPooler spatialPooler)
        {
            _spatialPooler = spatialPooler;
            Init();
        }

        #endregion


    }
}
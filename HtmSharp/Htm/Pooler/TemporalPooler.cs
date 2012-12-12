using System;
using System.Collections.Generic;
using System.Linq;

namespace TPooler
{
    public class TemporalPooler
    {
        private readonly bool[,,] _activeState;
        private readonly int _cellsCount;
        private readonly int _columnsCount;
        private readonly bool[,,] _predictiveState;
        private readonly IEnumerable<Segment>[,] _segments;


        public TemporalPooler(int columnsCount, int cellsCount)
        {
            _columnsCount = columnsCount;
            _cellsCount = cellsCount;

            _predictiveState = new bool[_columnsCount,_cellsCount,2];
            _activeState = new bool[_columnsCount,_cellsCount,2];
            _segments = new IEnumerable<Segment>[_columnsCount,_cellsCount];

            Initialize();
        }

        private void Initialize()
        {
        }


        //Phase 1
        //The first phase calculates the active state for each cell. 
        //For each winning column we determine which cells should become 
        //active. If the bottom-up input was predicted by any cell 
        //(i.e. its predictiveState was 1 due to a sequence segment 
        //in the previous time step), then those cells become active 
        //(lines 4-9).  If the bottom-up input was unexpected 
        //(i.e. no cells had predictiveState output on), then each cell 
        //in the column becomes active (lines 11-13).

        private void Activations(IEnumerable<int> activeColumns)
        {
            foreach (int c in activeColumns)
            {
                bool buPredicted = false;
                for (int i = 0; i < _cellsCount; i++)
                {
                    if (_predictiveState[c, i, Time.Before])
                    {
                        Segment s = GetActiveSegment(c, i, Time.Before);
                        if (s.IsSequenceSegment)
                        {
                            buPredicted = true;
                            _activeState[c, i, Time.Now] = true;
                        }
                    }
                }

                if (buPredicted == false)
                {
                    for (int i = 0; i < _cellsCount; i++)
                    {
                        _activeState[c, i, Time.Now] = true;
                    }
                }
            }
        }


        //For the given column c cell i, return a segment index such 
        //that segmentActive(s,t, state) is true. If multiple segments 
        //are active, sequence segments are given preference. Otherwise, 
        //segments with most activity are given preference.

        private Segment GetActiveSegment(int c, int i, int time)
        {
            List<Segment> activeSegmnets = _segments[c, i].Where(s => SegmentActive(c, i, s, time)).ToList();
            activeSegmnets.Sort();
            return activeSegmnets.FirstOrDefault();
        }


        //This routine returns true if the number of connected synapses on 
        //segment s that are active due to the given state at time t is 
        //greater than activationThreshold. The parameter state can be 
        //activeState, or learnState.


        private bool SegmentActive(int c, int i, Segment segment, int time)
        {
            segment.AmmountOfActiveCells = segment.Synapses.Count(
                synapse => synapse.IsConnected() && _activeState[c, i, time]);

            return segment.AmmountOfActiveCells > Parameters.ActivationThreshold;
        }


        //Phase 2
        //The second phase calculates the predictive state for each cell. 
        //A cell will turn on its predictiveState if any one of its segments 
        //becomes active, i.e. if enough of its horizontal connections are 
        //currently firing due to feed-forward input.

        private void Predictions()
        {
            for (int c = 0; c < _columnsCount; c++)
            {
                for (int i = 0; i < _cellsCount; i++)
                {
                    foreach (Segment s in _segments[c, i])
                    {
                        if (SegmentActive(c, i, s, Time.Now))
                        {
                            _predictiveState[c, i, Time.Now] = true;
                        }
                    }
                }
            }
        }

        public void Run(IEnumerable<int> activeColumns)
        {
            IterateTime();
            Activations(activeColumns);
            Predictions();
        }
        

        private void IterateTime()
        {
            Time.Switch();

            for (int c = 0; c < _columnsCount; c++)
            {
                for (int i = 0; i < _cellsCount; i++)
                {
                    _predictiveState[c, i, Time.Now] = false;
                    _activeState[c, i, Time.Now] = false;
                }
            }
        }
    }

    internal class Segment : IComparable<Segment>
    {
        public bool IsSequenceSegment { get; set; }

        public int AmmountOfActiveCells { get; set; }

        public IEnumerable<Synapse> Synapses { get; set; }

        #region IComparable<Segment> Members

        public int CompareTo(Segment obj)
        {
            if (IsSequenceSegment == obj.IsSequenceSegment &&
                AmmountOfActiveCells == obj.AmmountOfActiveCells)
            {
                return 0;
            }

            if ((IsSequenceSegment && !obj.IsSequenceSegment) ||
                ((IsSequenceSegment == obj.IsSequenceSegment) && AmmountOfActiveCells > obj.AmmountOfActiveCells))
            {
                return -1;
            }

            return 1;
        }

        #endregion
    }

    internal class Synapse
    {
        public double Permanance { get; set; }

        public bool IsConnected()
        {
            return Permanance > Parameters.ConnectedPermanance;
        }
    }

    internal class State
    {
        public static int Active;
    }

    internal class Time
    {
        public static int Now;
        public static int Before = 1;

        public static void Switch()
        {
            var value = Now;
            Now = Before;
            Before = value;
        }

    }

    internal static class Parameters
    {
        public static double ConnectedPermanance = 0.2;
        public static int ActivationThreshold = 2;
    }
}
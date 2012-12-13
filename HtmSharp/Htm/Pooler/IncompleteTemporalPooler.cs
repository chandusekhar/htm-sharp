using System;
using System.Collections.Generic;
using System.Linq;

namespace TPooler
{
    public class ComplexTemporalPooler
    {
        private readonly bool[,,] _activeState;
        private readonly int _cellsCount;
        private readonly int _columnsCount;
        private readonly bool[,,] _learnState;
        private readonly bool[,,] _predictiveState;
        private readonly List<Segment>[,] _segmentUpdateList;
        private readonly IEnumerable<Segment>[,] _segments;


        public ComplexTemporalPooler(int columnsCount, int cellsCount)
        {
            _columnsCount = columnsCount;
            _cellsCount = cellsCount;

            _predictiveState = new bool[_columnsCount,cellsCount,2];
            _activeState = new bool[_columnsCount,_cellsCount,2];
            _learnState = new bool[_columnsCount,_cellsCount,2];
            _segments = new IEnumerable<Segment>[_columnsCount,_cellsCount];
            _segmentUpdateList = new List<Segment>[_columnsCount,_cellsCount];
        }


        //Phase 1
        //The first phase calculates the activeState for each cell that is 
        //in a winning column. For those columns, the code further selects 
        //one cell per column as the learning cell (learnState). The logic 
        //is as follows: if the bottom-up input was predicted by any cell 
        //(i.e. its predictiveState output was 1 due to a sequence segment)
        //, then those cells become active (lines 23-27). If that segment 
        //became active from cells chosen with learnState on, this cell is 
        //selected as the learning cell (lines 28-30). If the bottom-up 
        //input was not predicted, then all cells in the become active 
        //(lines 32-34). In addition, the best matching cell is chosen as 
        //the learning cell (lines 36-41) and a new segment is added to
        //that cell.

        private void Activations(IEnumerable<int> activeColumns)
        {
            foreach (int c in activeColumns)
            {
                bool buPredicted = false;
                bool lcChosen = false;

                for (int i = 0; i < _cellsCount; i++)
                {
                    if (_predictiveState[c, i, Time.Before])
                    {
                        Segment s = GetActiveSegment(c, i, Time.Before, State.Active);
                        if (s.IsSequenceSegment)
                        {
                            buPredicted = true;
                            _activeState[c, i, Time.Now] = true;
                            if (SegmentActive(c, i, s, Time.Before, State.Learning))
                            {
                                lcChosen = true;
                                _learnState[c, i, Time.Now] = true;
                            }
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

                if (lcChosen == false)
                {
                    Tuple<int, Segment> match = GetBestMatchingCell(c, Time.Before);
                    int i = match.Item1;
                    Segment s = match.Item2;

                    _learnState[c, i, Time.Now] = true;
                    Segment sUpdate = GetSegmentActiveSynapses(c, i, s, Time.Before, true);
                    sUpdate.IsSequenceSegment = true;
                    _segmentUpdateList[c, i].Add(sUpdate);
                }
            }
        }


        //For the given column c cell i, return a segment index such that 
        //segmentActive(s,t, state) is true. If multiple segments are 
        //active, sequence segments are given preference. Otherwise, 
        //segments with most activity are given preference.

        private Segment GetActiveSegment(int c, int i, int time, int state)
        {
            List<Segment> activeSegments = _segments[c, i].Where(segment => SegmentActive(c, i, segment, time, state)).ToList();

            foreach (Segment segment in activeSegments)
            {
                segment.AmmountOfActiveCells = segment.CalculateAmmountOfActiveCells();
            }

            activeSegments.Sort((x, y) =>
                                {
                                    if (x.IsSequenceSegment == y.IsSequenceSegment &&
                                        x.AmmountOfActiveCells == y.AmmountOfActiveCells)
                                    {
                                        return 0;
                                    }

                                    if ((x.IsSequenceSegment && !y.IsSequenceSegment) ||
                                        ((x.IsSequenceSegment == y.IsSequenceSegment) &&
                                         x.AmmountOfActiveCells > y.AmmountOfActiveCells))
                                    {
                                        return -1;
                                    }

                                    return 1;
                                });

            return activeSegments.FirstOrDefault();
        }


        //This routine returns true if the number of connected synapses 
        //on segment s that are active due to the given state at time t 
        //is greater than activationThreshold. The parameter state can 
        //be activeState, orlearnState.

        private bool SegmentActive(int c, int i, Segment segment, int time, int state)
        {
            if (state == State.Active)
            {
                if (!_activeState[c, i, time])
                {
                    return false;
                }
            }
            if (state == State.Learning)
            {
                if (!_learnState[c, i, time])
                {
                    return false;
                }
            }

            return segment.CalculateAmmountOfActiveCells() > Parameters.ActivationThreshold;
        }


        //Phase 2
        //The second phase calculates the predictive state for each cell. 
        //A cell will turn on its predictive state output if one of its 
        //segments becomes active, i.e. if enough of its lateral inputs 
        //are currently active due to feed-forward input.  In this case, 
        //the cell queues up the following changes: a) reinforcement of 
        //the currently active segment (lines 47-48), and b) reinforcement 
        //of a segment that could have predicted this activation, i.e. a 
        //segment that has a (potentially weak) match to activity during 
        //the previous time step (lines 50-53).

        private void Predictions()
        {
            for (int c = 0; c < _columnsCount; c++)
            {
                for (int i = 0; i < _cellsCount; i++)
                {
                    foreach (Segment s in _segments[c, i])
                    {
                        if (SegmentActive(c, i, s, Time.Now, State.Active))
                        {
                            _predictiveState[c, i, Time.Now] = true;

                            Segment activeUpdate = GetSegmentActiveSynapses(c, i, s, Time.Now, false);
                            _segmentUpdateList[c, i].Add(activeUpdate);

                            Segment predSegment = GetBestMatchingSegment(c, i, Time.Before);
                            Segment predUpdate = GetSegmentActiveSynapses(c, i, predSegment, Time.Before, true);
                            _segmentUpdateList[c, i].Add(predUpdate);
                        }
                    }
                }
            }
        }


        //Return a segmentUpdate data structure containing a list of proposed 
        //changes to segment s. Let activeSynapses be the list of active synapses 
        //where the originating cells have their activeState output = 1 at time 
        //step t. (This list is empty if s = -1 since the segment doesn't exist.) 
        //newSynapses is an optional argument that defaults to false. If newSynapses 
        //is true, then newSynapseCount - count(activeSynapses) synapses are added 
        //to activeSynapses. These synapses are randomly chosen from the set of 
        //cells that have learnState output = 1 at time step t.

        private Segment GetSegmentActiveSynapses(int c, int i, Segment segment, int time, bool newSynapses)
        {
            throw new NotImplementedException();
        }


        //For the given column c cell i at time t, find the segment with the largest 
        //number of active synapses. This routine is aggressive in finding the best 
        //match. The permanence value of synapses is allowed to be below 
        //connectedPerm. The number of active synapses is allowed to be below 
        //activationThreshold, but must be above minThreshold. The routine returns 
        //the segment index. If no segments are found, then an index of -1 is returned.

        private Segment GetBestMatchingSegment(int c, int i, int time)
        {
            //TODO i don't know how to use the time variable.

            List<Segment> segments = _segments[c, i].ToList();

            foreach (Segment segment in segments)
            {
                segment.AmmountOfActiveCells = segment.CalculateAmmountOfActiveCells();
            }

            segments.Sort((x, y) =>
                          {
                              if (x.AmmountOfActiveCells == y.AmmountOfActiveCells)
                              {
                                  return 0;
                              }
                              if (x.AmmountOfActiveCells > y.AmmountOfActiveCells)
                              {
                                  return -1;
                              }

                              return 1;
                          });

            return segments.FirstOrDefault();
        }


        //For the given column, return the cell with the best matching segment 
        //(as defined above). If no cell has a matching segment, then return 
        //the cell with the fewest number of segments.

        private Tuple<int, Segment> GetBestMatchingCell(int c, int time)
        {
            var matches = new List<Tuple<int, Segment>>();
            int minSegmentSize = _segments[c, 0].Count();
            int minSegmentCellIndex = 0;

            for (int i = 0; i < _cellsCount; i++)
            {
                if (minSegmentSize > _segments[c, i].Count())
                {
                    minSegmentSize = _segments[c, i].Count();
                    minSegmentCellIndex = i;
                }

                Segment bestSegment = GetBestMatchingSegment(c, i, time);
                matches.Add(new Tuple<int, Segment>(i, bestSegment));
            }

            if (matches.Count > 0)
            {
                matches.Sort((x, y) =>
                             {
                                 if ((x.Item2 != null && y.Item2 != null) &&
                                     x.Item2.AmmountOfActiveCells == y.Item2.AmmountOfActiveCells)
                                 {
                                     return 0;
                                 }

                                 if ((x.Item2 != null && y.Item2 == null) ||
                                     ((x.Item2 != null && y.Item2 != null) &&
                                      (x.Item2.AmmountOfActiveCells > y.Item2.AmmountOfActiveCells)))
                                 {
                                     return -1;
                                 }
                                 return 1;
                             });

                return matches.FirstOrDefault();
            }

            return new Tuple<int, Segment>(minSegmentCellIndex, _segments[c, minSegmentCellIndex].FirstOrDefault());
        }


        //Phase 3
        //The third and last phase actually carries out learning. In this 
        //phase segment updates that have been queued up are actually 
        //implemented once we get feed- forward input and the cell is 
        //chosen as a learning cell (lines 56-57). Otherwise, if the cell 
        //ever stops predicting for any reason, we negatively reinforce 
        //the segments (lines 58-60).

        private void Learning()
        {
            for (int c = 0; c < _columnsCount; c++)
            {
                for (int i = 0; i < _cellsCount; i++)
                {
                    if (_learnState[c, i, Time.Now])
                    {
                        adaptSegments(_segmentUpdateList[c, i], true);
                        _segmentUpdateList[c, i].Clear();
                    }
                    else if (!_predictiveState[c, i, Time.Now] &&
                             _predictiveState[c, i, Time.Before])
                    {
                        adaptSegments(_segmentUpdateList[c, i], false);
                        _segmentUpdateList[c, i].Clear();
                    }
                }
            }
        }

        //This function iterates through a list of segmentUpdate's and reinforces 
        //each segment. For each segmentUpdate element, the following changes are 
        //performed. If positiveReinforcement is true then synapses on the active
        //list get their permanence counts incremented by permanenceInc. All other
        //synapses get their permanence counts decremented by permanenceDec. 
        //If positiveReinforcement is false, then synapses on the active list 
        //get their permanence counts decremented by permanenceDec.  
        //After this step, any synapses in segmentUpdate that do yet exist get 
        //added with a permanencecount of initialPerm.

        private void adaptSegments(List<Segment> segmentUpdate, bool positiveReinforcement)
        {
            throw new NotImplementedException();
        }

        public void Run(IEnumerable<int> activeColumns)
        {
            Activations(activeColumns);
            Predictions();
            Learning();
        }
    }

    internal class Segment
    {
        public int AmmountOfActiveCells;
        public bool IsSequenceSegment;

        public IEnumerable<Synapse> Synapses { get; set; }

        public int CalculateAmmountOfActiveCells()
        {
            return Synapses.Count(synapse => synapse.IsConnected());
        }
    }

    internal class Synapse
    {
        public double Permanance { get; set; }

        public bool IsConnected()
        {
            return Permanance > Parameters.ConnectedPermanance;
        }
    }


    internal class Time
    {
        public static int Before;
        public static int Now = 1;
    }


    internal class State
    {
        public static int Active;
        public static int Learning = 1;
    }


    internal class Parameters
    {
        public static int ActivationThreshold = 2;
        public static double ConnectedPermanance = 0.2;
    }
}
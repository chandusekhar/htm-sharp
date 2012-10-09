using System;
using System.Collections.Generic;
using System.Linq;
using Htm.Common;

namespace Htm
{
    public class HtmSpatialPooler
    {
        #region Fields

        private static readonly Random Ran = new Random();
        private List<HtmColumn> _activeColumns;
        private List<HtmColumn> _columnList;
        private double _inhibitionRadius;
        private double _inhibitionRadiusBefore;
        private HtmInput _input;

        #endregion

        #region Properties

        public IEnumerable<HtmColumn> Columns
        {
            get
            {
                return _columnList;
            }
        }

        public IEnumerable<HtmColumn> ActiveColumns
        {
            get
            {
                return _activeColumns;
            }
        }

        #endregion

        #region Methods

        public void Run()
        {
            Overlap();
            Inhibition();
            Learn();
        }

        /// <summary>
        /// Phase 1: Overlap Given an input vector, the first phase calculates the overlap of each column with that vector.
        /// The overlap for each column is simply the number of connected synapses with active inputs, multiplied by its
        /// boost. If this value is below minOverlap, we set the overlap score to zero.
        /// </summary>
        private void Overlap()
        {
            foreach (HtmColumn column in _columnList)
            {
                int overlap = column.GetConnectedSynapses().Sum(synapse => synapse.SourceInput ? 1 : 0);

                if (overlap < HtmParameters.MinimumOverlap)
                {
                    column.Overlap = 0;
                    column.AddOverlapToHistory(false);
                }
                else
                {
                    column.Overlap = overlap * column.Boost;
                    column.AddOverlapToHistory(true);
                }
            }
        }


        /// <summary>
        /// Phase 2: Inhibition The second phase calculates which columns remain as winners after the inhibition step.
        /// desiredLocalActivity is a parameter that controls the number of columns that end up winning. For example, if
        /// desiredLocalActivity is 10, a column will be a winner if its overlap score is greater than the score of the 10'th
        /// highest column within its inhibition radius.
        /// </summary>
        private void Inhibition()
        {
            _activeColumns.Clear();

            foreach (HtmColumn column in _columnList)
            {
                if ((int)Math.Round(_inhibitionRadius) != (int)Math.Round(_inhibitionRadiusBefore) || column.Neighbors == null)
                {
                    column.Neighbors = CalculateNeighBors(column);
                }

                double minLocalActivity = KthScore(column.Neighbors, HtmParameters.DesiredLocalActivity);

                if (column.Overlap > 0 && column.Overlap >= minLocalActivity)
                {
                    column.AddActivationToHistory(true);
                    _activeColumns.Add(column); // To see if needs to be unique
                }
                else
                {
                    column.AddActivationToHistory(false);
                }
            }

            Console.WriteLine("Active columns : {0}", _activeColumns.Count);
        }

        /// <summary>
        /// A list of all the columns that are within inhibitionRadius of column c
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        private IEnumerable<HtmColumn> CalculateNeighBors(HtmColumn column)
        {
            int minX = Math.Max(column.X - (int)_inhibitionRadius, 0);
            int maxX = Math.Min(column.X + (int)_inhibitionRadius, _input.Matrix.GetLength(0));

            int minY = Math.Max(column.Y - (int)_inhibitionRadius, 0);
            int maxY = Math.Min(column.Y + (int)_inhibitionRadius, _input.Matrix.GetLength(1));

            return _columnList.Where(htmColumn => htmColumn != column &&
                                                  htmColumn.X >= minX &&
                                                  htmColumn.X < maxX &&
                                                  htmColumn.Y >= minY &&
                                                  htmColumn.Y < maxY).ToList();
        }


        /// <summary>
        /// kthScore(cols, k) Given the list of columns, return the k'th highest overlap value.
        /// </summary>
        /// <param name="neighbors"></param>
        /// <param name="desiredLocalActivity"></param>
        /// <returns></returns>
        private static double KthScore(IEnumerable<HtmColumn> neighbors, int desiredLocalActivity)
        {
            IOrderedEnumerable<HtmColumn> sorted = neighbors.OrderByDescending(c => c.Overlap);
            int index = Math.Min(desiredLocalActivity, sorted.Count()) - 1;
            if (index >= 0)
            {
                return sorted.ElementAt(index).Overlap;
            }
            return 0;
        }

        /// <summary>
        /// Phase 3: Learning The third phase performs learning; it updates the permanence values of all synapses as
        /// necessary, as well as the boost and inhibition radius. The main learning rule is implemented in lines 20-26. For
        /// winning columns, if a synapse is active, its permanence value is incremented, otherwise it is decremented.
        /// Permanence values are constrained to be between 0 and 1. Lines 28-36 implement boosting. There are two separate
        /// boosting mechanisms in place to help a column learn connections. If a column does not win often enough (as
        /// measured by activeDutyCycle), its overall boost value is increased (line 30-32). Alternatively, if a column's
        /// connected synapses do not overlap well with any inputs often enough (as measured by overlapDutyCycle), its
        /// permanence values are boosted (line 34-36). Note: once learning is turned off, boost(c) is frozen. Finally, at
        /// the end of Phase 3 the inhibition radius is recomputed (line 38).
        /// </summary>
        private void Learn()
        {
            foreach (HtmColumn column in _activeColumns)
            {
                foreach (HtmForwardSynapse synapse in column.PotentialSynapses)
                {
                    if (synapse.SourceInput)
                    {
                        synapse.Permanance += HtmParameters.PermanceIncrement;
                        synapse.Permanance = Math.Min(synapse.Permanance, 1.0);
                    }
                    else
                    {
                        synapse.Permanance -= HtmParameters.PermanceIncrement;
                        synapse.Permanance = Math.Max(synapse.Permanance, 0.0);
                    }
                }
            }

            foreach (HtmColumn column in _columnList)
            {
                column.UpdateColumnBoost();
                column.UpdateSynapsePermanance(HtmParameters.ConnectedPermanence);
            }

            _inhibitionRadiusBefore = _inhibitionRadius;
            _inhibitionRadius = AverageReceptiveFieldSize();
        }

        /// <summary>
        /// averageReceptiveFieldSize() The radius of the average connected receptive field size of all the columns. The
        /// connected receptive field size of a column includes only the connected synapses (those with permanence values >=
        /// connectedPerm). This is used to determine the extent of lateral inhibition between columns.
        /// </summary>
        /// <returns></returns>
        private double AverageReceptiveFieldSize()
        {
            double receptiveFieldSizeSum = 0.0;
            int count = 0;
            foreach (HtmColumn column in _columnList)
            {
                foreach (HtmForwardSynapse synapse in column.GetConnectedSynapses())
                {
                    receptiveFieldSizeSum += Math.Sqrt(Math.Pow(Math.Abs(column.X - synapse.X), 2) + Math.Pow(Math.Abs(column.Y - synapse.Y), 2));
                    count++;
                }
            }
            return (receptiveFieldSizeSum / count);
        }


        /// <summary>
        /// Initialization Prior to receiving any inputs, the region is initialized by computing a list of initial potential
        /// synapses for each column. This consists of a random set of inputs selected from the input space. Each input is
        /// represented by a synapse and assigned a random permanence value. The random permanence values are chosen with two
        /// criteria. First, the values are chosen to be in a small range around connectedPerm (the minimum permanence value
        /// at which a synapse is considered "connected"). This enables potential synapses to become connected (or
        /// disconnected) after a small number of training iterations. Second, each column has a natural center over the
        /// input region, and the permanence values have a bias towards this center (they have higher values near the
        /// center).
        /// </summary>
        /// <param name="input"></param>
        public void Init(HtmInput input)
        {
            _input = input;
            _columnList = new List<HtmColumn>();
            _activeColumns = new List<HtmColumn>();

            var inputIndexList = new List<int>();


            for (int i = 0; i < input.Matrix.GetLength(0) * input.Matrix.GetLength(1); i++)
            {
                inputIndexList.Add(i);
            }

            IEnumerable<KMeansCluster> clusters = KMeansAlgorithm.FindMatrixClusters(input.Matrix.GetLength(0), input.Matrix.GetLength(1), HtmParameters.ColumnsCount);
            foreach (KMeansCluster cluster in clusters)
            {
                List<int> htmSynapses = inputIndexList.Shuffle(Ran).ToList();
                var synapses = new List<HtmForwardSynapse>();

                for (int j = 0; j < HtmParameters.AmountOfPotentialSynapses; j++)
                {
                    var newSynapse = new HtmForwardSynapse(HtmParameters.ConnectedPermanence)
                                     {
                                         Input = input,
                                         Y = htmSynapses[j] / input.Matrix.GetLength(0),
                                         X = htmSynapses[j] % input.Matrix.GetLength(0),
                                         Permanance = (Ran.Next(5)) / (double)10,
                                     };

                    synapses.Add(newSynapse);
                }

                _columnList.Add(new HtmColumn
                                {
                                    Y = (int)Math.Round(cluster.Location.Y),
                                    X = (int)Math.Round(cluster.Location.X),
                                    PotentialSynapses = synapses
                                });
            }


            _activeColumns = new List<HtmColumn>();
        }

        #endregion

        #region Instance

        public HtmSpatialPooler(HtmInput input)
        {
            _inhibitionRadius = HtmParameters.InhibitionRatio;
            Init(input);
        }

        #endregion
    }
}
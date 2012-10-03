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
        private List<HtmColumn> _columnList;
        private List<HtmColumn> _activeColumns;
        private readonly int _minOverlap;
        private readonly int _desiredLocalActivity;
        private readonly double _permananceInc;
        private double _inhibitionRadius;
        private double _inhibitionRadiusBefore;
        private readonly double _connectedPermanence;
        private HtmInput _input;

        #endregion

        #region Methods

        public void Run()
        {
            Overlap();
            Inhibition();
            Learn();
        }

        private void Overlap()
        {
            foreach (var column in _columnList)
            {
                var overlap = column.GetConnectedSynapses().Sum(synapse => synapse.SourceInput ? 1 : 0);

                if (overlap < _minOverlap)
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

        private void Inhibition()
        {
            _activeColumns.Clear();

            foreach (var column in _columnList)
            {
                if (Math.Round(_inhibitionRadius) != Math.Round(_inhibitionRadiusBefore) || column.Neighbors == null)
                {
                    column.Neighbors = CalculateNeighBors(column);
                }

                var minLocalActivity = KthScore(column.Neighbors, _desiredLocalActivity);

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

        private IEnumerable<HtmColumn> CalculateNeighBors(HtmColumn column)
        {
            int minX = Math.Max(column.X - (int)_inhibitionRadius, 0);
            int maxX = Math.Min(column.X + (int)_inhibitionRadius, _input.Matrix.GetLength(0));

            int minY = Math.Max(column.Y - (int)_inhibitionRadius, 0);
            int maxY = Math.Min(column.Y + (int)_inhibitionRadius, _input.Matrix.GetLength(1));

            var ret = new List<HtmColumn>();


            foreach (var htmColumn in _columnList)
            {
                if (htmColumn.X >= minX && htmColumn.X < maxX && htmColumn.Y >= minY && htmColumn.Y < maxY)
                {
                    if (htmColumn != column)
                    {
                        ret.Add(htmColumn);
                    }
                }

            }

            return ret;
        }

        private static double KthScore(IEnumerable<HtmColumn> neighbors, int desiredLocalActivity)
        {
            return neighbors.OrderByDescending(c => c.Overlap).ElementAt(Math.Min(desiredLocalActivity, neighbors.Count()) - 1).Overlap;
        }

        private void Learn()
        {
            foreach (var column in _activeColumns)
            {
                foreach (var synapse in column.PotentialSynapses)
                {
                    if (synapse.SourceInput)
                    {
                        synapse.Permanance += _permananceInc;
                        synapse.Permanance = Math.Min(synapse.Permanance, 1.0);
                    }
                    else
                    {
                        synapse.Permanance -= _permananceInc;
                        synapse.Permanance = Math.Max(synapse.Permanance, 0.0);
                    }
                }
            }

            foreach (var column in _columnList)
            {
                column.UpdateColumnBoost();
                column.UpdateSynapsePermanance(_connectedPermanence);
            }

            _inhibitionRadiusBefore = _inhibitionRadius;
            _inhibitionRadius = AverageReceptiveFieldSize();
        }

        private double AverageReceptiveFieldSize()
        {
            var receptiveFieldSizeSum = 0.0;
            var count = 0;
            foreach (var column in _columnList)
            {
                foreach (var synapse in column.GetConnectedSynapses())
                {
                    receptiveFieldSizeSum += Math.Sqrt(Math.Pow(Math.Abs(column.X - synapse.X), 2) + Math.Pow(Math.Abs(column.Y - synapse.Y), 2));
                    count++;
                }
            }
            return (receptiveFieldSizeSum / count);
        }




        public void Init(HtmInput input,
                         int columnsCount = 9,
                         int amountOfPotentialSynapses = 36)
        {

            _input = input;
            _columnList = new List<HtmColumn>();
            _activeColumns = new List<HtmColumn>();

            var inputIndexList = new List<int>();


            for (int i = 0; i < input.Matrix.GetLength(0) * input.Matrix.GetLength(1); i++)
            {
                inputIndexList.Add(i);
            }

            var clusters = KMeansAlgorithm.FindMatrixClusters(input.Matrix.GetLength(0), input.Matrix.GetLength(1), columnsCount);
            foreach (var cluster in clusters)
            {

                var htmSynapses = inputIndexList.Shuffle(Ran).ToList();
                var synapses = new List<HtmSynapse>();

                for (int j = 0; j < amountOfPotentialSynapses; j++)
                {
                    var newSynapse = new HtmSynapse
                                         {
                                             Input = input,
                                             Y = htmSynapses[j] / input.Matrix.GetLength(0),
                                             X = htmSynapses[j] % input.Matrix.GetLength(0),
                                             Permanance = (Ran.Next(5)) / (double)10,
                                         };

                    synapses.Add(newSynapse);


                }

                _columnList.Add(new HtmColumn(_connectedPermanence)
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

        public HtmSpatialPooler(HtmInput input,
                                int columnsCount = 9,
                                int amountOfPotentialSynapses = 32,
                                int minOverlap = 3,
                                int desiredLocalActivity = 1,
                                double inhibitionRadios = 5.0,
                                double permananceInc = 0.05,
                                double connectedPermanence = 0.2)
        {
            _minOverlap = minOverlap;
            _desiredLocalActivity = desiredLocalActivity;
            _permananceInc = permananceInc;
            _inhibitionRadius = inhibitionRadios;
            _connectedPermanence = connectedPermanence;


            Init(input, columnsCount, amountOfPotentialSynapses);

        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Htm.Common;

namespace Htm
{
    public class HtmSpatialPooler
    {
        #region Fields

        private List<HtmColumn> _columnList;
        private List<HtmColumn> _activeColumns;
        private List<HtmSynapse> _synapses;
        private HtmColumn[,] _columnMatrix;
        private readonly int _minOverlap;
        private readonly int _desiredLocalActivity;
        private readonly double _permananceInc;
        private double _inhibitionRadius;
        private double _inhibitionRadiusBefore;
        private readonly double _connectedPermanence;

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
                foreach (var synapse in column.GetConnectedSynapses())
                {
                    column.Overlap += synapse.SourceInput ? 1 : 0;
                }

                if (column.Overlap < _minOverlap)
                {
                    column.Overlap = 0;
                    column.AddOverlapToHistory(false);
                }
                else
                {
                    column.Overlap *= column.Boost;
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
        }

        private IEnumerable<HtmColumn> CalculateNeighBors(HtmColumn column)
        {
            int minX = Math.Max(column.X - (int)_inhibitionRadius, 0);
            int maxX = Math.Min(column.X + (int)_inhibitionRadius, (int)Math.Sqrt(_synapses.Count));

            int minY = Math.Max(column.Y - (int)_inhibitionRadius, 0);
            int maxY = Math.Min(column.Y + (int)_inhibitionRadius, (int)Math.Sqrt(_synapses.Count));

            var ret = new List<HtmColumn>();


            foreach (var htmColumn in _columnList)
            {
                if (htmColumn.X >= minX && htmColumn.X < maxX && htmColumn.Y >= minY && htmColumn.Y < maxY)
                {
                    if (htmColumn != column)
                    {
                        ret.Add(column);
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




        public void Init(int synapsesCount = 144,
                         int columnsCount = 9,
                         int amountOfPotentialSynapses = 36)
        {

            _columnList = new List<HtmColumn>();
            _activeColumns = new List<HtmColumn>();
            _synapses = new List<HtmSynapse>();

            var synapseSpaceSize = (int)Math.Sqrt(synapsesCount);
            var columnSpaceSize = (int)Math.Sqrt(columnsCount);

            var ran = new Random();

            
            for (int i = 0; i < synapsesCount; i++)
            {

                _synapses.Add(new HtmSynapse
                                     {
                                         Index = i,
                                         Y = i / synapseSpaceSize,
                                         X = i % synapseSpaceSize,
                                         Permanance = (ran.Next(5)) / (double)10,
                                         SourceInput = ran.Next(2) == 0
                                     });
            }


            for (int i = 0; i < columnsCount; i++)
            {
                var htmSynapses = _synapses.Shuffle(ran).ToList();
                var synapses = new List<HtmSynapse>();

                for (int j = 0; j < amountOfPotentialSynapses; j++)
                {
                    synapses.Add(htmSynapses[j]);
                }

                _columnList.Add(new HtmColumn(_connectedPermanence)
                                    {
                                        Y = (synapseSpaceSize / (columnSpaceSize + 1)) * (i / columnSpaceSize + 1) - 1,
                                        X = (synapseSpaceSize / (columnSpaceSize + 1)) * (i % columnSpaceSize + 1) - 1,
                                        PotentialSynapses = synapses
                                    });
            }




            _columnMatrix = new HtmColumn[columnSpaceSize, columnSpaceSize];

            for (int x = 0; x < _columnMatrix.GetLength(0); x++)
            {
                for (int y = 0; y < _columnMatrix.GetLength(1); y++)
                {
                    _columnMatrix[x, y] = _columnList[x * _columnMatrix.GetLength(0) + y];
                }
            }

            _activeColumns = new List<HtmColumn>();
        }

        #endregion

        #region Instance

        public HtmSpatialPooler(int synapsesCount = 144,
                                int columnsCount = 9,
                                int amountOfPotentialSynapses = 32,
                                int minOverlap = 2,
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


            Init(synapsesCount, columnsCount, amountOfPotentialSynapses);

        }

        #endregion
    }
}

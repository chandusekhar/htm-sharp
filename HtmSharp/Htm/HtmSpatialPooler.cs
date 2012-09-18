using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Htm
{
    public class HtmSpatialPooler
    {
        #region Fields

        private List<HtmColumn> _columnList;
        private HtmColumn[,] _columnMatrix;
        private int _columnMatrixRowCount;
        private int _columnMatrixColumnCount;

        private List<HtmColumn> _activeColumns;

        private int _minOverlap;
        private int _desiredLocalActivity;
        private double _permananceInc;
        private double _connectedPermanance;
        private double _inhibitionRadius;
        private double _inhibitionRadiusBefore;

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
                    column.Overlap += synapse.SourceInput == true ? 1 : 0;
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
            int maxX = Math.Min(column.X + (int)_inhibitionRadius, _columnMatrixColumnCount);

            int minY = Math.Max(column.Y - (int)_inhibitionRadius, 0);
            int maxY = Math.Min(column.Y + (int)_inhibitionRadius, _columnMatrixRowCount);

            var ret = new List<HtmColumn>((maxX - minX) * (maxY - minY) - 1);

            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    if (x != column.X && y != column.Y)
                    {
                        ret.Add(_columnMatrix[x, y]);
                    }
                }
            }

            return ret;
        }

        private double KthScore(IEnumerable<HtmColumn> neighbors, int desiredLocalActivity)
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
                column.UpdateSynapsePermanance(_connectedPermanance);
            }

            _inhibitionRadiusBefore = _inhibitionRadius;
            _inhibitionRadius = AverageReceptiveFieldSize();

        }

        private double AverageReceptiveFieldSize()
        {
            //TODO implement this
            return _inhibitionRadius;
        }

        #endregion

        #region Instance

        public HtmSpatialPooler(IEnumerable<HtmColumn> columns,
                                int rowCount,
                                int columnCount,
                                int minOverlap = 2,
                                int desiredLocalActivity = 1,
                                double inhibitionRadios = 5.0,
                                double connectedPermanance = 0.2,
                                double permananceInc = 0.05)
        {
            #region Argument Check

            if (_columnList == null)
            {
                throw new ArgumentNullException("columns");
            }

            if (rowCount * columnCount != columns.Count())
            {
                throw new ArgumentException("(rowCount * columnCount != columns.Count()) == true");
            }

            #endregion

            _columnList = new List<HtmColumn>(columns);
            _columnMatrix = new HtmColumn[columnCount, rowCount];

            _columnMatrixRowCount = rowCount;
            _columnMatrixColumnCount = columnCount;

            for (int x = 0; x < columnCount; x++)
            {
                for (int y = 0; y < rowCount; y++)
                {
                    _columnMatrix[x, y] = _columnList[x * columnCount + y];
                    _columnMatrix[x, y].X = x;
                    _columnMatrix[x, y].Y = y;
                }
            }

            _activeColumns = new List<HtmColumn>();

            _minOverlap = minOverlap;
            _desiredLocalActivity = desiredLocalActivity;
            _permananceInc = permananceInc;
            _inhibitionRadius = inhibitionRadios;
            _connectedPermanance = connectedPermanance;
        }

        #endregion
    }
}

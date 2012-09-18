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

        private List<HtmColumn> _columns;
        private List<HtmColumn> _activeColumns;
        

        private int _minOverlap;
        private int _desiredLocalActivity;
        private double _permananceInc;
        private double _connectedPermanance;
        private double _inhibitionRadios;
        private double _inhibitionRadiosBefore;


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
            foreach (var column in _columns)
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

            foreach (var column in _columns)
            {
                if (Math.Round(_inhibitionRadios) != Math.Round(_inhibitionRadiosBefore) || column.Neighbors == null)
                {
                    //TODO Calculate Neighbors
                    //column.Neighbors = CalculateNeighBors(column)
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

            foreach (var column in _columns)
            {             
                column.UpdateColumnBoost();
                column.UpdateSynapsePermanance(_connectedPermanance);
            }

            _inhibitionRadiosBefore = _inhibitionRadios;
            _inhibitionRadios = AverageReceptiveFieldSize();

        }

        private double AverageReceptiveFieldSize()
        {
            //TODO implement this
            return _inhibitionRadios;
        }              

        #endregion

        #region Instance

        public HtmSpatialPooler(IEnumerable<HtmColumn> columns, 
                                int minOverlap = 2, 
                                int desiredLocalActivity = 1,
                                double inhibitionRadios = 5.0,
                                double connectedPermanance = 0.2,
                                double permananceInc = 0.05)
        {
            _columns = new List<HtmColumn>(columns);
            _activeColumns = new List<HtmColumn>();

            _minOverlap = minOverlap;
            _desiredLocalActivity = desiredLocalActivity;
            _permananceInc = permananceInc;
            _inhibitionRadios = inhibitionRadios;
            _connectedPermanance = connectedPermanance;
        }

        #endregion
    }
}

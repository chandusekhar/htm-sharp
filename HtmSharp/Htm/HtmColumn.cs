using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Htm
{
    public class HtmColumn
    {
        #region Fields

        private readonly List<bool> _afterInhibationActivationHistory;
        private readonly List<bool> _beforeInhibationActivationHistory;
        private readonly int _historySize;

        #endregion

        #region Properties

        public int X
        {
            get;
            set;
        }

        public int Y
        {
            get;
            set;
        }

        public IEnumerable<HtmCell> Cells
        {
            get; 
            set;
        }

        public IEnumerable<HtmColumn> Neighbors
        {
            get;
            set;
        }

        public IEnumerable<HtmForwardSynapse> PotentialSynapses
        {
            get;
            set;
        }

        public double Boost
        {
            get;
            set;
        }

        public double Overlap
        {
            get;
            set;
        }

        public double MinimalDutyCycle
        {
            get;
            set;
        }

        public double ActiveDutyCycle
        {
            get;
            set;
        }

        public double OverlapDutyCycle
        {
            get;
            set;
        }

        #endregion

        #region Methods

        public IEnumerable<HtmForwardSynapse> GetConnectedSynapses()
        {
            return PotentialSynapses.Where(synapse => synapse.IsConnected() ).ToList();
        }

        public void AddActivationToHistory(bool state)
        {
            _afterInhibationActivationHistory.Insert(0, state);
            if (_afterInhibationActivationHistory.Count > _historySize)
            {
                _afterInhibationActivationHistory.RemoveAt(_historySize);
            }
        }

        public void AddOverlapToHistory(bool state)
        {
            _beforeInhibationActivationHistory.Insert(0, state);
            if (_beforeInhibationActivationHistory.Count > _historySize)
            {
                _beforeInhibationActivationHistory.RemoveAt(_historySize);
            }
        }

        public void UpdateColumnBoost()
        {
            MinimalDutyCycle = 0.01 * (!Neighbors.Any() ? 1 : Neighbors.Max(n => n.ActiveDutyCycle));

            ActiveDutyCycle = (double)_afterInhibationActivationHistory.Count(state => state) / _afterInhibationActivationHistory.Count();

            if (ActiveDutyCycle > MinimalDutyCycle)
            {
                Boost = 1.0;
            }
            else
            {
                Boost += MinimalDutyCycle;
            }
        }

        public void UpdateSynapsePermanance(double connectedPermanance)
        {
            OverlapDutyCycle = (double)_beforeInhibationActivationHistory.Count(state => state) / _beforeInhibationActivationHistory.Count();

            if (OverlapDutyCycle < MinimalDutyCycle)
            {
                foreach (var synapse in PotentialSynapses)
                {
                    synapse.Permanance += connectedPermanance;
                }
            }
        }

        #endregion

        #region Instance

        public HtmColumn(int historySize = 1000)
        {
            _afterInhibationActivationHistory = new List<bool>();
            _beforeInhibationActivationHistory = new List<bool>();
            
            Cells = new BindingList<HtmCell>();
            PotentialSynapses = new List<HtmForwardSynapse>();
            
            _historySize = historySize;
            Boost = 1;
        }

        #endregion


        public override string ToString()
        {
            return X + "-" + Y + "-" + Overlap;
        }
    }
}

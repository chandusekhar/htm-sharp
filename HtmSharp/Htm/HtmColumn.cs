using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Htm
{
    public class HtmColumn
    {
        #region Fields
        
        private List<bool> _afterInhibationActivationHistory;
        private List<bool> _beforeInhibationActivationHistory;

        private double _connectedPermanence;
        private int _historySize;

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

        public IEnumerable<HtmColumn> Neighbors
        {
            get;
            set;
        }

        public IEnumerable<HtmSynapse> PotentialSynapses
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

        public IEnumerable<HtmSynapse> GetConnectedSynapses()
        {
            return PotentialSynapses.Where(synapse => synapse.Permanance > _connectedPermanence).ToList();
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
            MinimalDutyCycle = 0.01 * this.Neighbors.Max(n => n.ActiveDutyCycle);

            ActiveDutyCycle = (double)_afterInhibationActivationHistory.Count(state => state == true) / _afterInhibationActivationHistory.Count();
            
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
            OverlapDutyCycle = (double)_beforeInhibationActivationHistory.Count(state => state == true) / _beforeInhibationActivationHistory.Count();

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

        public HtmColumn(double connectedPermanence = 0.2, int historySize = 1000)
        {
            _afterInhibationActivationHistory = new List<bool>();
            _beforeInhibationActivationHistory = new List<bool>();

            PotentialSynapses = new List<HtmSynapse>();

            _connectedPermanence = connectedPermanence;
            _historySize = historySize;
        }

        #endregion
    }
}

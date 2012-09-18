using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Htm
{
    public class HtmColumn
    {

        private List<bool> _afterInhibationActivationHistory;
        private List<bool> _beforeInhibationActivationHistory;
                
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
                
        public IEnumerable<HtmSynapse> GetConnectedSynapses()
        {
            //TODO change 0.2 with a parameter
            return PotentialSynapses.Where(synapse => synapse.Permanance > 0.2).ToList();
        }

        
        public void AddActivationToHistory(bool state)
        {
            _afterInhibationActivationHistory.Insert(0, state);
            if (_afterInhibationActivationHistory.Count > 1000)
            {
                _afterInhibationActivationHistory.RemoveAt(1000);
            }
        }

        public void AddOverlapToHistory(bool state)
        {
            _beforeInhibationActivationHistory.Insert(0, state);
            if (_beforeInhibationActivationHistory.Count > 1000)
            {
                _beforeInhibationActivationHistory.RemoveAt(1000);
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
        

        #region Instance

        public HtmColumn()
        {
            _afterInhibationActivationHistory = new List<bool>();
            _beforeInhibationActivationHistory = new List<bool>();

            PotentialSynapses = new List<HtmSynapse>();            
        }

        #endregion
    }
}

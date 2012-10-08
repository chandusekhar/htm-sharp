namespace Htm
{
    public class HtmLateralSynapse
    {
        #region Fields

        private readonly double _connectedPermanence;

        #endregion

        #region Properties

        public HtmCell InputCell
        {
            get; 
            set;
        }

        public double Permanance
        {
            get;
            set;
        }

        #endregion

        #region Methods

        public bool IsConnected()
        {
            return Permanance > _connectedPermanence;
        }

        #endregion

        #region Instance

        public HtmLateralSynapse(double connectedPermanence)
        {
            _connectedPermanence = connectedPermanence;
        }

        #endregion
    }
}
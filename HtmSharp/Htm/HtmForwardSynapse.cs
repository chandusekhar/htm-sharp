namespace Htm
{
    public class HtmForwardSynapse
    {
        #region Fields

        private readonly double _connectedPermanence;

        #endregion

        #region Properties

        public bool SourceInput
        {
            get
            {
                return Input.Matrix[X, Y];
            }
        }

        public HtmInput Input
        {
            get; 
            set;
        }

        public double Permanance
        {
            get; 
            set;
        }

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

        #endregion

        #region Methods

        public bool IsConnected()
        {
            return Permanance > _connectedPermanence;
        }

        public override string ToString()
        {
            return X + "-" + Y + "-" + Permanance;
        }

        #endregion

        #region Instance

        public HtmForwardSynapse(double connectedPermanence)
        {
            _connectedPermanence = connectedPermanence;
        }

        #endregion
    }
}
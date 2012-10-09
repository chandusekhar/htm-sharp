namespace Htm
{
    public class HtmRegion
    {
        #region Fields
        
        private readonly HtmTemporalPooler _temporalPooler;

        #endregion

        #region Instance

        public HtmRegion(HtmInput input)
        {
            _temporalPooler = new HtmTemporalPooler(new HtmSpatialPooler(input));
        }

        #endregion

        #region Methods

        public void Run()
        {
            _temporalPooler.Run();
        }

        #endregion
    }
}
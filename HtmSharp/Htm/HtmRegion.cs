namespace Htm
{
    public class HtmRegion
    {
        #region Fields

        private readonly HtmSpatialPooler _spatialPooler;
        private readonly HtmTemporalPooler _temporalPooler;

        #endregion

        #region Instance

        public HtmRegion(HtmInput input)
        {
            _spatialPooler = new HtmSpatialPooler(input);
            _temporalPooler = new HtmTemporalPooler();
        }

        #endregion

        #region Methods

        public void Run()
        {
            _spatialPooler.Run();
            _temporalPooler.ComputeActiveState(_spatialPooler.ActiveColumns);
            _temporalPooler.ComputePredictiveState(_spatialPooler.Columns);
        }

        #endregion
    }
}
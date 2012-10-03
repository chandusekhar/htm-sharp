using System.Collections.Generic;

namespace Htm.Common
{
    public class KMeansCluster
    {
        public KMeansCluster()
        {
            AssignedInputs = new List<KMeansPoint>();
        }

        public KMeansPoint Location { get; set; }

        public List<KMeansPoint> AssignedInputs { get; set; }
    }
}
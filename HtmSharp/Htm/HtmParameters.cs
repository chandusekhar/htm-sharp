namespace Htm
{
    public static class HtmParameters
    {
        /*
         * 
         * Spatial pooler parameters 
         * 
         */

        /// <summary>
        /// The number of columns
        /// </summary>
        public static int ColumnsCount = 9;
        
        /// <summary>
        /// The number of synapses used by each column as input
        /// </summary>
        public static int AmountOfPotentialSynapses = 32;

        /// <summary>
        /// A minimum number of inputs that must be active for a column to be considered during the inhibition step.
        /// </summary>
        public static int MinimumOverlap = 3;

        /// <summary>
        /// A parameter controlling the number of columns that will be winners after the inhibition step
        /// </summary>
        public static int DesiredLocalActivity = 1;

        /// <summary>
        /// Starting value for : Average connected receptive field size of the columns.
        /// </summary>
        public static double InhibitionRatio = 5.0;

        /// <summary>
        /// Amount permanence values of forward synapses are incremented during spatial pooler learning
        /// </summary>
        public static double PermanceIncrement = 0.05;
        
        /// <summary>
        /// If the permanence value for a forward synapse is greater than this value, it is said to be connected.
        /// </summary>
        public static double ConnectedPermanence = 0.2;

        /*
         * 
         * Temporal pooler parameters
         * 
         */
        
        /// <summary>
        /// Initial permanence value for a lateral synapse.
        /// </summary>
        public static double InitialPermanence = 0.4;

        /// <summary>
        /// Amount of dendrite segments in a cell
        /// </summary>
        public static int AmountOfSegments = 10;

        /// <summary>
        /// Amount of synapses in a dendrite segment
        /// </summary>
        public static int AmountOfSynapses = 30;

        /// <summary>
        /// Amount of Cells in a column
        /// </summary>
        public static int CellsPerColumns = 3;

        /// <summary>
        /// If the permanence value for a lateral synapse is greater than this value, it is said to be connected.
        /// </summary>
        public static double LateralSynapseConnectedPermanance = 0.5;


    }
}
    
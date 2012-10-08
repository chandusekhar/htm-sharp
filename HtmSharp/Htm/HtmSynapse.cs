namespace Htm
{
    public class HtmSynapse
    {
        public bool SourceInput
        {
            get
            {
                return Input.Matrix[X, Y];
            }
        }

        public HtmInput Input { get; set; }

        public double Permanance
        {
            get;
            set;
        }

        public int X { get; set; }

        public int Y { get; set; }


        public override string ToString()
        {
            return X + "-" + Y + "-" + Permanance;
        }
    }
}

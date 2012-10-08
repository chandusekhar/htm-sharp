using System;

namespace Htm.Common
{
    public class KMeansPoint
    {
        public KMeansPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public KMeansPoint()
        {
        }

        public double X { get; set; }

        public double Y { get; set; }

        public double CalculateDistance(KMeansPoint point)
        {
            return Math.Sqrt(Math.Pow(Math.Abs(X - point.X), 2) + Math.Pow(Math.Abs(Y - point.Y), 2));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (KMeansPoint)) return false;
            return Equals((KMeansPoint) obj);
        }

        public bool Equals(KMeansPoint other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.X.Equals(X) && other.Y.Equals(Y);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode()*397) ^ Y.GetHashCode();
            }
        }

        public override string ToString()
        {
            return X + "-" + Y;
        }
    }
}
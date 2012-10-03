using System;
using System.Collections.Generic;
using System.Linq;

namespace Htm.Common
{
    public class KMeansAlgorithm
    {
        private static readonly Random Ran = new Random();

        public static IEnumerable<KMeansCluster> FindMatrixClusters(int matX, int matY, int clusterCount)
        {
            //Generating inputs
            var input = new List<KMeansPoint>();
            for (int i = 0; i < matX; i++)
            {
                for (int j = 0; j < matY; j++)
                {
                    input.Add(new KMeansPoint(i, j));
                }
            }

            //Generating clusters
            var clusters = new List<KMeansCluster>();
            for (int i = 0; i < clusterCount; i++)
            {
                var location = new KMeansPoint(Ran.Next(matX), Ran.Next(matY));

                while (clusters.Any(item => location.Equals(item)))
                {
                    location = new KMeansPoint(Ran.Next(matX), Ran.Next(matY));
                }
                clusters.Add(new KMeansCluster {Location = location});
            }

            bool running = true;
            while (running)
            {
                //Assing inputs to clusters
                foreach (KMeansCluster cluster in clusters)
                {
                    cluster.AssignedInputs.Clear();
                }
                foreach (KMeansPoint point in input)
                {
                    double minDistance = double.MaxValue;

                    KMeansCluster minCluster = null;
                    foreach (KMeansCluster cluster in clusters)
                    {
                        double distance = cluster.Location.CalculateDistance(point);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            minCluster = cluster;
                        }
                    }

                    if (minCluster != null)
                    {
                        minCluster.AssignedInputs.Add(point);
                    }
                }

                running = false;

                //Recalculate cluster positions
                foreach (KMeansCluster cluster in clusters)
                {
                    double newX = 0;
                    double newY = 0;

                    foreach (KMeansPoint point in cluster.AssignedInputs)
                    {
                        newX += point.X;
                        newY += point.Y;
                    }

                    newX = newX/cluster.AssignedInputs.Count;
                    newY = newY/cluster.AssignedInputs.Count;

                    if (Math.Abs(cluster.Location.X - newX) > 0.001 || Math.Abs(cluster.Location.Y - newY) > 0.001)
                    {
                        running = true;
                    }

                    cluster.Location.X = newX;
                    cluster.Location.Y = newY;
                }
            }


            return clusters;
        }
    }
}
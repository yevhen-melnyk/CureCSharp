using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cure
{
    class Program
    {
        static List<Bag> freeBags = new List<Bag>();
        static List<Cluster> clusters = new List<Cluster>();
        public static Random r = new Random(DateTime.Now.Millisecond);

        static void Main(string[] args)
        {
            //Cluster count that we aim to achieve
            int ClusterCount = 7;
            //Number of bags that will be randomly chosen to form the initial clusters
            int selectionBagCount = 50;
            //Number of bags that will represent each cluster during final cluster filling
            int representativesCount = 5;
            //Amount of CURE algorithm "shrink", in other words, how much will the representatives shift towards the clusteroid
            double shrinkAmount = 0.2;
            //Total amount of data
            int dataCount = 400;

            //Fill our dataset with random bags
            AddDummyData(dataCount);
            List<Bag> freeSelectionBags = new List<Bag>();

            #region Select some points to create clusters
            //select several random bags to work with
            for (int i = 0; i < selectionBagCount; i++)
            {
                int index = r.Next(0, freeBags.Count - 1);
                Bag bag = freeBags[index];
                freeSelectionBags.Add(bag);
                freeBags.Remove(bag);
            }
            #endregion

            #region Create initial clusters (hierarchial)
            //find all pair-wise bags distances
            Dictionary<Tuple<Bag, Bag>, double> bagDistances = new Dictionary<Tuple<Bag, Bag>, double>();
            foreach (Bag A in freeSelectionBags)
                foreach (Bag B in freeSelectionBags)
                {
                    if (A == B) continue;
                    bagDistances.Add(new Tuple<Bag, Bag>(A, B), Bag.Distance(A, B));
                }
            while (freeSelectionBags.Count > 1)
            {
                //find lowest distance between bags and merge those bags
                var lowestDistance = bagDistances.OrderBy(x => x.Value).FirstOrDefault();
                var newCluster = new Cluster();
                var bagA = lowestDistance.Key.Item1;
                var bagB = lowestDistance.Key.Item2;
                newCluster.bags.Add(bagA);
                newCluster.bags.Add(bagB);
                clusters.Add(newCluster);

                //selected bags are not free anymore
                freeSelectionBags.Remove(bagA);
                freeSelectionBags.Remove(bagB);

                //remove distances concerning those two bags we merged
                var distToRemove = bagDistances.Where(x =>
                    x.Key.Item1 == lowestDistance.Key.Item1 ||
                    x.Key.Item1 == lowestDistance.Key.Item2 ||
                    x.Key.Item2 == lowestDistance.Key.Item1 ||
                    x.Key.Item2 == lowestDistance.Key.Item2
                ).ToList();
                foreach (var d in distToRemove)
                    bagDistances.Remove(d.Key);
            }
            #endregion

            #region Merge clusters
            //reducing cluster count by merging existing clusters
            while (clusters.Count > ClusterCount)
            {
                Dictionary<Tuple<Cluster, Cluster>, double> clusterDistances = new Dictionary<Tuple<Cluster, Cluster>, double>();
                foreach (Cluster A in clusters)
                    foreach (Cluster B in clusters)
                    {
                        if (A == B) continue;
                        clusterDistances.Add(new Tuple<Cluster, Cluster>(A, B), Cluster.Distance(A, B));
                    }

                //find lowest distance between cluster centroids and merge those clusters
                var lowestDistance = clusterDistances.OrderBy(x => x.Value).FirstOrDefault();
                var clusterA = lowestDistance.Key.Item1;
                var clusterB = lowestDistance.Key.Item2;
                var newCluster = Cluster.Merge(clusterA, clusterB);
                clusters.Add(newCluster);
                clusters.Remove(clusterA);
                clusters.Remove(clusterB);

                //remove distances concerning those two bags we merged
                var distToRemove = clusterDistances.Where(x =>
                    x.Key.Item1 == lowestDistance.Key.Item1 ||
                    x.Key.Item1 == lowestDistance.Key.Item2 ||
                    x.Key.Item2 == lowestDistance.Key.Item1 ||
                    x.Key.Item2 == lowestDistance.Key.Item2
                ).ToList();
                foreach (var d in distToRemove)
                    clusterDistances.Remove(d.Key);

            }

            //make all clusters generate their representatives
            foreach (Cluster c in clusters)
            {
                c.CreateRepresentatives(representativesCount, shrinkAmount);
            }

            //move all free bags to some clusters
            while (freeBags.Count > 0)
            {
                //select random remaining bag
                var index = Program.r.Next(0, freeBags.Count);
                var bag = freeBags.ElementAt(index);
                //get all distances to clusters (their closest representatives actually)
                Dictionary<Cluster, double> distances = new Dictionary<Cluster, double>();
                foreach (Cluster c in clusters)
                {
                    distances.Add(c, c.MinRepresDistance(bag));
                }
                var mindistance = distances.OrderBy(x => x.Value).FirstOrDefault();
                var clusterToMerge = mindistance.Key;
                clusterToMerge.bags.Add(bag);
                freeBags.Remove(bag);

            }

            #endregion
            Console.WriteLine("Number of clusters: " + clusters.Count);
            for (int i = 0; i < clusters.Count; i++)
            {
                Cluster c = clusters[i];
                Console.WriteLine("Cluster description " + i);
                Console.WriteLine(c.Description());
            }
          

            Console.ReadLine();

        }

        static void AddDummyData(int count)
        {
            List<string> products = new List<string>() { "cabbage", "potato", "cheese", "meat", "milk", "bread", "eggs", "tomatoes", "ketchup", "water", "salad", "broccoli" };
            for (int i = 0; i < count; i++)
            {
                var newBag = new Bag();
                for (int j = 0; j < 6; j++)
                {
                    int index = r.Next(0, products.Count - 1);
                    string product = products[index];
                    if (!newBag.items.Contains(product))
                        newBag.items.Add(product);
                    else
                        j--;

                }
                freeBags.Add(newBag);
            }

        }

        //Find average distances between bags inside given cluster
        public static double MedDist(Cluster c)
        {
            Dictionary<Tuple<Bag, Bag>, double> cDistances = new Dictionary<Tuple<Bag, Bag>, double>();
            foreach (Bag A in c.bags)
                foreach (Bag B in c.bags)
                {
                    if (A == B) continue;
                    cDistances.Add(new Tuple<Bag, Bag>(A, B), Bag.Distance(A, B));
                }

            double medDist = 0;
            foreach (var d in cDistances)
                medDist += d.Value;
            medDist /= cDistances.Count();
            return medDist;
        }

        //merge two clusters. Why isn't this in Cluster class?
        static void Merge(Cluster A, Cluster B)
        {
            if (clusters.Contains(A) && clusters.Contains(B))
            {
                clusters.Add(Cluster.Merge(A, B));
                clusters.Remove(A);
                clusters.Remove(B);
            }
            else throw new Exception("One or both clusters aren't found in the data set.");
        }
    }
}

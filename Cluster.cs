using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cure
{
    class Cluster
    {
        public List<Bag> bags= new List<Bag>();

        public List<FuzzyBag> representatives = new List<FuzzyBag>();

        public static Cluster Merge(Cluster A, Cluster B)
        {
            var res = new Cluster();
            res.bags.AddRange(A.bags);
            res.bags.AddRange(B.bags);
            return res;
        }

        public FuzzyBag Centroid()
        {
            Random random = new Random(DateTime.Now.Millisecond);
            var res = new FuzzyBag();
            Dictionary<string, int> counts = new Dictionary<string, int>();
            Dictionary<string, double> chances = new Dictionary<string, double>();
            HashSet<string> products = new HashSet<string>();

            foreach (var r in bags)
            {
                foreach (var p in r.items)
                {
                    if (!counts.ContainsKey(p))
                        counts.Add(p, 1);
                    else {
                        counts[p]++;
                    }
                }
            }

            foreach (var count in counts)
            {
                chances.Add(count.Key, (double)count.Value / (double)bags.Count);
            }

            foreach (var chance in chances) {
                if (random.NextDouble() <= chance.Value) {
                    res.items.Add(chance.Key, chance.Value);
                }
            }

            return res;
        }

        //Jaccar distance between centroids
        public static double Distance(Cluster A, Cluster B) {
            double JaccardIndex = Bag.Distance(A.Centroid(), B.Centroid());
            return JaccardIndex;
        }

        //Lowest distance between representatives
        public double MinRepresDistance(Bag bag) {
            var a = from r in representatives select Bag.Distance(bag, r);
            double minDist = a.OrderBy(x => x).FirstOrDefault();
            return minDist;
        }

        public void CreateRepresentatives(int count, double shrink) {
            //select the needed amount of entry points
            var staged = new List<Bag>();
            while (staged.Count < count && staged.Count<bags.Count) {
                var index = Program.r.Next(0, bags.Count);
                var sel = bags.ElementAt(index);
                if (!staged.Contains(sel))
                    staged.Add(sel);
            }

            foreach (var s in staged) {
                FuzzyBag f = s.CloseIn(this.Centroid(), shrink);
                representatives.Add(f);
            }
            
        }

        //Form a brief cluster description
        public string Description() {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Distinctive cluster characteristics: ");
            FuzzyBag centroid = Centroid();
            var standartLength = builder.Length;
            foreach (var item in centroid.items) {
                if (item.Value > 0.85)
                {
                    builder.AppendLine("    Very high presence of " + item.Key + " — " + Math.Round(item.Value,2)*100 + "%");
                    continue;
                }
                if (item.Value > 0.75)
                {
                    builder.AppendLine("    High presence of " + item.Key + " — " + Math.Round(item.Value,2)*100 + "%");
                    continue;
                }
                if (item.Value < 0.20)
                {
                    builder.AppendLine("    Very low presence of " + item.Key + " — " + Math.Round(item.Value,2)*100 + "%");
                    continue;
                }
                if (item.Value < 0.40)
                {
                    builder.AppendLine("    Low presence of " + item.Key + " — " + Math.Round(item.Value,2)*100 + "%");
                    continue;
                }
                
            }
            if (builder.Length == standartLength)
                builder.AppendLine("Cluster does not have any definitive characteristics. It's fairly likely that this cluster contains bags that don't really fit into other clusters. If the number of bags here is large then it would be wise to consider increasing the cluster number.");
            builder.AppendLine("Total bags count: " + bags.Count);
            return builder.ToString();
        }
    }
}

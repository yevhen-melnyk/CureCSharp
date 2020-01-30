using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cure
{
    class Bag
    {
        public List<string> items = new List<string>();



        //Jaccard distance
        public static double Distance(Bag A, Bag B)
        {
            var CommonProducts = from a in A.items.AsEnumerable<string>()
                                 join b in B.items.AsEnumerable<string>() on a equals b
                                 select a;
            double JaccardIndex = (((double)CommonProducts.Count()) /
                                   ((double)(A.items.Count() + B.items.Count())));
            return JaccardIndex;
        }

        public static double Distance(Bag A, FuzzyBag F)
        {
            var CommonProducts = from a in A.items.AsEnumerable<string>()
                                 join f in F.items.AsEnumerable<KeyValuePair<string, double>>() on a equals f.Key
                                 select f.Value;
            double JaccardIndex = (((double)CommonProducts.Sum()) /
                                   ((double)(A.items.Count() + F.items.Count())));
            return JaccardIndex;
        }

        public static double Distance(FuzzyBag A, FuzzyBag B)
        {
            var CommonProducts = from a in A.items.AsEnumerable<KeyValuePair<string, double>>()
                                 join b in B.items.AsEnumerable<KeyValuePair<string, double>>() on a.Key equals b.Key
                                 select a.Value*b.Value;
            double JaccardIndex = (((double)CommonProducts.Sum()) /
                                   ((double)(A.items.Count() + B.items.Count())));
            return JaccardIndex;
        }

        // Shift this bag to the given clusteroid by given amount, result is a new FuzzyBag.
        public FuzzyBag CloseIn( FuzzyBag F, double amount)
        {
            Bag A = this;
            var res = new FuzzyBag();
            foreach (string p in A.items)
            {
                if (F.items.ContainsKey(p))
                {
                    var dist = (1d - F.items[p]) * amount;
                    res.items.Add(p, 1 - dist);
                }
            }
            return res;
        }
    }

    // A class for representing centroids (clusteroids)
    // Products here are represented by their average presence (0.0-1.0)
    class FuzzyBag
    {
        public Dictionary<string, double> items = new Dictionary<string, double>();


    }
}

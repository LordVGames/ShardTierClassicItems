using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShardTierClassicItems
{
    internal static class Extensions
    {
        // copied from survariants

        /// <summary>Gets a random element from the collection</summary>
        /// <param name="rng">the Xoroshiro128Plus instance</param>
        /// <returns>the chosen element</returns>
        public static T GetRandom<T>(this IEnumerable<T> self, Xoroshiro128Plus rng)
        {
            return self.ElementAt(rng.RangeInt(0, self.Count()));
        }
    }
}
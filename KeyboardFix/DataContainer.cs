using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardFix
{
    public class DataContainer
    {
        private Dictionary<string, int> blockedCounts = new Dictionary<string, int>();
        private int totalBlocks = 0;

        public void IncrementBlockCount(string key)
        {
            if (blockedCounts.ContainsKey(key))
            {
                blockedCounts[key]++;
            }
            else
            {
                blockedCounts[key] = 1;
            }
            totalBlocks++;
        }

        public Dictionary<string,int> getBlockedCountsDict()
        {
            return blockedCounts;
        }

    }
}

using System.Collections.Generic;
using SObject = StardewValley.Object;

namespace ExtendedFridge
{
    public class PlayerData
    {
        public IList<int[]> PItems { get; set; } = new List<int[]>();
        //public Tuple<int,int,int> pItems { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using SObject = StardewValley.Object;

namespace ExtendedFridge
{
    public class PlayerData
    {
        public IList<int[]> pItems { get; set; } = new List<int[]>();
        //public Tuple<int,int,int> pItems { get; set; }
    }
}

using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace M007_ExtendedFridge
{
    public class FridgeModConfig
    {
        public string fridgePrevPageKey {get; set;}
        public string fridgeNextPageKey {get; set;}
        public bool autoSwitchPageOnGrab {get; set;}

        public FridgeModConfig()
        {
            fridgeNextPageKey = "Right";
            fridgePrevPageKey = "Left";
            autoSwitchPageOnGrab = true;
        }

    }

}
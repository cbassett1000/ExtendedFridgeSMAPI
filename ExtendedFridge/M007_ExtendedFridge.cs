using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.IO;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using xTile.Dimensions;
using SObject = StardewValley.Object;

namespace ExtendedFridge
{
    public class M007_ExtendedFridge_Mod : Mod
    {
        private static FridgeChest _fridge;
        private static FridgeModConfig config;
        internal static ISemanticVersion Version;
        private static bool IsInFridgeMenu;
        private static readonly int FRIDGE_TILE_ID = 173;
        private PlayerData pData;
        private string cfgPath;

        public override void Entry(IModHelper helper)
        {
            var modPath = Helper.DirectoryPath;
            config = Helper.ReadConfig<FridgeModConfig>();
            Version = ModManifest.Version;


            MenuEvents.MenuChanged += Event_MenuChanged;

            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            SaveEvents.BeforeSave += SaveEvents_BeforeSave;
            PlayerEvents.Warped += PlayerEvents_Warped;
            ControlEvents.KeyReleased += Event_KeyReleased;
            Monitor.Log("ExtendedFridge Entry");
        }

        public void CheckForAction(bool isBack = false)
        {
            int a = 5;
        }

        public void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            //Set Up Player Data
            this.cfgPath = Path.Combine("data", $"{Constants.SaveFolderName}.json");
            this.pData = this.Helper.ReadJsonFile<PlayerData>(this.cfgPath) ?? new PlayerData();
            this.Helper.WriteJsonFile(this.cfgPath, pData);
            
            //Lets try to load up the items now.
            if (this.pData.pItems == null || this.pData.pItems.Count < 1)
                return;
            //Load up the FarmHouse
            FarmHouse h = (FarmHouse)Game1.currentLocation;
            if (h == null)
                return;
            if (h.fridge.Value.items.Count > 0)
                h.fridge.Value.items.Clear();
            //Passed the count, now we move on to adding them
            foreach (int[] i in this.pData.pItems)
            {
                h.fridge.Value.items.Add(new SObject(i[0], i[1], false, -1, i[2]));
            }
        }

        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {
            FarmHouse h = (FarmHouse)Game1.currentLocation;
            if (h == null)
                return;
            //this.pData.pItems = _fridge.items;
            pData.pItems?.Clear();
            if(this.pData.pItems == null)
                this.pData.pItems = new List<int[]>();
            foreach (Item i in _fridge.items)
            {
                if (i is SObject obj)
                {
                    
                    this.pData.pItems.Add(new int[]{i.ParentSheetIndex, i.Stack, obj.Quality});
                }
            }
            this.Helper.WriteJsonFile(this.cfgPath, this.pData);
        }

        private void Event_KeyReleased(object send, EventArgsKeyPressed e)
        {
            if (e.KeyPressed.ToString().Equals(config.fridgeNextPageKey) && Game1.activeClickableMenu is FridgeGrabMenu)
            {
                _fridge.MovePageToNext();
            }

            if (e.KeyPressed.ToString().Equals(config.fridgePrevPageKey) && Game1.activeClickableMenu is FridgeGrabMenu)
            {
                _fridge.MovePageToPrevious();
            }
        }

        private void PlayerEvents_Warped(object send, EventArgsPlayerWarped e)
        {
            var priorlocation = e.PriorLocation;
            if (e.NewLocation is FarmHouse)
            {
                FarmHouse ptrFH = (FarmHouse)Game1.currentLocation;
            }
        }

        private void Event_MenuChanged(object send, EventArgsClickableMenuChanged e)
        {
            Vector2 lastGrabbedTile = Game1.player.lastGrabTile;
            //Log.Debug("M007_ExtendedFridge Event_MenuChanged HIT", new object[0]);
            /*
            if (Game1.currentLocation is FarmHouse)
            {
                this.Monitor.Log(String.Format("M007_ExtendedFridge lastGrabTileX:{0} lastGrabTileY:{1}", (int)Game1.player.lastGrabTile.X, (int)Game1.player.lastGrabTile.Y));
            }*/

            if (ClickedOnFridge())
            {

                IsInFridgeMenu = true;
                if (e.NewMenu is ItemGrabMenu ptrMenu)
                {
                    if (_fridge == null || _fridge.items.Count == 0)
                    {
                        _fridge = new FridgeChest(config.autoSwitchPageOnGrab);
                        FarmHouse h = (FarmHouse)Game1.currentLocation;
                        _fridge.items.AddRange(h.fridge.Value.items);
                    }
                    _fridge.ShowCurrentPage();
                    // this.Monitor.Log("M007_ExtendedFridge Fridge HOOKED");
                }

            }
            if (e.NewMenu is CraftingPage cp && Game1.currentLocation is FarmHouse fh)
            {
                if (_fridge != null)
                {
                    fh.fridge.Value.items.Clear();
                    fh.fridge.Value.items.AddRange(_fridge.items);
                    _fridge.items.Clear();
                }

            }


        }

        void runConfig()
        {
            var myconfig = Helper.ReadConfig<FridgeModConfig>();
        }

        public void grabItemFromChest(Item item, Farmer who)
        {
            _fridge.grabItemFromChest(item, who);
        }

        public void grabItemFromInventory(Item item, Farmer who)
        {
            _fridge.grabItemFromInventory(item, who);
        }

        private bool ClickedOnFridge()
        {
            if (Game1.currentLocation is FarmHouse ptrFarmhouse)
            {
                Location tileLocation = new Location((int)Game1.player.lastGrabTile.X, (int)Game1.player.lastGrabTile.Y);

                if (ptrFarmhouse.map.GetLayer("Buildings").Tiles[tileLocation] != null)
                {
                    int currTileIdx = ptrFarmhouse.map.GetLayer("Buildings").Tiles[tileLocation].TileIndex;

                    return currTileIdx == FRIDGE_TILE_ID;
                }
            }

            return false;
        }


        public static List<Item> organize()
        {
            List<Item> outItem;
            if (_fridge != null)
                outItem = _fridge.items;
            else
                outItem = null;
            return outItem;
        }

        public static string showMessage()
        {
            //return 
            string outPut = "";
            if (_fridge != null)
                outPut =
                    $"Extended Fridge {M007_ExtendedFridge_Mod.Version} Current Page: 0 | {_fridge.items.Count} items in fridge.";
            return outPut;
        }
    }
}
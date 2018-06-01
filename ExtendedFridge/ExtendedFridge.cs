using System;
using System.Collections.Generic;
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
    public class ExtendedFridgeMod : Mod
    {
        private static FridgeChest _fridge;
        private static FridgeModConfig _config;
        internal static ISemanticVersion Version;
        //private static bool _isInFridgeMenu;
        private static readonly int FRIDGE_TILE_ID = 173;
        private PlayerData _pData;
        private string _cfgPath;
        //Create instance so we can get monitor.
        public static ExtendedFridgeMod Instance;

        public override void Entry(IModHelper helper)
        {
            _config = Helper.ReadConfig<FridgeModConfig>();
            Version = ModManifest.Version;
            //Set instance
            Instance = this;

            MenuEvents.MenuChanged += Event_MenuChanged;

            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            SaveEvents.BeforeSave += SaveEvents_BeforeSave;
            PlayerEvents.Warped += PlayerEvents_Warped;
            ControlEvents.KeyReleased += Event_KeyReleased;
            Monitor.Log("ExtendedFridge Entry");
        }

        public void CheckForAction(bool isBack = false)
        {
            //int a = 5;
        }

        public void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            //Set Up Player Data
            _cfgPath = Path.Combine("data", $"{Constants.SaveFolderName}.json");
            _pData = Helper.ReadJsonFile<PlayerData>(_cfgPath) ?? new PlayerData();
            Helper.WriteJsonFile(_cfgPath, _pData);
            FarmHouse h = (FarmHouse)Game1.currentLocation;

            //Lets try to load up the items now.
            if (_pData.PItems == null || _pData.PItems.Count < 1 || (h != null && h.upgradeLevel == 0))
                return;

            if (h == null)
                return;
            if (h.fridge.Value.items.Count > 0)
                h.fridge.Value.items.Clear();
            //Passed the count, now we move on to adding them
            foreach (int[] i in _pData.PItems)
            {
                h.fridge.Value.items.Add(new SObject(i[0], i[1], false, -1, i[2]));
            }
        }

        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {
            FarmHouse h = (FarmHouse)Game1.currentLocation;
            if (h == null || h.upgradeLevel == 0)
                return;
            //this.pData.pItems = _fridge.items;
            _pData.PItems?.Clear();
            if(_pData.PItems == null)
                _pData.PItems = new List<int[]>();
            foreach (Item i in _fridge.Items)
            {
                if (i is SObject obj)
                {
                    
                    _pData.PItems.Add(new[]{i.ParentSheetIndex, i.Stack, obj.Quality});
                }
            }
            Helper.WriteJsonFile(_cfgPath, _pData);
        }

        private void Event_KeyReleased(object send, EventArgsKeyPressed e)
        {
            if (e.KeyPressed.ToString().Equals(_config.fridgeNextPageKey) && Game1.activeClickableMenu is FridgeGrabMenu)
            {
                _fridge.MovePageToNext();
            }

            if (e.KeyPressed.ToString().Equals(_config.fridgePrevPageKey) && Game1.activeClickableMenu is FridgeGrabMenu)
            {
                _fridge.MovePageToPrevious();
            }
        }

        private void PlayerEvents_Warped(object send, EventArgsPlayerWarped e)
        {
            
        }

        private void Event_MenuChanged(object send, EventArgsClickableMenuChanged e)
        {
            //Log.Debug("M007_ExtendedFridge Event_MenuChanged HIT", new object[0]);
            /*
            if (Game1.currentLocation is FarmHouse)
            {
                this.Monitor.Log(String.Format("M007_ExtendedFridge lastGrabTileX:{0} lastGrabTileY:{1}", (int)Game1.player.lastGrabTile.X, (int)Game1.player.lastGrabTile.Y));
            }*/

            if (ClickedOnFridge())
            {

                //_isInFridgeMenu = true;
                if (e.NewMenu is ItemGrabMenu)
                {
                    if (_fridge == null || _fridge.Items.Count == 0)
                    {
                        _fridge = new FridgeChest(_config.autoSwitchPageOnGrab);
                        FarmHouse h = (FarmHouse)Game1.currentLocation;
                        _fridge.Items.AddRange(h.fridge.Value.items);
                    }
                    _fridge.ShowCurrentPage();
                    // this.Monitor.Log("M007_ExtendedFridge Fridge HOOKED");
                }

            }
            if (e.NewMenu is CraftingPage && Game1.currentLocation is FarmHouse fh)
            {
                if (_fridge != null)
                {
                    fh.fridge.Value.items.Clear();
                    fh.fridge.Value.items.AddRange(_fridge.Items);
                    _fridge.Items.Clear();
                }

            }


        }

        public void GrabItemFromChest(Item item, Farmer who)
        {
            _fridge.GrabItemFromChest(item, who);
        }

        public void GrabItemFromInventory(Item item, Farmer who)
        {
            _fridge.GrabItemFromInventory(item, who);
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


        public static List<Item> Organize()
        {
            var outItem = _fridge?.Items;
            return outItem;
        }

        public static string ShowMessage()
        {
            //return 
            string outPut = "";
            if (_fridge != null)
                outPut =
                    $"Extended Fridge {Version} Current Page: 0 | {_fridge.Items.Count} items in fridge.";
            return outPut;
        }
    }
}
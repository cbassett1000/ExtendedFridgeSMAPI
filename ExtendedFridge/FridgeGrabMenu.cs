using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.Objects;
using Object = StardewValley.Object;

//using xTile.Dimensions;


namespace ExtendedFridge
{
    internal class FridgeGrabMenu : MenuWithInventory
    {
        public const int RegionItemsToGrabMenuModifier = 53910;

        public const int RegionOrganizeButton = 106;

        public const int RegionColorPickToggle = 27346;

        public const int RegionSpecialButton = 12485;

        public const int RegionLastShippedHolder = 12598;

        public const int SourceNone = 0;

        public const int SourceChest = 1;

        public const int SourceGift = 2;

        public const int SourceFishingChest = 3;

        public const int SpecialButtonJunimotoggle = 1;

        public InventoryMenu ItemsToGrabMenu;

        private TemporaryAnimatedSprite _poof;

        public bool ReverseGrab;

        public bool ShowReceivingMenu = true;

        public bool DrawBg = true;

        public bool DestroyItemOnClick;

        public bool CanExitOnKey;

        public bool PlayRightClickSound;

        public bool AllowRightClick;

        public bool ShippingBin;

        private readonly string _message;

        //M07: Comment these when copy-pasting code
        //private ItemGrabMenu.behaviorOnItemSelect behaviorFunction;
        //public ItemGrabMenu.behaviorOnItemSelect behaviorOnItemGrab;
        //M07: And use these instead
        public BehaviorOnItemSelect BehaviorFunction;
        public BehaviorOnItemSelect BehaviorOnItemGrab;
        //END_M07

        private Item _sourceItem;

        public ClickableTextureComponent OrganizeButton;

        public ClickableTextureComponent ColorPickerToggleButton;

        public ClickableTextureComponent SpecialButton;

        public ClickableTextureComponent LastShippedHolder;

        public List<ClickableComponent> DiscreteColorPickerCc;

        public int Source;

        public int WhichSpecialButton;

        public object SpecialObject;

        private readonly bool _snappedtoBottom;

        public DiscreteColorPicker ChestColorPicker;


        #region -- Events --



        public delegate void BehaviorOnItemSelect(Item item, Farmer who);
        #endregion

        #region -- Needed for Extended Fridge --        
        private readonly ClickableTextureComponent _previousPageButton;
        private readonly ClickableTextureComponent _nextPageButton;

        private readonly bool _showPrevButton;
        private readonly bool _showNextButton;

        public BehaviorOnPageCtlClick BehaviorOnClickNextButton;
        public BehaviorOnPageCtlClick BehaviorOnClickPreviousButton;
        public BehaviorOnOrganizeItems BehaviorOnClickOrganize;


        public delegate void BehaviorOnPageCtlClick();
        public delegate void BehaviorOnOrganizeItems();
        #endregion

        //WAS:
        //public FridgeGrabMenu(List<Item> inventory)
        //    : base((InventoryMenu.highlightThisItem)null, true, true, 0, 0)
        //{
        //    this.ItemsToGrabMenu = new InventoryMenu(this.xPositionOnScreen + Game1.tileSize / 2, this.yPositionOnScreen, false, inventory, (InventoryMenu.highlightThisItem)null, -1, 3, 0, 0, true);
        //    this.inventory.showGrayedOutSlots = true;
        //}

        //IS:
        //DONE
        public FridgeGrabMenu(List<Item> inventory) : base(null, true, true)
        {
            ItemsToGrabMenu = new InventoryMenu(xPositionOnScreen + Game1.tileSize / 2, yPositionOnScreen, false, inventory);
            trashCan.myID = 106;
            ItemsToGrabMenu.populateClickableComponentList();
            foreach (ClickableComponent t in ItemsToGrabMenu.inventory)
            {
                if (t == null) continue;
                ClickableComponent item = t;
                item.myID = item.myID + 53910;
                ClickableComponent clickableComponent = t;
                clickableComponent.upNeighborID = clickableComponent.upNeighborID + 53910;
                ClickableComponent item1 = t;
                item1.rightNeighborID = item1.rightNeighborID + 53910;
                t.downNeighborID = -7777;
                ClickableComponent clickableComponent1 = t;
                clickableComponent1.leftNeighborID = clickableComponent1.leftNeighborID + 53910;
                t.fullyImmutable = true;
            }
            if (Game1.options.SnappyMenus)
            {
                for (int j = 0; j < 12; j++)
                {
                    if (this.inventory != null && this.inventory.inventory != null && this.inventory.inventory.Count >= 12)
                    {
                        this.inventory.inventory[j].upNeighborID = (ShippingBin ? 12598 : -7777);
                    }
                }
                if (!ShippingBin)
                {
                    for (int k = 0; k < 36; k++)
                    {
                        if (this.inventory != null && this.inventory.inventory != null && this.inventory.inventory.Count > k)
                        {
                            this.inventory.inventory[k].upNeighborID = -7777;
                            this.inventory.inventory[k].upNeighborImmutable = true;
                        }
                    }
                }
                if (trashCan != null)
                {
                    trashCan.leftNeighborID = 11;
                }
                if (okButton != null)
                {
                    okButton.leftNeighborID = 11;
                }
                populateClickableComponentList();
                snapToDefaultClickableComponent();
            }
            this.inventory.showGrayedOutSlots = true;
        }

        //WAS:
        //public FridgeGrabMenu(List<Item> inventory, bool reverseGrab, bool showReceivingMenu, InventoryMenu.highlightThisItem highlightFunction, FridgeGrabMenu.behaviorOnItemSelect behaviorOnItemSelectFunction, string message, FridgeGrabMenu.behaviorOnItemSelect behaviorOnItemGrab = null, bool snapToBottom = false, bool canBeExitedWithKey = false, bool playRightClickSound = true, bool allowRightClick = true, bool showOrganizeButton = false, int source = 0, FridgeGrabMenu.behaviorOnPageCtlClick behaviorOnClickNextButton = null, FridgeGrabMenu.behaviorOnPageCtlClick behaviorOnClickPreviousButton = null, bool bShowPrevButton = false, bool bShowNextButton = false, FridgeGrabMenu.behaviorOnOrganizeItems behaviorOnClickOrganize = null)
        //    : base(highlightFunction, true, true, 0, 0)
        //{
        //    this.source = source;
        //    this.message = message;
        //    this.reverseGrab = reverseGrab;
        //    this.showReceivingMenu = showReceivingMenu;
        //    this.playRightClickSound = playRightClickSound;
        //    this.allowRightClick = allowRightClick;
        //    this.inventory.showGrayedOutSlots = true;

        //    this.behaviorOnClickPreviousButton = behaviorOnClickPreviousButton;
        //    this.behaviorOnClickNextButton = behaviorOnClickNextButton;
        //    this.behaviorOnClickOrganize = behaviorOnClickOrganize;

        //    if (snapToBottom)
        //    {
        //        this.movePosition(0, Game1.viewport.Height - (this.yPositionOnScreen + this.height - IClickableMenu.spaceToClearTopBorder));
        //        this.snappedtoBottom = true;
        //    }

        //    this.ItemsToGrabMenu = new InventoryMenu(this.xPositionOnScreen + Game1.tileSize / 2, this.yPositionOnScreen, false, inventory, highlightFunction);
        //    this.behaviorFunction = behaviorOnItemSelectFunction;
        //    this.behaviorOnItemGrab = behaviorOnItemGrab;
        //    this.canExitOnKey = canBeExitedWithKey;

        //    if (showOrganizeButton)
        //    {
        //        this.organizeButton = new ClickableTextureComponent("", new Rectangle(this.xPositionOnScreen + this.width, this.yPositionOnScreen + this.height / 3 - Game1.tileSize, Game1.tileSize, Game1.tileSize), "", "Organize", Game1.mouseCursors, new Rectangle(162, 440, 16, 16), (float)Game1.pixelZoom);
        //    }

        //    this.previousPageButton = new ClickableTextureComponent("", new Rectangle(this.xPositionOnScreen + this.width + Game1.tileSize, (this.yPositionOnScreen + this.height / 3 - Game1.tileSize) + 2, Game1.tileSize, Game1.tileSize), "", "Previous Page", Game1.mouseCursors, new Rectangle(352, 494, 12, 12), (float)Game1.pixelZoom);
        //    this.nextPageButton = new ClickableTextureComponent("", new Rectangle(this.xPositionOnScreen + this.width + 2 * Game1.tileSize, (this.yPositionOnScreen + this.height / 3 - Game1.tileSize) + 2, Game1.tileSize, Game1.tileSize), "", "Next Page", Game1.mouseCursors, new Rectangle(365, 494, 12, 12), (float)Game1.pixelZoom);

        //    this.showPrevButton = bShowPrevButton;
        //    this.showNextButton = bShowNextButton;

        //    if (!Game1.isAnyGamePadButtonBeingPressed() && Game1.lastCursorMotionWasMouse || (this.ItemsToGrabMenu.actualInventory.Count <= 0 || Game1.activeClickableMenu != null))
        //    {
        //        return;
        //    }

        //    Game1.setMousePosition(this.inventory.inventory[0].bounds.Center);
        //}

        //M07
        //1. ItemGrabMenu.behaviorOnItemSelect to FridgeGrabMenu.behaviorOnItemSelect 
        //2. ItemGrabMenu.behaviorOnItemSelectFunction to FridgeGrabMenu.behaviorOnItemSelectFunction behaviorOnItemGrab
        //3. Add FridgeGrabMenu.behaviorOnPageCtlClick behaviorOnClickNextButton = null, FridgeGrabMenu.behaviorOnPageCtlClick behaviorOnClickPreviousButton = null, bool bShowPrevButton = false, bool bShowNextButton = false, FridgeGrabMenu.behaviorOnOrganizeItems behaviorOnClickOrganize = null at the end
        //DONE
        public FridgeGrabMenu(IList<Item> inventory, bool reverseGrab, bool showReceivingMenu, InventoryMenu.highlightThisItem highlightFunction, BehaviorOnItemSelect behaviorOnItemSelectFunction, string message, BehaviorOnItemSelect behaviorOnItemGrab = null, bool snapToBottom = false, bool canBeExitedWithKey = false, bool playRightClickSound = true, bool allowRightClick = true, bool showOrganizeButton = false, int source = 0, Item sourceItem = null, int whichSpecialButton = -1, object specialObject = null, BehaviorOnPageCtlClick behaviorOnClickNextButton = null, BehaviorOnPageCtlClick behaviorOnClickPreviousButton = null, bool bShowPrevButton = false, bool bShowNextButton = false, BehaviorOnOrganizeItems behaviorOnClickOrganize = null) : base(highlightFunction, true, true)
		{
		    Source = source;
            _message = message;
            ReverseGrab = reverseGrab;
            ShowReceivingMenu = showReceivingMenu;
            PlayRightClickSound = playRightClickSound;
            AllowRightClick = allowRightClick;
            this.inventory.showGrayedOutSlots = true;
            _sourceItem = sourceItem;

            //M07
            BehaviorOnClickPreviousButton = behaviorOnClickPreviousButton;
            BehaviorOnClickNextButton = behaviorOnClickNextButton;
            BehaviorOnClickOrganize = behaviorOnClickOrganize;
            //END_M07

            if (source == 1 && sourceItem is Chest)
            {
                ChestColorPicker = new DiscreteColorPicker(xPositionOnScreen, yPositionOnScreen - Game1.tileSize - borderWidth * 2, 0, new Chest(true))
                {
                    colorSelection = ChestColorPicker.getSelectionFromColor((sourceItem as Chest).playerChoiceColor.Value)
                };
                (ChestColorPicker.itemToDrawColored as Chest).playerChoiceColor.Value = ChestColorPicker.getColorFromSelection(ChestColorPicker.colorSelection);
                ClickableTextureComponent clickableTextureComponent = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width, yPositionOnScreen + Game1.tileSize + Game1.pixelZoom * 5, 16 * Game1.pixelZoom, 16 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(119, 469, 16, 16), Game1.pixelZoom)
                {
                    hoverText = Game1.content.LoadString("Strings\\UI:Toggle_ColorPicker", new object[0]),
                    myID = 27346,
                    downNeighborID = (showOrganizeButton ? 106 : 5948),
                    leftNeighborID = 11
                };
                ColorPickerToggleButton = clickableTextureComponent;
            }
            WhichSpecialButton = whichSpecialButton;
            SpecialObject = specialObject;
            if (whichSpecialButton == 1)
            {
                SpecialButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width, yPositionOnScreen + Game1.tileSize + Game1.pixelZoom * 5, 16 * Game1.pixelZoom, 16 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(108, 491, 16, 16), Game1.pixelZoom)
                {
                    myID = 12485,
                    downNeighborID = (showOrganizeButton ? 106 : 5948)
                };
                if (specialObject is JunimoHut)
                {
                    SpecialButton.sourceRect.X = ((specialObject as JunimoHut).noHarvest.Value ? 124 : 108);
                }
            }
            if (snapToBottom)
            {
                movePosition(0, Game1.viewport.Height - (yPositionOnScreen + height - spaceToClearTopBorder));
                _snappedtoBottom = true;
            }
            //new InventoryMenu(this.xPositionOnScreen + Game1.tileSize / 2, this.yPositionOnScreen, false, inventory, highlightFunction)
            ItemsToGrabMenu = new InventoryMenu(xPositionOnScreen + Game1.tileSize / 2, yPositionOnScreen, false, inventory, highlightFunction);
            if (Game1.options.SnappyMenus)
            {
                ItemsToGrabMenu.populateClickableComponentList();
                foreach (ClickableComponent t in ItemsToGrabMenu.inventory)
                {
                    if (t == null) continue;
                    ClickableComponent item = t;
                    item.myID = item.myID + 53910;
                    ClickableComponent clickableComponent = t;
                    clickableComponent.upNeighborID = clickableComponent.upNeighborID + 53910;
                    ClickableComponent item1 = t;
                    item1.rightNeighborID = item1.rightNeighborID + 53910;
                    t.downNeighborID = -7777;
                    ClickableComponent clickableComponent1 = t;
                    clickableComponent1.leftNeighborID = clickableComponent1.leftNeighborID + 53910;
                    t.fullyImmutable = true;
                }
            }
            BehaviorFunction = behaviorOnItemSelectFunction;
            BehaviorOnItemGrab = behaviorOnItemGrab;
            CanExitOnKey = canBeExitedWithKey;

            //M07
            _previousPageButton = new ClickableTextureComponent("", new Rectangle(xPositionOnScreen + width + Game1.tileSize, (yPositionOnScreen + height / 3 - Game1.tileSize) + 2, Game1.tileSize, Game1.tileSize), "", "Previous Page", Game1.mouseCursors, new Rectangle(352, 494, 12, 12), Game1.pixelZoom);
            _nextPageButton = new ClickableTextureComponent("", new Rectangle(xPositionOnScreen + width + 2 * Game1.tileSize, (yPositionOnScreen + height / 3 - Game1.tileSize) + 2, Game1.tileSize, Game1.tileSize), "", "Next Page", Game1.mouseCursors, new Rectangle(365, 494, 12, 12), Game1.pixelZoom);

            _showPrevButton = bShowPrevButton;
            _showNextButton = bShowNextButton;
            //END_M07

            if (showOrganizeButton)
            {
                ClickableTextureComponent clickableTextureComponent1 = new ClickableTextureComponent("", new Rectangle(xPositionOnScreen + width, yPositionOnScreen + height / 3 - Game1.tileSize, Game1.tileSize, Game1.tileSize), "", Game1.content.LoadString("Strings\\UI:ItemGrab_Organize", new object[0]), Game1.mouseCursors, new Rectangle(162, 440, 16, 16), Game1.pixelZoom)
                {
                    myID = 106
                };
                int num1;
                if (ColorPickerToggleButton != null)
                {
                    num1 = 27346;
                }
                else
                {
                    num1 = (SpecialButton != null ? 12485 : -500);
                }
                clickableTextureComponent1.upNeighborID = num1;
                clickableTextureComponent1.downNeighborID = 5948;
                OrganizeButton = clickableTextureComponent1;
            }
            if ((Game1.isAnyGamePadButtonBeingPressed() || !Game1.lastCursorMotionWasMouse) && ItemsToGrabMenu.actualInventory.Count > 0 && Game1.activeClickableMenu == null)
            {
                Game1.setMousePosition(this.inventory.inventory[0].bounds.Center);
            }
            if (Game1.options.snappyMenus && Game1.options.gamepadControls)
            {
                if (ChestColorPicker != null)
                {
                    DiscreteColorPickerCc = new List<ClickableComponent>();
                    for (int j = 0; j < ChestColorPicker.totalColors; j++)
                    {
                        DiscreteColorPickerCc.Add(new ClickableComponent(new Rectangle(ChestColorPicker.xPositionOnScreen + borderWidth / 2 + j * 9 * Game1.pixelZoom, ChestColorPicker.yPositionOnScreen + borderWidth / 2, 9 * Game1.pixelZoom, 7 * Game1.pixelZoom), "")
                        {
                            myID = j + 4343,
                            rightNeighborID = (j < ChestColorPicker.totalColors - 1 ? j + 4343 + 1 : -1),
                            leftNeighborID = (j > 0 ? j + 4343 - 1 : -1),
                            downNeighborID = (ItemsToGrabMenu == null || ItemsToGrabMenu.inventory.Count <= 0 ? 0 : 53910)
                        });
                    }
                }
                for (int k = 0; k < 12; k++)
                {
                    if (this.inventory != null && this.inventory.inventory != null && this.inventory.inventory.Count >= 12)
                    {
                        ClickableComponent item2 = this.inventory.inventory[k];
                        int num;
                        if (ShippingBin)
                        {
                            num = 12598;
                        }
                        else if (DiscreteColorPickerCc == null || ItemsToGrabMenu == null || ItemsToGrabMenu.inventory.Count > k)
                        {
                            num = (ItemsToGrabMenu.inventory.Count > k ? 53910 + k : 53910);
                        }
                        else
                        {
                            num = 4343;
                        }
                        item2.upNeighborID = num;
                    }
                    if (DiscreteColorPickerCc != null && ItemsToGrabMenu != null && ItemsToGrabMenu.inventory.Count > k)
                    {
                        ItemsToGrabMenu.inventory[k].upNeighborID = 4343;
                    }
                }
                if (!ShippingBin)
                {
                    for (int l = 0; l < 36; l++)
                    {
                        if (this.inventory != null && this.inventory.inventory != null && this.inventory.inventory.Count > l)
                        {
                            this.inventory.inventory[l].upNeighborID = -7777;
                            this.inventory.inventory[l].upNeighborImmutable = true;
                        }
                    }
                }
                if (trashCan != null && this.inventory.inventory.Count >= 12 && this.inventory.inventory[11] != null)
                {
                    this.inventory.inventory[11].rightNeighborID = 5948;
                }
                if (trashCan != null)
                {
                    trashCan.leftNeighborID = 11;
                }
                if (okButton != null)
                {
                    okButton.leftNeighborID = 11;
                }
                populateClickableComponentList();
                snapToDefaultClickableComponent();
            }
        }

        //NOMOD
        public void InitializeShippingBin()
        {
            ShippingBin = true;
            LastShippedHolder = new ClickableTextureComponent("", new Rectangle(xPositionOnScreen + width / 2 - 12 * Game1.pixelZoom, yPositionOnScreen + height / 2 - 20 * Game1.pixelZoom - Game1.tileSize, 24 * Game1.pixelZoom, 24 * Game1.pixelZoom), "", Game1.content.LoadString("Strings\\UI:ShippingBin_LastItem", new object[0]), Game1.mouseCursors, new Rectangle(293, 360, 24, 24), Game1.pixelZoom)
            {
                myID = 12598,
                region = 12598
            };
            if (Game1.options.snappyMenus)
            {
                for (int i = 0; i < 12; i++)
                {
                    if (inventory != null && inventory.inventory != null && inventory.inventory.Count >= 12)
                    {
                        inventory.inventory[i].upNeighborID = -7777;
                        if (i == 11)
                        {
                            inventory.inventory[i].rightNeighborID = 5948;
                        }
                    }
                }
                populateClickableComponentList();
                snapToDefaultClickableComponent();
            }
        }

        //NOMOD
        public void SetBackgroundTransparency(bool b)
        {
            DrawBg = b;
        }

        //NOMOD
        public void SetDestroyItemOnClick(bool b)
        {
            DestroyItemOnClick = b;
        }
        #region "Old Code"
        //WAS:
        //public override void receiveRightClick(int x, int y, bool playSound = true)
        //{
        //    if (!this.allowRightClick)
        //        return;
        //    base.receiveRightClick(x, y, playSound && this.playRightClickSound);
        //    if (this.heldItem == null && this.showReceivingMenu)
        //    {
        //        this.heldItem = this.ItemsToGrabMenu.rightClick(x, y, this.heldItem, false);
        //        if (this.heldItem != null && this.behaviorOnItemGrab != null)
        //            this.behaviorOnItemGrab(this.heldItem, Game1.player);
        //        if (this.heldItem is StardewValley.Object && (this.heldItem as StardewValley.Object).parentSheetIndex == 326)
        //        {
        //            this.heldItem = (Item)null;
        //            Game1.player.canUnderstandDwarves = true;
        //            this.poof = new TemporaryAnimatedSprite(Game1.animations, new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2((float)(x - x % Game1.tileSize + Game1.tileSize / 4), (float)(y - y % Game1.tileSize + Game1.tileSize / 4)), false, false);
        //            Game1.playSound("fireball");
        //        }
        //        else if (this.heldItem is StardewValley.Object && (this.heldItem as StardewValley.Object).isRecipe)
        //        {
        //            string key = this.heldItem.Name.Substring(0, this.heldItem.Name.IndexOf("Recipe") - 1);
        //            try
        //            {
        //                if ((this.heldItem as StardewValley.Object).category == -7)
        //                    Game1.player.cookingRecipes.Add(key, 0);
        //                else
        //                    Game1.player.craftingRecipes.Add(key, 0);
        //                this.poof = new TemporaryAnimatedSprite(Game1.animations, new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2((float)(x - x % Game1.tileSize + Game1.tileSize / 4), (float)(y - y % Game1.tileSize + Game1.tileSize / 4)), false, false);
        //                Game1.playSound("newRecipe");
        //            }
        //            catch (Exception ex)
        //            {
        //                StardewModdingAPI.Log.Debug(String.Format("FridgeGrabMenu Exception in receiveRightClick {0}", ex));
        //            }
        //            this.heldItem = (Item)null;
        //        }
        //        else
        //        {
        //            if (!Game1.player.addItemToInventoryBool(this.heldItem, false))
        //                return;
        //            this.heldItem = (Item)null;
        //            Game1.playSound("coin");
        //        }
        //    }
        //    else
        //    {
        //        if (!this.reverseGrab && this.behaviorFunction == null)
        //            return;
        //        this.behaviorFunction(this.heldItem, Game1.player);
        //        if (!this.destroyItemOnClick)
        //            return;
        //        this.heldItem = (Item)null;
        //    }
        //}
        #endregion
        //IS
        //DONE
        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (!AllowRightClick)
            {
                return;
            }
            base.receiveRightClick(x, y, (playSound && PlayRightClickSound));
            if (heldItem == null && ShowReceivingMenu)
            {
                heldItem = ItemsToGrabMenu.rightClick(x, y, heldItem, false);
                if (heldItem != null && BehaviorOnItemGrab != null && Game1.activeClickableMenu is ItemGrabMenu itm)
                {
                    BehaviorOnItemGrab(heldItem, Game1.player);
                    if (Game1.activeClickableMenu != null)
                    {
                        itm.setSourceItem(_sourceItem);
                    }
                    if (Game1.options.SnappyMenus)
                    {
                        itm.currentlySnappedComponent = currentlySnappedComponent;
                        itm.snapCursorToCurrentSnappedComponent();
                    }
                }
                if (heldItem is Object && (heldItem as Object).ParentSheetIndex == 326)
                {
                    heldItem = null;
                    Game1.player.canUnderstandDwarves = true;
                    //this.poof = new TemporaryAnimatedSprite(Game1.animations, new Microsoft.Xna.Framework.Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2((float)(x - x % Game1.tileSize + Game1.tileSize / 4), (float)(y - y % Game1.tileSize + Game1.tileSize / 4)), false, false);
                    Game1.playSound("fireball");
                    return;
                }
                if (heldItem is Object && (heldItem as Object).IsRecipe)
                {
                    string str = heldItem.Name.Substring(0, heldItem.Name.IndexOf("Recipe", StringComparison.Ordinal) - 1);
                    try
                    {
                        if ((heldItem as Object).Category != -7)
                        {
                            Game1.player.craftingRecipes.Add(str, 0);
                        }
                        else
                        {
                            Game1.player.cookingRecipes.Add(str, 0);
                        }
                        //this.poof = new TemporaryAnimatedSprite(Game1.animations, new Microsoft.Xna.Framework.Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2((float)(x - x % Game1.tileSize + Game1.tileSize / 4), (float)(y - y % Game1.tileSize + Game1.tileSize / 4)), false, false);
                        Game1.playSound("newRecipe");
                    }
                    catch (Exception exception)
                    {
                        ExtendedFridgeMod.Instance.Monitor.Log($"Error: {exception}", LogLevel.Trace);
                    }
                    heldItem = null;
                    return;
                }
                if (Game1.player.addItemToInventoryBool(heldItem))
                {
                    heldItem = null;
                    Game1.playSound("coin");
                }
            }
            else if (ReverseGrab || BehaviorFunction != null)
            {
                BehaviorFunction(heldItem, Game1.player);
                if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is ItemGrabMenu)
                {
                    ((ItemGrabMenu) Game1.activeClickableMenu).setSourceItem(_sourceItem);
                }
                if (DestroyItemOnClick)
                {
                    heldItem = null;
                }
            }
        }

        //DONE
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            if (_snappedtoBottom)
            {
                movePosition((newBounds.Width - oldBounds.Width) / 2, Game1.viewport.Height - (yPositionOnScreen + height - spaceToClearTopBorder));
            }
            ItemsToGrabMenu?.gameWindowSizeChanged(oldBounds, newBounds);
            if (OrganizeButton != null)
            {
                OrganizeButton = new ClickableTextureComponent("", new Rectangle(xPositionOnScreen + width, yPositionOnScreen + height / 3 - Game1.tileSize, Game1.tileSize, Game1.tileSize), "", Game1.content.LoadString("Strings\\UI:ItemGrab_Organize", new object[0]), Game1.mouseCursors, new Rectangle(162, 440, 16, 16), Game1.pixelZoom);
            }
            if (Source == 1 && _sourceItem is Chest)
            {
                ChestColorPicker = new DiscreteColorPicker(xPositionOnScreen, yPositionOnScreen - Game1.tileSize - borderWidth * 2)
                {
                    colorSelection = ChestColorPicker.getSelectionFromColor(((Chest) _sourceItem).playerChoiceColor.Value)
                };
            }
        }

        //DONE
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, (!DestroyItemOnClick));
            if (ShippingBin && LastShippedHolder.containsPoint(x, y))
            {
                if (Game1.getFarm().lastItemShipped != null && Game1.player.addItemToInventoryBool(Game1.getFarm().lastItemShipped))
                {
                    Game1.playSound("coin");
                    Game1.getFarm().shippingBin.Remove(Game1.getFarm().lastItemShipped);
                    Game1.getFarm().lastItemShipped = null;
                    if (Game1.player.ActiveObject != null)
                    {
                        Game1.player.showCarrying();
                        Game1.player.Halt();
                    }
                }
                return;
            }
            if (ChestColorPicker != null)
            {
                ChestColorPicker.receiveLeftClick(x, y);
                if (_sourceItem is Chest item)
                {
                    item.playerChoiceColor.Value = ChestColorPicker.getColorFromSelection(ChestColorPicker.colorSelection);
                }
            }
            if (ColorPickerToggleButton != null && ColorPickerToggleButton.containsPoint(x, y))
            {
                Game1.player.showChestColorPicker = !Game1.player.showChestColorPicker;
                if (ChestColorPicker != null) ChestColorPicker.visible = Game1.player.showChestColorPicker;
                try
                {
                    Game1.playSound("drumkit6");
                }
                catch (Exception exception)
                {
                    ExtendedFridgeMod.Instance.Monitor.Log($"Error: {exception}", LogLevel.Trace);
                }
            }
            if (WhichSpecialButton != -1 && SpecialButton != null && SpecialButton.containsPoint(x, y))
            {
                Game1.playSound("drumkit6");
                if (WhichSpecialButton == 1 && SpecialObject is JunimoHut)
                {
                    (SpecialObject as JunimoHut).noHarvest.Value = !((JunimoHut) SpecialObject).noHarvest.Value;
                    SpecialButton.sourceRect.X = (((JunimoHut) SpecialObject).noHarvest.Value ? 124 : 108);
                }
            }
            if (heldItem == null && ShowReceivingMenu)
            {
                heldItem = ItemsToGrabMenu.leftClick(x, y, heldItem, false);
                if (heldItem != null && BehaviorOnItemGrab != null)
                {
                    BehaviorOnItemGrab(heldItem, Game1.player);
                    if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is ItemGrabMenu)
                    {
                        ((ItemGrabMenu) Game1.activeClickableMenu).setSourceItem(_sourceItem);
                        if (Game1.options.SnappyMenus)
                        {
                            ((ItemGrabMenu) Game1.activeClickableMenu).currentlySnappedComponent = currentlySnappedComponent;
                            ((ItemGrabMenu) Game1.activeClickableMenu).snapCursorToCurrentSnappedComponent();
                        }
                    }
                }
                if (heldItem is Object && (heldItem as Object).ParentSheetIndex == 326)
                {
                    heldItem = null;
                    Game1.player.canUnderstandDwarves = true;
                    //this.poof = new TemporaryAnimatedSprite(Game1.animations, new Microsoft.Xna.Framework.Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2((float)(x - x % Game1.tileSize + Game1.tileSize / 4), (float)(y - y % Game1.tileSize + Game1.tileSize / 4)), false, false);
                    Game1.playSound("fireball");
                }
                else if (heldItem is Object && (heldItem as Object).ParentSheetIndex == 102)
                {
                    heldItem = null;
                    Game1.player.foundArtifact(102, 1);
                    //this.poof = new TemporaryAnimatedSprite(Game1.animations, new Microsoft.Xna.Framework.Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2((float)(x - x % Game1.tileSize + Game1.tileSize / 4), (float)(y - y % Game1.tileSize + Game1.tileSize / 4)), false, false);
                    Game1.playSound("fireball");
                }
                else if (heldItem is Object && (heldItem as Object).IsRecipe)
                {
                    string str = heldItem.Name.Substring(0, heldItem.Name.IndexOf("Recipe", StringComparison.Ordinal) - 1);
                    try
                    {
                        if ((heldItem as Object).Category != -7)
                        {
                            Game1.player.craftingRecipes.Add(str, 0);
                        }
                        else
                        {
                            Game1.player.cookingRecipes.Add(str, 0);
                        }
                        //this.poof = new TemporaryAnimatedSprite(Game1.animations, new Microsoft.Xna.Framework.Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2((float)(x - x % Game1.tileSize + Game1.tileSize / 4), (float)(y - y % Game1.tileSize + Game1.tileSize / 4)), false, false);
                        Game1.playSound("newRecipe");
                    }
                    catch (Exception exception1)
                    {
                        ExtendedFridgeMod.Instance.Monitor.Log($"Error: {exception1}", LogLevel.Trace);
                    }
                    heldItem = null;
                }
                else if (Game1.player.addItemToInventoryBool(heldItem))
                {
                    heldItem = null;
                    Game1.playSound("coin");
                }
            }
            else if ((ReverseGrab || BehaviorFunction != null) && isWithinBounds(x, y))
            {
                BehaviorFunction(heldItem, Game1.player);
                if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is ItemGrabMenu)
                {
                    ((ItemGrabMenu) Game1.activeClickableMenu).setSourceItem(_sourceItem);
                    if (Game1.options.SnappyMenus)
                    {
                        ((ItemGrabMenu) Game1.activeClickableMenu).currentlySnappedComponent = currentlySnappedComponent;
                        ((ItemGrabMenu) Game1.activeClickableMenu).snapCursorToCurrentSnappedComponent();
                    }
                }
                if (DestroyItemOnClick)
                {
                    heldItem = null;
                    return;
                }
            }
            if (OrganizeButton != null && OrganizeButton.containsPoint(x, y))
            {
                ItemGrabMenu.organizeItemsInList(ExtendedFridgeMod.Organize());
                //ItemGrabMenu.organizeItemsInList(this.ItemsToGrabMenu.actualInventory);
                Game1.activeClickableMenu = new FridgeGrabMenu(ItemsToGrabMenu.actualInventory, false, true, InventoryMenu.highlightAllItems, BehaviorFunction, null, BehaviorOnItemGrab, false, true, true, true, true, Source, _sourceItem);
                Game1.playSound("Ship");
                return;
            }

            if (_previousPageButton != null && _previousPageButton.containsPoint(x, y) && BehaviorOnClickPreviousButton != null)
            {
                BehaviorOnClickPreviousButton();
                Game1.playSound("Ship");
                return;
            }

            if (_nextPageButton != null && _nextPageButton.containsPoint(x, y) && BehaviorOnClickNextButton != null)
            {
                BehaviorOnClickNextButton();
                Game1.playSound("Ship");
                return;
            }

            if (heldItem != null && !isWithinBounds(x, y) && heldItem.canBeTrashed())
            {
                Game1.playSound("throwDownITem");
                Game1.createItemDebris(heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
                inventory.onAddItem?.Invoke(heldItem, Game1.player);
                heldItem = null;
            }
        }

        //NOMOD
        public static void OrganizeItemsInList(IList<Item> items)
        {
            items.ToList().Sort();
            items.ToList().Reverse();
        }

        //NOMOD
        public bool AreAllItemsTaken()
        {
            return ItemsToGrabMenu.actualInventory.All(t => t == null);
        }

        //DONE
        public override void receiveKeyPress(Keys key)
        {
            if (Game1.options.snappyMenus && Game1.options.gamepadControls)
            {
                applyMovementKey(key);
            }
            if ((CanExitOnKey || AreAllItemsTaken()) && Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose())
            {
                exitThisMenu();
                if (Game1.currentLocation.currentEvent != null)
                {
                    Event currentCommand = Game1.currentLocation.currentEvent;
                    currentCommand.CurrentCommand = currentCommand.CurrentCommand + 1;
                }
            }
            else if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && heldItem != null)
            {
                Game1.setMousePosition(trashCan.bounds.Center);
            }
            if (key == Keys.Delete && heldItem != null && heldItem.canBeTrashed())
            {
                if (heldItem is Object && Game1.player.specialItems.Contains(((Object) heldItem).ParentSheetIndex))
                {
                    Game1.player.specialItems.Remove(((Object) heldItem).ParentSheetIndex);
                }
                heldItem = null;
                Game1.playSound("trashcan");
            }
        }

        //NOMOD
        public override void update(GameTime time)
        {
            base.update(time);
            if (_poof != null && _poof.update(time))
            {
                _poof = null;
            }
            ChestColorPicker?.update(time);
        }

        //DONE
        public override void performHoverAction(int x, int y)
        {
            if (ColorPickerToggleButton != null)
            {
                ColorPickerToggleButton.tryHover(x, y, 0.25f);
                if (ColorPickerToggleButton.containsPoint(x, y))
                {
                    hoverText = ColorPickerToggleButton.hoverText;
                    return;
                }
            }
            SpecialButton?.tryHover(x, y, 0.25f);
            if (!ItemsToGrabMenu.isWithinBounds(x, y) || !ShowReceivingMenu)
            {
                base.performHoverAction(x, y);
            }
            else
            {
                hoveredItem = ItemsToGrabMenu.hover(x, y, heldItem);
            }
            if (OrganizeButton != null)
            {
                hoverText = null;
                OrganizeButton.tryHover(x, y);
                if (OrganizeButton.containsPoint(x, y))
                {
                    hoverText = OrganizeButton.hoverText;
                }
            }

            //M07: Needed to make previous/next page button usable
            if (_previousPageButton.containsPoint(x, y) && _showPrevButton)
            {
                hoverText = _previousPageButton.hoverText;
            }

            if (_nextPageButton.containsPoint(x, y) && _showNextButton)
            {
                hoverText = _nextPageButton.hoverText;
            }
            //END_M07

            if (ShippingBin)
            {
                hoverText = null;
                if (LastShippedHolder.containsPoint(x, y) && Game1.getFarm().lastItemShipped != null)
                {
                    hoverText = LastShippedHolder.hoverText;
                }
            }
            ChestColorPicker?.performHoverAction(x, y);
        }

        //DONE
        protected override void customSnapBehavior(int direction, int oldRegion, int oldId)
        {
            if (direction != 2)
            {
                if (direction == 0)
                {
                    if (ShippingBin && Game1.getFarm().lastItemShipped != null && oldId < 12)
                    {
                        currentlySnappedComponent = getComponentWithID(12598);
                        currentlySnappedComponent.downNeighborID = oldId;
                        snapCursorToCurrentSnappedComponent();
                        return;
                    }
                    if (oldId < 53910 && oldId >= 12)
                    {
                        currentlySnappedComponent = getComponentWithID(oldId - 12);
                        return;
                    }
                    int num = oldId + 24;
                    for (int i = 0; i < 3 && ItemsToGrabMenu.inventory.Count <= num; i++)
                    {
                        num = num - 12;
                    }
                    if (num >= 0)
                    {
                        currentlySnappedComponent = getComponentWithID(num + 53910);
                    }
                    else if (ItemsToGrabMenu.inventory.Count > 0)
                    {
                        currentlySnappedComponent = getComponentWithID(53910 + ItemsToGrabMenu.inventory.Count - 1);
                    }
                    else if (DiscreteColorPickerCc != null)
                    {
                        currentlySnappedComponent = getComponentWithID(4343);
                    }
                    snapCursorToCurrentSnappedComponent();
                }
                return;
            }
            for (int j = 0; j < 12; j++)
            {
                if (inventory != null && inventory.inventory != null && inventory.inventory.Count >= 12 && ShippingBin)
                {
                    inventory.inventory[j].upNeighborID = (ShippingBin ? 12598 : Math.Min(j, ItemsToGrabMenu.inventory.Count - 1) + 53910);
                }
            }
            if (!ShippingBin && oldId >= 53910)
            {
                int num1 = oldId - 53910;
                if (num1 + 12 <= ItemsToGrabMenu.inventory.Count - 1)
                {
                    currentlySnappedComponent = getComponentWithID(num1 + 12 + 53910);
                    snapCursorToCurrentSnappedComponent();
                    return;
                }
            }
            currentlySnappedComponent = getComponentWithID((oldRegion == 12598 ? 0 : (oldId - 53910) % 12));
            snapCursorToCurrentSnappedComponent();
        }

        //TODOx
        public override void draw(SpriteBatch b)
        {
            if (DrawBg)
            {
                b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.Black * 0.5f);
            }

            draw(b, false, false);

            if (ShowReceivingMenu)
            {
                b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen - 16 * Game1.pixelZoom, yPositionOnScreen + height / 2 + Game1.tileSize + Game1.pixelZoom * 4), new Rectangle(16, 368, 12, 16), Color.White, 4.712389f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 1f);
                b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen - 16 * Game1.pixelZoom, yPositionOnScreen + height / 2 + Game1.tileSize - Game1.pixelZoom * 4), new Rectangle(21, 368, 11, 16), Color.White, 4.712389f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 1f);
                b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen - 10 * Game1.pixelZoom, yPositionOnScreen + height / 2 + Game1.tileSize - Game1.pixelZoom * 11), new Rectangle(4, 372, 8, 11), Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 1f);
                if (Source != 0)
                {
                    b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen - 18 * Game1.pixelZoom, yPositionOnScreen + Game1.tileSize + Game1.pixelZoom * 4), new Rectangle(16, 368, 12, 16), Color.White, 4.712389f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 1f);
                    b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen - 18 * Game1.pixelZoom, yPositionOnScreen + Game1.tileSize - Game1.pixelZoom * 4), new Rectangle(21, 368, 11, 16), Color.White, 4.712389f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 1f);
                    Rectangle rectangle = new Rectangle(sbyte.MaxValue, 412, 10, 11);
                    switch (Source)
                    {
                        case 2:
                            rectangle.X += 20;
                            break;
                        case 3:
                            rectangle.X += 10;
                            break;
                    }
                    b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen - 13 * Game1.pixelZoom, yPositionOnScreen + Game1.tileSize - Game1.pixelZoom * 11), rectangle, Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 1f);
                }
                Game1.drawDialogueBox(ItemsToGrabMenu.xPositionOnScreen - borderWidth - spaceToClearSideBorder, ItemsToGrabMenu.yPositionOnScreen - borderWidth - spaceToClearTopBorder, ItemsToGrabMenu.width + borderWidth * 2 + spaceToClearSideBorder * 2, ItemsToGrabMenu.height + spaceToClearTopBorder + borderWidth * 2, false, true, _message);
                ItemsToGrabMenu.draw(b);
            }
            else if (_message != null)
            {
                Game1.drawDialogueBox(Game1.viewport.Width / 2, ItemsToGrabMenu.yPositionOnScreen + ItemsToGrabMenu.height / 2, false, false, _message);
            }

            _poof?.draw(b, true);

            if (ShippingBin && Game1.getFarm().lastItemShipped != null)
            {
                LastShippedHolder.draw(b);
                Game1.getFarm().lastItemShipped.drawInMenu(b, new Vector2(LastShippedHolder.bounds.X + Game1.pixelZoom * 4, LastShippedHolder.bounds.Y + Game1.pixelZoom * 4), 1f);
                b.Draw(Game1.mouseCursors, new Vector2(LastShippedHolder.bounds.X + Game1.pixelZoom * -2, LastShippedHolder.bounds.Bottom - Game1.pixelZoom * 25), new Rectangle(325, 448, 5, 14), Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 1f);
                b.Draw(Game1.mouseCursors, new Vector2(LastShippedHolder.bounds.X + Game1.pixelZoom * 21, LastShippedHolder.bounds.Bottom - Game1.pixelZoom * 25), new Rectangle(325, 448, 5, 14), Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 1f);
                b.Draw(Game1.mouseCursors, new Vector2(LastShippedHolder.bounds.X + Game1.pixelZoom * -2, LastShippedHolder.bounds.Bottom - Game1.pixelZoom * 11), new Rectangle(325, 452, 5, 13), Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 1f);
                b.Draw(Game1.mouseCursors, new Vector2(LastShippedHolder.bounds.X + Game1.pixelZoom * 21, LastShippedHolder.bounds.Bottom - Game1.pixelZoom * 11), new Rectangle(325, 452, 5, 13), Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 1f);
            }

            OrganizeButton?.draw(b);

            if (_showPrevButton) { _previousPageButton.draw(b); }
            if (_showNextButton) { _nextPageButton.draw(b); }

            if (hoverText != null && (hoveredItem == null || ItemsToGrabMenu == null))
                drawHoverText(b, hoverText, Game1.smallFont);
            if (hoveredItem != null)
                drawToolTip(b, hoveredItem.getDescription(), hoveredItem.Name, hoveredItem, heldItem != null);
            else if (hoveredItem != null && ItemsToGrabMenu != null)
                drawToolTip(b, ItemsToGrabMenu.descriptionText, ItemsToGrabMenu.descriptionTitle, hoveredItem, heldItem != null);
            heldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);
            Game1.mouseCursorTransparency = 1f;

            //IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), 200, 20, 880, 76,new Color(125f,125f,125f) , 1f, true);
            drawBorderLabel(b, ExtendedFridgeMod.ShowMessage()/*message*/, Game1.smallFont, xPositionOnScreen, 0);

            drawMouse(b);
        }

        //NOMOD
        public void SetSourceItem(Item item)
        {
            _sourceItem = item;
            ChestColorPicker = null;
            ColorPickerToggleButton = null;
            if (Source == 1 && _sourceItem is Chest)
            {
                if (ChestColorPicker != null)
                {
                    ChestColorPicker = new DiscreteColorPicker(xPositionOnScreen,
                        yPositionOnScreen - Game1.tileSize - borderWidth * 2, 0, new Chest(true))
                    {
                        colorSelection =
                            ChestColorPicker.getSelectionFromColor(((Chest) _sourceItem).playerChoiceColor.Value)
                    };
                    ((Chest) ChestColorPicker.itemToDrawColored).playerChoiceColor.Value =
                        ChestColorPicker.getColorFromSelection(ChestColorPicker.colorSelection);
                }
                var clickableTextureComponent = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width, yPositionOnScreen + Game1.tileSize + Game1.pixelZoom * 5, 16 * Game1.pixelZoom, 16 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(119, 469, 16, 16), Game1.pixelZoom)
                {
                    hoverText = Game1.content.LoadString("Strings\\UI:Toggle_ColorPicker", new object[0])
                };
                ColorPickerToggleButton = clickableTextureComponent;
            }
        }

        //NOMOD
        public sealed override void snapToDefaultClickableComponent()
        {
            currentlySnappedComponent = !ShippingBin ? getComponentWithID((ItemsToGrabMenu.inventory.Count > 0 ? 53910 : 0)) : getComponentWithID(0);
            snapCursorToCurrentSnappedComponent();
        }

        //NOMOD
        public override void receiveGamePadButton(Buttons b)
        {
            base.receiveGamePadButton(b);
            if (b == Buttons.Back && OrganizeButton != null)
            {
                OrganizeItemsInList(Game1.player.Items);
                Game1.playSound("Ship");
            }
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace ExtendedFridge
{
    internal class FridgeChest
    {
        public readonly int MAX_CAPACITY = ITEMS_PER_PAGE * MAX_ITEM_PAGE;
        public const int ITEMS_PER_PAGE = 36;
        public const int MAX_ITEM_PAGE = 6;
        public int currentpage;
        public Item lastAddedItem;
        private bool _autoSwitchPageOnGrab;
        
        public List<Item> items = new List<Item>();

        public FridgeChest(bool bSwitchPage)
        {
            _autoSwitchPageOnGrab = bSwitchPage;
        }

        //public void DisplayCurrentPage()
        //{
        //    List<Item> newItems = GetCurrentPageItems();

        //    Game1.activeClickableMenu = (IClickableMenu)new ItemGrabMenu(newItems, false, true, new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems), new ItemGrabMenu.behaviorOnItemSelect(this.grabItemFromInventory), GetPageString(), new ItemGrabMenu.behaviorOnItemSelect(this.grabItemFromChest), false, true, true, true, true, 1);
        //}

        public bool PageHasItems(int pagenumber)
        {
            return ( items.Count >= (pagenumber * 36) );
        }

        public void MovePageToNext()
        {
            if ( (currentpage + 1) > (MAX_ITEM_PAGE- 1 ) || !PageHasItems(currentpage +1 ) ) { return; }

            currentpage += 1;
            ShowCurrentPage();
        }

        public void ShowCurrentPage()
        {
            List<Item> newItems = GetCurrentPageItems();

            bool bShowNextPage = false;
            bool bShowPrevPage = false;

            int nextpage = currentpage + 1;
            int prevpage = currentpage - 1;            

            bShowPrevPage = ( (prevpage >= 0) && PageHasItems(prevpage) );
            bShowNextPage = ( ( nextpage <= (MAX_ITEM_PAGE - 1) ) && PageHasItems(nextpage) );         

            //Game1.activeClickableMenu = (IClickableMenu)new FridgeGrabMenu(newItems, false, true, new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems), new FridgeGrabMenu.behaviorOnItemSelect(this.grabItemFromInventory), GetPageString(), new FridgeGrabMenu.behaviorOnItemSelect(this.grabItemFromChest), false, true, true, true, true, 1, new FridgeGrabMenu.behaviorOnPageCtlClick(this.MovePageToNext), new FridgeGrabMenu.behaviorOnPageCtlClick(this.MovePageToPrevious), bShowPrevPage, bShowNextPage, new FridgeGrabMenu.behaviorOnOrganizeItems(this.OrganizeItems));
            Game1.activeClickableMenu = new FridgeGrabMenu(newItems, false, true, InventoryMenu.highlightAllItems, grabItemFromInventory, GetPageString(), grabItemFromChest, false, true, true, true, true, 1, null, -1, null, MovePageToNext, MovePageToPrevious, bShowPrevPage, bShowNextPage, OrganizeItems);
        }

        public void MovePageToPrevious()
        {
            if (currentpage <= 0) { return; }
            if (!PageHasItems(currentpage - 1)) { return; }

            currentpage -= 1;
            ShowCurrentPage();
        }

        private int GetPageForIndex(int itemindex)
        {
            if (itemindex % 36 == 0)
            {
                return (itemindex / 36) - 1;
            }
            return itemindex / 36;
        }

        private List<Item> GetCurrentPageItems()
        {
            List<Item> curPageItems = new List<Item>();

            int low_limit = currentpage * 36;
            int high_limit = low_limit + 35;

            for (int i = low_limit; i <= high_limit; i++)
            {
                if (i < items.Count)
                { 
                    curPageItems.Add(items[i]); 
                }
                else
                { 
                    break; 
                }
            }            

            return curPageItems;
        }

        //behaviorOnItemGrab
        public void grabItemFromChest(Item item, Farmer who)
        {
            if (!who.couldInventoryAcceptThisItem(item))
                return;
            items.Remove(item);
            clearNulls();

            FarmHouse h = (FarmHouse)Game1.currentLocation;
            IList<Item> fItems = h.fridge.Value.items;
            fItems = items;

            //TODO: implement page change

            //List<Item> newItems = GetCurrentPageItems();

            ShowCurrentPage();
        }

        //behaviorOnItemSelectFunction
        public void grabItemFromInventory(Item item, Farmer who)
        {
            if (item.Stack == 0)
                item.Stack = 1;
            Item obj = addItem(item);
            if (obj == null)
                who.removeItemFromInventory(item);
            else
                who.addItemToInventory(obj);
            clearNulls();

            //TODO: implement page change
            //List<Item> newItems = GetCurrentPageItems();
            ShowCurrentPage();
        }

        public Item addItem(Item item)
        {

            lastAddedItem = null;

            for (int index = 0; index < items.Count(); ++index)
            {
                if (items[index] != null && items[index].canStackWith(item))
                {
                    item.Stack = items[index].addToStack(item.Stack);
                    if (item.Stack <= 0) { return null; }

                    if (_autoSwitchPageOnGrab) { currentpage = GetPageForIndex(index); }                    
                }
            }
            if (items.Count() >= MAX_CAPACITY) { return item; }

            items.Add(item);
            if (_autoSwitchPageOnGrab) { currentpage = GetPageForIndex(items.Count); }
            

            lastAddedItem = item;

            FarmHouse h = (FarmHouse)Game1.currentLocation;
            IList<Item> fItems = h.fridge.Value.items;
            fItems = items;

            return null;
        }

        public void clearNulls()
        {
            for (int index = items.Count() - 1; index >= 0; --index)
            {
                if (items[index] == null)
                {
                    items.RemoveAt(index);
                    //if (_autoSwitchPageOnGrab) { currentpage = GetPageForIndex(index); }
                }
            }

            FarmHouse h = (FarmHouse)Game1.currentLocation;
            IList<Item> fItems = h.fridge.Value.items;
            fItems = items;
        }

        private string GetPageString()
        {
            return String.Format("Extended Fridge {0} | Current Page: {1} | {2} items in fridge", M007_ExtendedFridge_Mod.Version, (currentpage + 1), items.Count);
        }


        //organize items behaviour
        public void OrganizeItems()
        {
            items.Sort();
            items.Reverse();

            FarmHouse h = (FarmHouse)Game1.currentLocation;
            IList<Item> fItems = h.fridge.Value.items;
            fItems = items;

            currentpage = 0;
            ShowCurrentPage();
        }
    }
}
using System.Collections.Generic;
using StardewValley;
using StardewValley.Menus;

namespace ExtendedFridge
{
    internal class FridgeChest
    {
        public readonly int MaxCapacity = ItemsPerPage * MaxItemPage;
        public const int ItemsPerPage = 36;
        public const int MaxItemPage = 6;
        public int Currentpage;
        public Item LastAddedItem;
        private readonly bool _autoSwitchPageOnGrab;
        
        public List<Item> Items = new List<Item>();

        public FridgeChest(bool bSwitchPage)
        {
            _autoSwitchPageOnGrab = bSwitchPage;
        }
        #region "Old"
        //public void DisplayCurrentPage()
        //{
        //    List<Item> newItems = GetCurrentPageItems();

        //    Game1.activeClickableMenu = (IClickableMenu)new ItemGrabMenu(newItems, false, true, new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems), new ItemGrabMenu.behaviorOnItemSelect(this.grabItemFromInventory), GetPageString(), new ItemGrabMenu.behaviorOnItemSelect(this.grabItemFromChest), false, true, true, true, true, 1);
        //}
#endregion
        public bool PageHasItems(int pagenumber)
        {
            return ( Items.Count >= (pagenumber * 36) );
        }

        public void MovePageToNext()
        {
            if ( (Currentpage + 1) > (MaxItemPage- 1 ) || !PageHasItems(Currentpage +1 ) ) { return; }

            Currentpage += 1;
            ShowCurrentPage();
        }

        public void ShowCurrentPage()
        {
            List<Item> newItems = GetCurrentPageItems();

            int nextpage = Currentpage + 1;
            int prevpage = Currentpage - 1;            

            var bShowPrevPage = ( (prevpage >= 0) && PageHasItems(prevpage) );
            var bShowNextPage = ( ( nextpage <= (MaxItemPage - 1) ) && PageHasItems(nextpage) );         

            //Game1.activeClickableMenu = (IClickableMenu)new FridgeGrabMenu(newItems, false, true, new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems), new FridgeGrabMenu.behaviorOnItemSelect(this.grabItemFromInventory), GetPageString(), new FridgeGrabMenu.behaviorOnItemSelect(this.grabItemFromChest), false, true, true, true, true, 1, new FridgeGrabMenu.behaviorOnPageCtlClick(this.MovePageToNext), new FridgeGrabMenu.behaviorOnPageCtlClick(this.MovePageToPrevious), bShowPrevPage, bShowNextPage, new FridgeGrabMenu.behaviorOnOrganizeItems(this.OrganizeItems));
            Game1.activeClickableMenu = new FridgeGrabMenu(newItems, false, true, InventoryMenu.highlightAllItems, GrabItemFromInventory, GetPageString(), GrabItemFromChest, false, true, true, true, true, 1, null, -1, null, MovePageToNext, MovePageToPrevious, bShowPrevPage, bShowNextPage, OrganizeItems);
        }

        public void MovePageToPrevious()
        {
            if (Currentpage <= 0) { return; }
            if (!PageHasItems(Currentpage - 1)) { return; }

            Currentpage -= 1;
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

            int lowLimit = Currentpage * 36;
            int highLimit = lowLimit + 35;

            for (int i = lowLimit; i <= highLimit; i++)
            {
                if (i < Items.Count)
                { 
                    curPageItems.Add(Items[i]); 
                }
                else
                { 
                    break; 
                }
            }            

            return curPageItems;
        }

        //behaviorOnItemGrab
        public void GrabItemFromChest(Item item, Farmer who)
        {
            if (!who.couldInventoryAcceptThisItem(item))
                return;
            Items.Remove(item);
            ClearNulls();

            //1TODO: implement page change

            //List<Item> newItems = GetCurrentPageItems();

            ShowCurrentPage();
        }

        //behaviorOnItemSelectFunction
        public void GrabItemFromInventory(Item item, Farmer who)
        {
            if (item.Stack == 0)
                item.Stack = 1;
            Item obj = AddItem(item);
            if (obj == null)
                who.removeItemFromInventory(item);
            else
                who.addItemToInventory(obj);
            ClearNulls();

            //1TODO: implement page change
            //List<Item> newItems = GetCurrentPageItems();
            ShowCurrentPage();
        }

        public Item AddItem(Item item)
        {

            LastAddedItem = null;

            for (int index = 0; index < Items.Count; ++index)
            {
                if (Items[index] != null && Items[index].canStackWith(item))
                {
                    item.Stack = Items[index].addToStack(item.Stack);
                    if (item.Stack <= 0) { return null; }

                    if (_autoSwitchPageOnGrab) { Currentpage = GetPageForIndex(index); }                    
                }
            }
            if (Items.Count >= MaxCapacity) { return item; }

            Items.Add(item);
            if (_autoSwitchPageOnGrab) { Currentpage = GetPageForIndex(Items.Count); }
            

            LastAddedItem = item;

            return null;
        }

        public void ClearNulls()
        {
            for (int index = Items.Count - 1; index >= 0; --index)
            {
                if (Items[index] == null)
                {
                    Items.RemoveAt(index);
                    //if (_autoSwitchPageOnGrab) { currentpage = GetPageForIndex(index); }
                }
            }
        }

        private string GetPageString()
        {
            return
                $"Extended Fridge {ExtendedFridgeMod.Version} | Current Page: {(Currentpage + 1)} | {Items.Count} items in fridge";
        }


        //organize items behaviour
        public void OrganizeItems()
        {
            Items.Sort();
            Items.Reverse();

            Currentpage = 0;
            ShowCurrentPage();
        }
    }
}
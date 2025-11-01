using System;
using Duckov.UI;
using ItemStatsSystem;

namespace tinygrox.DuckovMods.GiveMeInventoryFilter.HarmonyPatches
{
    public class ItemDecomposeViewPatches
    {
        public static void OnOpenPostfix(InventoryDisplay ___characterInventoryDisplay, InventoryDisplay ___storageDisplay)
        {
            var setFilterFunction = SetFilterFunction();
            ___characterInventoryDisplay.SetFilter(setFilterFunction);
            ___storageDisplay.SetFilter(setFilterFunction);
        }

        private static Func<Item, bool> SetFilterFunction()
        {
            return (Item e) => e && DecomposeDatabase.CanDecompose(e.TypeID);
        }
    }
}

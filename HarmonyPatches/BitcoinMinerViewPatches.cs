using System;
using Duckov.UI;
using ItemStatsSystem;

namespace tinygrox.DuckovMods.GiveMeInventoryFilter.HarmonyPatches
{
    public class BitcoinMinerViewPatches
    {
        public static void OnOpenPostfix(InventoryDisplay ___inventoryDisplay, InventoryDisplay ___storageDisplay)
        {
            var setFilterFunction = Utilities.SetFilterFunction("ComputerParts_GPU");
            ___inventoryDisplay.SetFilter(setFilterFunction);
            ___storageDisplay.SetFilter(setFilterFunction);
        }
    }
}

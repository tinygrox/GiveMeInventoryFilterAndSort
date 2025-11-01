using System;
using Duckov.UI;
using ItemStatsSystem;

namespace tinygrox.DuckovMods.GiveMeInventoryFilter.HarmonyPatches
{
    public static class MasterKeysRegisterViewPatches
    {
        public static void OnOpenPostfix(InventoryDisplay ___inventoryDisplay, InventoryDisplay ___playerStorageInventoryDisplay)
        {
            var setFilterFunction = Utilities.SetFilterFunction("Key");
            ___inventoryDisplay.SetFilter(setFilterFunction);
            ___playerStorageInventoryDisplay.SetFilter(setFilterFunction);
        }
    }
}

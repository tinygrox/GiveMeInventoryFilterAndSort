using System;
using Duckov.UI;
using ItemStatsSystem;

namespace tinygrox.DuckovMods.GiveMeInventoryFilter.HarmonyPatches
{
    public static class FormulasRegisterViewPatches
    {
        // OnOpen | EntryFunc_CanOperate
        public static void OnOpenPostfix(InventoryDisplay ___inventoryDisplay, InventoryDisplay ___playerStorageInventoryDisplay)
        {
            var setFilterFunction = Utilities.SetFilterFunction("Formula", "Formula_Blueprint");
            ___inventoryDisplay.SetFilter(setFilterFunction);
            ___playerStorageInventoryDisplay.SetFilter(setFilterFunction);
        }
    }
}

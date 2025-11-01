using System.Collections.Generic;
using Duckov.UI;
using ItemStatsSystem;

namespace tinygrox.DuckovMods.GiveMeInventoryFilter.HarmonyPatches
{
    public class ItemRepairViewPatches
    {
        public static void GetAllEquippedItemsPostfix(ItemRepairView __instance, List<Item> __result)
        {
            CharacterMainControl characterMainControl = CharacterMainControl.Main;
            if (!characterMainControl) return;

            foreach (Item item in characterMainControl.CharacterItem.Inventory.Content)
            {
                if (item && item.Repairable)
                {
                    __result.Add(item);
                }
            }
        }
    }
}

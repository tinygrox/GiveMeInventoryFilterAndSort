using System;
using Cysharp.Threading.Tasks;
using Duckov.UI;
// ReSharper disable InconsistentNaming

namespace tinygrox.DuckovMods.GiveMeInventoryFilter.HarmonyPatches
{
    public static class StockShopViewPatches
    {
        public static void OnOpenPostfix(InventoryDisplay ___playerStorageDisplay)
        {
            AddFilterObjectAsync(___playerStorageDisplay).Forget();
        }
        private static async UniTaskVoid AddFilterObjectAsync(InventoryDisplay playerStorageInventory)
        {
            try
            {
                await UniTask.WaitUntil(() => playerStorageInventory);
                var IFD = playerStorageInventory.GetComponentInChildren<InventoryFilterDisplay>();
                if (IFD)
                {
                    StockShopViewInventoryFilter.SelectFilterEntryMethod(IFD, new object[] { 0 });
                }
            }
            catch (Exception e)
            {
                ModLogger.Log.Error($"[GiveMeInventoryFilter] {e}");
            }
        }
    }
}

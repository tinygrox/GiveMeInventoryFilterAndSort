using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Duckov.Economy.UI;
using Duckov.UI;
using Duckov.Utilities;
using FMOD;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
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

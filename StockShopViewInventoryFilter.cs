using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Duckov.Economy.UI;
using Duckov.UI;
using ItemStatsSystem;
using SodaCraft.Localizations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace tinygrox.DuckovMods.GiveMeInventoryFilter
{
    // 计划结果：在左侧的仓库顶部设置一个 gameobject 用于挂载 InventoryFilterDisplay
    // 思路：1. 先创建一个 gameobject 在左侧仓库最上方， setParent 为大的容器
    // 2. 给其下的每个 Inventory 的 GameObject 都挂上 InventoryFilterProvider
    // 3. 配置 InventoryFilterDisplay 的 InventoryFilterDisplay.provider、InventoryFilterDisplay.targetDisplay
    public class StockShopViewInventoryFilter: MonoBehaviour
    {
        // public static readonly Func<InventoryFilterDisplay, InventoryFilterProvider> GetProvider = ReflectionHelper.CreateFieldGetter<InventoryFilterDisplay, InventoryFilterProvider>("provider");
        // public static readonly Action<InventoryFilterDisplay, InventoryFilterProvider> SetProvider = ReflectionHelper.CreateFieldSetter<InventoryFilterDisplay, InventoryFilterProvider>("provider");
        // public static readonly Func<InventoryFilterDisplay, InventoryDisplay> GetTargetDisplay = ReflectionHelper.CreateFieldGetter<InventoryFilterDisplay, InventoryDisplay>("targetDisplay");
        // public static readonly Action<InventoryFilterDisplay, InventoryDisplay> SetTargetDisplay = ReflectionHelper.CreateFieldSetter<InventoryFilterDisplay, InventoryDisplay>("targetDisplay");

        public static readonly Func<StockShopView, InventoryDisplay> GetPlayerInventoryDisplay = ReflectionHelper.CreateFieldGetter<StockShopView, InventoryDisplay>("playerInventoryDisplay");
        // public static readonly Action<StockShopView, InventoryDisplay> SetPlayerInventoryDisplay = ReflectionHelper.CreateFieldSetter<StockShopView, InventoryDisplay>("playerInventoryDisplay");
        // public static readonly Func<StockShopView, InventoryDisplay> GetPetInventoryDisplay = ReflectionHelper.CreateFieldGetter<StockShopView, InventoryDisplay>("petInventoryDisplay");
        // public static readonly Action<StockShopView, InventoryDisplay> SetPetInventoryDisplay = ReflectionHelper.CreateFieldSetter<StockShopView, InventoryDisplay>("petInventoryDisplay");
        public static readonly Func<StockShopView, InventoryDisplay> GetPlayerStorageDisplay = ReflectionHelper.CreateFieldGetter<StockShopView, InventoryDisplay>("playerStorageDisplay");
        // public static readonly Action<StockShopView, InventoryDisplay> SetPlayerStorageDisplay = ReflectionHelper.CreateFieldSetter<StockShopView, InventoryDisplay>("playerStorageDisplay");

        public static readonly Action<InventoryFilterDisplay, object[]> SelectFilterEntryMethod = ReflectionHelper.CreateVoidMethodCaller<InventoryFilterDisplay>("Select", typeof(int));
        private static bool s_initialized = false;
        private void OnEnable()
        {
            try
            {
                ManagedUIElement.onOpen += OnInitialized;
            }
            catch (Exception e)
            {
                ModLogger.Log.Error($"{e}");
            }
        }

        private void OnInitialized(ManagedUIElement managedUIElement)
        {
            // Debug.Log("[GiveMeInventoryFilter] OnInitialized");
            if (managedUIElement is StockShopView)
                WatchAndSetupUIAsync().Forget();
        }

        private async UniTask<bool> WatchAndSetupUIAsync()
        {
            try
            {
                var stockShopView = StockShopView.Instance;
                await UniTask.WaitUntil(() =>
                {
                    if (!stockShopView)
                    {
                        stockShopView = StockShopView.Instance;
                    }
                    return stockShopView;
                });

                var filterParent = stockShopView.transform.Find("Content/SelfStuff/Content/Scroll View/Viewport/Content/InventoryDisplay_Trading_Storage");
                await UniTask.WaitUntil(() => filterParent);

                InventoryDisplay playerStorageDisplay = GetPlayerStorageDisplay(stockShopView);
                SetupFilterGameObject(filterParent, playerStorageDisplay).Forget();
                filterParent = stockShopView.transform.Find("Content/SelfStuff/Content/Scroll View/Viewport/Content/InventoryDisplay_Trading");
                await UniTask.WaitUntil(() => filterParent);

                InventoryDisplay playerInventoryDisplay = GetPlayerInventoryDisplay(stockShopView);
                bool playerInventoryshowspace = playerInventoryDisplay.Editable && !playerInventoryDisplay.ShowSortButton;
                SetupFilterGameObject(filterParent, playerInventoryDisplay, playerInventoryshowspace).Forget();
                s_initialized = true;
            }
            catch (Exception ex)
            {
                s_initialized = false;
                // Debug.LogError($"[GiveMeInventoryFilter] {ex}");
                ModLogger.Log.Error($"{ex}");
            }

            return s_initialized;
        }

        private async UniTaskVoid SetupFilterGameObject(Transform filterParent, InventoryDisplay targetInventoryDisplay, bool showSpace = true, bool showDropdown = true)
        {
            ModLogger.Log.Debug("StockShopView WatchAndSetupUIAsync");

            await UniTask.WaitUntil(() => targetInventoryDisplay && targetInventoryDisplay.Target);

            if(!StockShopView.Instance) return;
            if (filterParent.transform.Find($"GiveMeInventoryFilter_FilterDisplay({filterParent.name})"))
            {
                return;
            }
            var fliterDisplayObj = Instantiate(UIPrefabs.FilterDisplay, filterParent);
            fliterDisplayObj.name = $"GiveMeInventoryFilter_FilterDisplay({filterParent.name})";
            if (filterParent.TryGetComponent(out VerticalLayoutGroup vlg))
            {
                vlg.spacing = 8f;
            }
            fliterDisplayObj.transform.localScale = Vector3.one;
            fliterDisplayObj.SetActive(showDropdown);
            InventoryFilterDisplay iFdComp = fliterDisplayObj.GetComponent<InventoryFilterDisplay>();
            if (iFdComp)
            {
                // s_fliterDisplayObj.transform.SetSiblingIndex(targetInventoryDisplay.transform.GetSiblingIndex());
                fliterDisplayObj.transform.SetAsFirstSibling();
                iFdComp.Setup(targetInventoryDisplay);
                SelectFilterEntryMethod(iFdComp, new object[] { 0 });
                // ModLogger.Log.Error()
                // Debug.Log("[GiveMeInventoryFilter] s_selectFilterEntryMethod Finished");
            }
            Utilities.AddDropdown(filterParent.Find("TitleBar (1)"), targetInventoryDisplay, showSpace, showDropdown);
        }

        private void OnDisable()
        {
            // Debug.Log("[GiveMeInventoryFilter] OnDisable");
            // s_fliterDisplayObj = null;
            ManagedUIElement.onOpen -= OnInitialized;
        }
    }
}

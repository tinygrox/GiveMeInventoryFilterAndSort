using System;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using tinygrox.DuckovMods.GiveMeInventoryFilter.SharedCode;
using UnityEngine;

namespace tinygrox.DuckovMods.GiveMeInventoryFilter;

public class LootViewFilter : MonoBehaviour
{
    public static Func<LootView, InventoryDisplay> GetPlayerInventoryDisplay;

    public static Func<LootView, InventoryDisplay> GetLootTargetInventoryDisplay;
    // private GameObject _dropdownObj;

    private void Awake()
    {
        GetPlayerInventoryDisplay = ReflectionHelper.CreateFieldGetter<LootView, InventoryDisplay>("characterInventoryDisplay");
        GetLootTargetInventoryDisplay = ReflectionHelper.CreateFieldGetter<LootView, InventoryDisplay>("lootTargetInventoryDisplay");
    }

    private void OnEnable()
    {
        try
        {
            // Debug.Log("[GiveMeInventoryFilter] LootViewFilter OnEnable");
            ManagedUIElement.onOpen += OnInitialized;
        }
        catch (Exception e)
        {
            ModLogger.Log.Error($"[GiveMeInventoryFilter] {e}");
            // Debug.LogError($"[GiveMeInventoryFilter] {e}");
        }
    }

    private void OnDisable() =>
        // Debug.Log("[GiveMeInventoryFilter] LootViewFilter OnDisable");
        ManagedUIElement.onOpen -= OnInitialized;

    private void OnInitialized(ManagedUIElement managedUIElement)
    {
        // Debug.Log("[GiveMeInventoryFilter] LootViewFilter OnInitialized");
        if (managedUIElement is LootView)
        {
            WatchAndSetupUIAsync().Forget();
        }
    }

    private async UniTaskVoid WatchAndSetupUIAsync()
    {
        try
        {
            Stopwatch timer = Stopwatch.StartNew();
            ModLogger.Log.Debug("LootView WatchAndSetupUIAsync");
            await UniTask.WaitUntil(() => LootView.Instance);

            LootView lootView = LootView.Instance;
            InventoryDisplay characterInventoryDisplay = GetPlayerInventoryDisplay(lootView);
            InventoryDisplay lootTargetInventoryDisplay = GetLootTargetInventoryDisplay(lootView);

            await UniTask.WaitUntil(() =>
            {
                if (!characterInventoryDisplay)
                {
                    characterInventoryDisplay = GetPlayerInventoryDisplay(lootView);
                }

                if (!lootTargetInventoryDisplay)
                {
                    lootTargetInventoryDisplay = GetLootTargetInventoryDisplay(lootView);
                }

                return characterInventoryDisplay && lootTargetInventoryDisplay;
            });

            Transform characterInventoryDisplayTransorm = characterInventoryDisplay.transform;
            bool characterInventoryDisplayTransormshowspace = characterInventoryDisplay.Editable && !characterInventoryDisplay.ShowSortButton;

            Transform lootTargetInventoryDisplayTransform = lootTargetInventoryDisplay.transform;
            bool lootTargetInventoryDisplayshowSpace = lootTargetInventoryDisplay.Editable && !lootTargetInventoryDisplay.ShowSortButton;
            bool lootTargetInventoryDisplayshowDropdown = lootTargetInventoryDisplay.Target == PlayerStorage.Inventory && lootTargetInventoryDisplay.Editable;
            ModLogger.Log.Debug($"lootTargetInventoryDisplayshowSpace:{lootTargetInventoryDisplayshowSpace}");
            ModLogger.Log.Debug($"lootTargetInventoryDisplayshowDropdown: {lootTargetInventoryDisplayshowDropdown}");

            if (IsDropdownExisted(characterInventoryDisplayTransorm, "TitleBar (1)/Filter_Dropdown") &&
                IsDropdownExisted(lootTargetInventoryDisplayTransform, "TitleBar (1)/Filter_Dropdown"))
            {
                Utilities.RefreshDropdownActive(lootTargetInventoryDisplayTransform.Find("TitleBar (1)/Filter_Dropdown").gameObject, lootTargetInventoryDisplayshowDropdown);
                timer.Stop();
                ModLogger.Log.Debug($"[提前结束]运行时长：{timer.ElapsedMilliseconds}ms");
                return;
            }

            ModLogger.Log.Debug($"Checking conditions for lootTargetInventoryDisplay({characterInventoryDisplay.Target?.DisplayName}): Editable={characterInventoryDisplay.Editable} | ShowSortButton={characterInventoryDisplay.ShowSortButton}");
            ModLogger.Log.Debug($"Checking conditions for lootTargetInventoryDisplay({lootTargetInventoryDisplay.Target?.DisplayName}): Editable={lootTargetInventoryDisplay.Editable}, ShowSortButton={lootTargetInventoryDisplay.ShowSortButton} lootTargetInventoryDisplay.Target == PlayerStorage.Inventory:{lootTargetInventoryDisplay.Target == PlayerStorage.Inventory}");

            SetupFilterGameObject(characterInventoryDisplayTransorm, characterInventoryDisplay, characterInventoryDisplayTransormshowspace).Forget();
            SetupFilterGameObject(lootTargetInventoryDisplayTransform, lootTargetInventoryDisplay, lootTargetInventoryDisplayshowSpace, lootTargetInventoryDisplayshowDropdown).Forget();
            timer.Stop();
            ModLogger.Log.Debug($"运行时长：{timer.ElapsedMilliseconds}ms");
        }
        catch (Exception ex)
        {
            ModLogger.Log.Error($"{ex}");
            // Debug.LogError($"[GiveMeInventoryFilter] {ex}");
        }
    }

    private bool IsDropdownExisted(Transform parent, string path)
    {
        bool existed = parent?.Find(path)?.gameObject;
        ModLogger.Log.Debug($"IsDropdownExisted: {existed}");
        return existed;
    }

    private async UniTaskVoid SetupFilterGameObject(Transform filterParent, InventoryDisplay targetInventoryDisplay, bool showSpace = true, bool showDropdown = true)
    {
        // 必须要先等待 InventoryDisplay Setup 完毕
        await UniTask.WaitUntil(() => targetInventoryDisplay && targetInventoryDisplay.Target);

        await UniTask.WaitForEndOfFrame(this);
        if (!LootView.Instance)
        {
            return;
        }

        // Utilities.AddDropdown(filterParent.Find("TitleBar (1)"), targetInventoryDisplay, showSpace, showDropdown);
        Utilities.AddDropdown(filterParent, targetInventoryDisplay, showSpace, showDropdown);
    }
}

using Duckov.Economy.UI;
using Duckov.MasterKeys.UI;
using Duckov.UI;
using HarmonyLib;
using SodaCraft.Localizations;
using tinygrox.DuckovMods.GiveMeInventoryFilter.HarmonyPatches;
using tinygrox.DuckovMods.GiveMeInventoryFilter.SharedCode;
using UnityEngine;

namespace tinygrox.DuckovMods.GiveMeInventoryFilter;

public class ModBehaviour : Duckov.Modding.ModBehaviour
{
    private const string HarmonyId = "tinygrox.DuckovMods.StockViewInventoryFilter";
    public static GameObject DropdownObjTemplate;
    private Harmony _harmony;

    private void Awake()
    {
        LocalizationUtility.LoadLanguageFile(LocalizationManager.CurrentLanguage);
        LoadSettings();
        _harmony = new Harmony(HarmonyId);
        _harmony.Patch(
            AccessTools.Method(typeof(FormulasRegisterView), "OnOpen"),
            postfix: new HarmonyMethod(typeof(FormulasRegisterViewPatches), nameof(FormulasRegisterViewPatches.OnOpenPostfix))
        );
        _harmony.Patch(
            AccessTools.Method(typeof(StockShopView), "OnOpen"),
            postfix: new HarmonyMethod(typeof(StockShopViewPatches), nameof(StockShopViewPatches.OnOpenPostfix))
        );
        _harmony.Patch(
            AccessTools.Method(typeof(MasterKeysRegisterView), "OnOpen"),
            postfix: new HarmonyMethod(typeof(MasterKeysRegisterViewPatches), nameof(MasterKeysRegisterViewPatches.OnOpenPostfix))
        );
        _harmony.Patch(
            AccessTools.Method(typeof(BitcoinMinerView), "OnOpen"),
            postfix: new HarmonyMethod(typeof(BitcoinMinerViewPatches), nameof(BitcoinMinerViewPatches.OnOpenPostfix))
        );
        _harmony.Patch(
            AccessTools.Method(typeof(ItemDecomposeView), "OnOpen"),
            postfix: new HarmonyMethod(typeof(ItemDecomposeViewPatches), nameof(ItemDecomposeViewPatches.OnOpenPostfix))
        );
        _harmony.Patch(
            AccessTools.Method(typeof(ItemRepairView), nameof(ItemRepairView.GetAllEquippedItems)),
            postfix: new HarmonyMethod(typeof(ItemRepairViewPatches), nameof(ItemRepairViewPatches.GetAllEquippedItemsPostfix))
        );
    }

    private void LoadSettings() => ModLogger.Instance.DefaultModName = "GiveMeInventoryFilter";

    protected override void OnAfterSetup()
    {
        LocalizationManager.OnSetLanguage += LocalizationUtility.LoadLanguageFile;
        if (!gameObject.GetComponent<StockShopViewInventoryFilter>())
        {
            gameObject.AddComponent<StockShopViewInventoryFilter>();
        }

        if (!gameObject.GetComponent<LootViewFilter>())
        {
            gameObject.AddComponent<LootViewFilter>();
        }
    }

    protected override void OnBeforeDeactivate()
    {
        LocalizationManager.OnSetLanguage -= LocalizationUtility.LoadLanguageFile;
        if (gameObject.TryGetComponent<StockShopViewInventoryFilter>(out StockShopViewInventoryFilter s))
        {
            Destroy(s);
        }
    }
}

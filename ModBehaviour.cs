
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Duckov.Economy.UI;
using Duckov.MasterKeys.UI;
using Duckov.UI;
using HarmonyLib;
using ItemStatsSystem;
using Newtonsoft.Json;
using SodaCraft.Localizations;
using tinygrox.DuckovMods.GiveMeInventoryFilter.HarmonyPatches;
using UnityEngine;
using FormulasRegisterView = Duckov.UI.FormulasRegisterView;

namespace tinygrox.DuckovMods.GiveMeInventoryFilter
{
    public class ModBehaviour: Duckov.Modding.ModBehaviour
    {
        private const string HarmonyId = "tinygrox.DuckovMods.StockViewInventoryFilter";
        private Harmony _harmony;
        public static GameObject DropdownObjTemplate;

        private void LoadLanguageFile(SystemLanguage language)
        {
            string langName = language.ToString();
            string langFilePath = GetLocalizationFilePath(langName);
            if (!File.Exists(langFilePath))
            {
                langFilePath = GetLocalizationFilePath("English");

                if(!File.Exists(langFilePath)) return;
            }

            string jsonContent = File.ReadAllText(langFilePath);
            var localizedStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);

            foreach (var pair in localizedStrings)
            {
                LocalizationManager.SetOverrideText(pair.Key, pair.Value);
            }
        }

        private static string GetLocalizationFilePath(string langName)
        {
            string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            return assemblyDir != null ? Path.Combine(assemblyDir, "Localization", $"{langName}.json") : null;
        }
        private void LoadSettings()
        {
            ModLogger.Instance.DefaultModName = "GiveMeInventoryFilter";
        }
        private void Awake()
        {
            LoadLanguageFile(LocalizationManager.CurrentLanguage);
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

        protected override void OnAfterSetup()
        {
            LocalizationManager.OnSetLanguage += LoadLanguageFile;
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
            LocalizationManager.OnSetLanguage -= LoadLanguageFile;
            if (gameObject.TryGetComponent<StockShopViewInventoryFilter>(out var s))
            {
                Destroy(s);
            }
        }
    }
}

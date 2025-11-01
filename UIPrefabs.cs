using Duckov.Utilities;
using UnityEngine;

namespace tinygrox.DuckovMods.GiveMeInventoryFilter
{
    public static class UIPrefabs
    {
        public static readonly GameObject FilterDisplay;
        public static readonly GameObject UI_DropDown;

        static UIPrefabs()
        {
            var levelManager = GameplayDataSettings.Prefabs.LevelManagerPrefab.gameObject;

            var gameplayUICanvas = levelManager.transform.Find("GameplayUICanvas").gameObject;

            FilterDisplay = Object.Instantiate(gameplayUICanvas.transform.Find("Tabs/ViewArea/LootView/Main/LootTarget/Content/FilterDisplay").gameObject, null);
            FilterDisplay.name = "GiveMeInventoryFilter_FilterDisplayTemplate";
            Object.DontDestroyOnLoad(FilterDisplay); // 留着给后续一直生成

            // UI_DropDown = Object.Instantiate();
        }

    }
}

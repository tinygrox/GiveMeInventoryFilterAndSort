using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using Duckov.Utilities;
using ItemStatsSystem;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace tinygrox.DuckovMods.GiveMeInventoryFilter
{
    // 解决傻逼下拉菜单不居中到选项的问题和切换语言的问题
     public class DropdownScrollbarAutoScroll : MonoBehaviour
    {
        private ScrollRect _scrollRect;
        private TMP_Dropdown _dropdown;

        private void Start()
        {
            _dropdown = GetComponentInParent<TMP_Dropdown>();
            if (_dropdown == null)
            {
                ModLogger.Log.Error("DropdownScrollbarAutoScroll could not find TMP_Dropdown.");
                return;
            }

            _scrollRect = GetComponentInChildren<ScrollRect>(true);
            if (_scrollRect == null)
            {
                ModLogger.Log.Warning("ScrollRect not found in children.");
            }

            // 启动最终的异步设置流程
            FinalizeSetupAsync().Forget();
        }

        private async UniTask FinalizeSetupAsync()
        {
            if (!_scrollRect) return;

            // --- 阶段 1: 等待布局初步稳定 ---
            // 我们循环等待，直到Content的高度连续两帧保持不变，这标志着布局计算已完成。
            float previousHeight = -1f;
            int stableFrames = 0;
            while (stableFrames < 2) // 要求连续稳定2帧，非常保险
            {
                await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
                float currentHeight = _scrollRect.content.rect.height;
                if (Mathf.Approximately(currentHeight, previousHeight))
                {
                    stableFrames++;
                }
                else
                {
                    stableFrames = 0;
                }
                previousHeight = currentHeight;
            }

            // --- 阶段 2: 隐藏滚动条并再次稳定布局 ---
            if (_scrollRect.verticalScrollbar)
            {
                _scrollRect.verticalScrollbar.gameObject.SetActive(false);
            }

            // 隐藏滚动条会改变Content的宽度，所以需要强制重建布局
            LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
            // 再等待一帧，让所有UI元素都更新到最终状态
            await UniTask.WaitForEndOfFrame(this);

            // --- 阶段 3: 执行精确定位 ---
            await GoToSelected();
        }

        private async UniTask GoToSelected()
        {
            if (!_scrollRect) return;

            // 1. 【必要的延迟】等待一帧的结束。
            //    这给了UI系统时间去计算初始布局，是这个简单方案能工作的最低要求。
            await UniTask.WaitForEndOfFrame(this);

            // 2. 【基础算法】计算比例
            int selectedOption = _dropdown.value;
            int totalOptions = _dropdown.options.Count;

            if (totalOptions <= 1)
            {
                _scrollRect.verticalNormalizedPosition = 1f;
                return;
            }

            // 使用浮点数进行除法，避免整数除法问题
            float ratio = (float)selectedOption / (totalOptions - 1f);
            float newScrollPos = 1f - ratio;

            // 3. 【必要的修正】使用 Clamp01 避免浮点数精度误差
            //    这将确保结果永远在 [0, 1] 范围内。
            _scrollRect.verticalNormalizedPosition = Mathf.Clamp01(newScrollPos);;
        }
    }

    public class DropOptionsLocalization : MonoBehaviour
    {
        private TMP_Dropdown _dropdown;

        private void Awake()
        {
            _dropdown = GetComponent<TMP_Dropdown>();
            LocalizationManager.OnSetLanguage += RefreshDropdownOptions;
        }

        public void RefreshDropdownOptions(SystemLanguage language)
        {
            ModLogger.Log.Info($"RefreshDropdownOptions!");
            if(!_dropdown) return;
            ModLogger.Log.Info($"No Return!");

            int currentValue = _dropdown.value;
            if (currentValue > _dropdown.options.Count)
            {
                currentValue = 0;
            }
            Utilities.PopulateDropdown(_dropdown);

            ModLogger.Log.Info($"Options Clear!");
            _dropdown.value = currentValue;
            _dropdown.RefreshShownValue();
            ModLogger.Log.Info($"Options refresh!");
        }

        private void OnDestroy()
        {
            LocalizationManager.OnSetLanguage -= RefreshDropdownOptions;
        }
    }


    public static class Utilities
    {
        public static readonly Dictionary<InventoryDisplay, int> CurrentSortIndex = new Dictionary<InventoryDisplay, int>();
        public static readonly List<string> DropdownOptions = new List<string>
        {
            "GiveMeInventoryFilter_Default",
            "GiveMeInventoryFilter_OrderByValue",
            "GiveMeInventoryFilter_OrderByValueAscending",
            "GiveMeInventoryFilter_OrderByWeight",
            "GiveMeInventoryFilter_OrderByWeightAscending",
            "GiveMeInventoryFilter_OrderByValueWeightRatio",
            "GiveMeInventoryFilter_OrderByValueWeightRatioAscending",
            "GiveMeInventoryFilter_OrderByRarity",
            "GiveMeInventoryFilter_OrderByRarityAscending",
            "GiveMeInventoryFilter_OrderByDurbility",
            "GiveMeInventoryFilter_OrderByDurbilityAscending"
        };

        public static List<string> GetTranslatedOptions()
        {
            return DropdownOptions.Select(key => key.ToPlainText()).ToList();
        }

        public static void ExecuteSortByIndex(InventoryDisplay inventoryDisplay, int currentIndex)
        {
            if (!inventoryDisplay) return;
            Stopwatch timer = Stopwatch.StartNew();
            CurrentSortIndex[inventoryDisplay] = currentIndex;
            bool isAscending;
            if(!inventoryDisplay) return;
            switch (currentIndex)
            {
                case 0:
                    InventorySortUtility.VanillaSort(inventoryDisplay);
                    break;
                case 1:
                case 2:
                    isAscending = (currentIndex == 2);
                    InventorySortUtility.SortInventoryByValue(inventoryDisplay, isAscending);
                    break;
                case 3:
                case 4:
                    isAscending = (currentIndex == 4);
                    InventorySortUtility.SortInventoryByWeight(inventoryDisplay, isAscending);
                    break;
                case 5:
                case 6:
                    isAscending = (currentIndex == 6);
                    InventorySortUtility.SortInventoryByValueWeightRatio(inventoryDisplay, isAscending);
                    break;
                case 7:
                case 8:
                    isAscending = currentIndex == 8;
                    InventorySortUtility.SortInventoryByRarity(inventoryDisplay, isAscending);
                    break;
                case 9:
                case 10:
                    isAscending = currentIndex == 10;
                    InventorySortUtility.SortInventoryByDurbility(inventoryDisplay, isAscending);
                    break;
            }
            ModLogger.Log.Info($"'{currentIndex}' 排序 {inventoryDisplay.Target.Content.Count} 物品花费了 {timer.ElapsedMilliseconds}ms");
        }

        // 联动我的Show Sort Button

        public static (int SortType, bool IsAscending) GetCurrentSortState(InventoryDisplay inventoryDisplay)
        {
            if (!CurrentSortIndex.TryGetValue(inventoryDisplay, out int index))
            {
                index = 0;
            }

            if (index < 0 || index >= DropdownOptions.Count)
            {
                return (0, false);
            }

            int sortType = index;
            bool isAscending = index % 2 == 0;

            return (sortType, isAscending);
        }

        public static int SetNewAscending(InventoryDisplay inventoryDisplay, bool newAscending)
        {
            if (!CurrentSortIndex.TryGetValue(inventoryDisplay, out int currentIndex))
            {
                currentIndex = 0;
            }

            int typeBase = currentIndex / 2 * 2;

            int newIndex = typeBase + (newAscending ? 1 : 0);
            CurrentSortIndex[inventoryDisplay] = newIndex;

            return newIndex;
        }

        public static Func<Item, bool> SetFilterFunction(params string[] tagsToMatch)
        {
            if (tagsToMatch == null || tagsToMatch.Length == 0)
            {
                return item => false;
            }

            return e =>
            {
                if (!e || e.Tags == null)
                {
                    return false;
                }

                return e && tagsToMatch.Any(tag => e.Tags.Contains(tag));
            };
        }

        public static T FindComponentInChild<T>(Transform parent) where T : Component
        {
            foreach (Transform child in parent)
            {
                T component = child.GetComponent<T>();
                if (component)
                {
                    return component;
                }
            }

            foreach (Transform child in parent)
            {
                T component = FindComponentInChild<T>(child);
                if (component)
                {
                    return component;
                }
            }

            return null;
        }
        public static void RefreshDropdownActive(GameObject dropdownObj, bool isShow)
        {
            if (dropdownObj)
            {
                dropdownObj.SetActive(isShow);
            }
        }
        public static void AddDropdown(Transform inventoryDisplayTransform, InventoryDisplay inventoryDisplay, bool showSpace = true, bool showDropdown = true)
        {
            ModLogger.Log.Info($"{inventoryDisplay.Target.DisplayName} showSpace:{showSpace}, showDropdown: {showDropdown}");
            // 我看过了只有 TitleBar (1) 有这个 HorizontalLayoutGroup
            var titleBar1 = inventoryDisplayTransform.GetComponentInChildren<HorizontalLayoutGroup>();
            if (!titleBar1)
            {
                return;
            }
            Transform parent = titleBar1.transform;
            if (parent.GetComponentInChildren<DropOptionsLocalization>())
            {
                var existingDropdown = parent.GetComponentInChildren<DropOptionsLocalization>().gameObject;
                RefreshDropdownActive(existingDropdown, showDropdown);
                return;
            }

            var dropdownObj = parent.Find($"Filter_Dropdown")?.gameObject;
            if (dropdownObj)
            {
                RefreshDropdownActive(dropdownObj, showDropdown);
                return;
            }

            if (ModBehaviour.DropdownObjTemplate)
            {
                dropdownObj = UnityEngine.Object.Instantiate(ModBehaviour.DropdownObjTemplate);
            }
            else
            {
                ModBehaviour.DropdownObjTemplate = UnityEngine.Object.Instantiate(GameObject.Find("GameManager (from Startup)/PauseMenu/Menu/OptionsPanel/ScrollView/Viewport/Content/Common/UI_Resolution/Dropdown"), null);
                if (ModBehaviour.DropdownObjTemplate)
                {
                    UnityEngine.Object.DontDestroyOnLoad(ModBehaviour.DropdownObjTemplate);
                    dropdownObj = UnityEngine.Object.Instantiate(ModBehaviour.DropdownObjTemplate);
                }
                else
                {
                    ModLogger.Log.Error($"找不到 GameManager (from Startup)/PauseMenu/Menu/OptionsPanel/ScrollView/Viewport/Content/Common/UI_Resolution/Dropdown");
                    return;
                }
            }
            var space = parent.transform.Find("space");
            if (space)
            {
                space.gameObject.SetActive(showSpace);
            }
            if (dropdownObj)
            {
                // dropdownObj.name = $"Filter_Dropdown({inventoryDisplay.name})";
                dropdownObj.name = $"Filter_Dropdown";
                dropdownObj.transform.SetParent(parent);
                dropdownObj.transform.localScale = Vector3.one;
                dropdownObj.transform.SetSiblingIndex(4);
                var dropdownComp = dropdownObj.GetComponent<TMP_Dropdown>();
                if (dropdownComp)
                {
                    var captionText = dropdownComp.captionText as TextMeshProUGUI;
                    if (captionText)
                    {
                        captionText.overflowMode = TextOverflowModes.Ellipsis;
                    }
                    var itemLabel = dropdownComp.itemText as TextMeshProUGUI;
                    if (itemLabel)
                    {
                        itemLabel.enableAutoSizing = false;
                        itemLabel.fontSize = 20f;
                    }
                    dropdownComp.onValueChanged.RemoveAllListeners();
                    dropdownComp.onValueChanged.AddListener(delegate
                    {
                        DropdownValueChanged(dropdownComp, inventoryDisplay);
                    });
                    PopulateDropdown(dropdownComp);
                    dropdownObj.AddComponent<DropOptionsLocalization>();
                }

                dropdownObj.SetActive(showDropdown);
            }
        }
        public static void PopulateDropdown(TMP_Dropdown dropdown)
        {
            if (!dropdown) return;
            dropdown.options.Clear();
            dropdown.AddOptions(GetTranslatedOptions());
            dropdown.value = 0;
            dropdown.RefreshShownValue();

            var itemLabel = dropdown.itemText as TextMeshProUGUI;
            var templateRect = dropdown.template;

            if (itemLabel && templateRect)
            {
                float maxWidth = 0;
                foreach (TMP_Dropdown.OptionData optionData in dropdown.options)
                {
                    if (itemLabel)
                    {
                        float preferredWidth = itemLabel.GetPreferredValues(optionData.text).x;
                        if (preferredWidth > maxWidth)
                        {
                            maxWidth = preferredWidth;
                        }
                    }
                }
                ModLogger.Log.Info($"{templateRect.name}'s MaxWidth: {maxWidth}");
                templateRect.sizeDelta = new Vector2(maxWidth + 20f, dropdown.options.Count * 40f + 20f);
            }
        }
        private static void DropdownValueChanged(TMP_Dropdown dropdown, InventoryDisplay inventoryDisplay)
        {
            ExecuteSortByIndex(inventoryDisplay, dropdown.value);
        }
    }
}

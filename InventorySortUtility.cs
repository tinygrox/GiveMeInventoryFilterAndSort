using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Duckov.UI;
using Duckov.Utilities;
using ItemStatsSystem;
using UnityEngine;

namespace tinygrox.DuckovMods.GiveMeInventoryFilter
{
    public static class InventorySortUtility
    {
        private static readonly Action<InventoryDisplay, object[]> s_onTargetSorted = ReflectionHelper.CreateVoidMethodCaller<InventoryDisplay>("OnTargetSorted", typeof(Inventory));
        private static readonly Action<Item, object[]> s_notifyItemRemoved = ReflectionHelper.CreateVoidMethodCaller<Item>("NotifyRemovedFromInventory", typeof(Inventory));
        // private static readonly Action<Item, object[]> s_notifyItemAdded = ReflectionHelper.CreateVoidMethodCaller<Item>("NotifyAddedToInventory", typeof(Inventory));

        public static List<Item> MergeSortedItems_Hashed_Queue(List<Item> items, out List<Item> itemsToDestroy)
        {
            itemsToDestroy = new List<Item>();
            if (items == null || items.Count == 0) return new List<Item>();

            var openQueues = new Dictionary<int, Queue<Item>>();
            var finalResult = new List<Item>();

            foreach (var current in items)
            {
                if (!current || current.StackCount <= 0) continue;

                if (!current.Stackable)
                {
                    finalResult.Add(current);
                    continue;
                }

                if (openQueues.TryGetValue(current.TypeID, out var queue))
                {
                    while (queue.Count > 0 && current.StackCount > 0)
                    {
                        var target = queue.Peek();
                        int space = target.MaxStackCount - target.StackCount;
                        if (space <= 0) { queue.Dequeue(); continue; } // 防御性检查

                        int move = Math.Min(space, current.StackCount);
                        target.SetInt("Count", target.StackCount + move);
                        current.SetInt("Count", current.StackCount - move);

                        if (target.StackCount >= target.MaxStackCount) queue.Dequeue();
                    }
                    if (queue.Count == 0) openQueues.Remove(current.TypeID);
                }

                if (current.StackCount > 0)
                {
                    finalResult.Add(current);
                    if (current.StackCount < current.MaxStackCount)
                    {
                        if (!openQueues.TryGetValue(current.TypeID, out var q2))
                        {
                            q2 = new Queue<Item>();
                            openQueues[current.TypeID] = q2;
                        }
                        q2.Enqueue(current);
                    }
                }
                else
                {
                    itemsToDestroy.Add(current);
                }
            }

            return finalResult;
        }

        private static Tag GetFirstTag(Item item)
        {
            if (!item || item.Tags == null || item.Tags.Count == 0)
            {
                return null;
            }

            return item.Tags.Get(0);
        }

        public static List<Item> GetSortedItems(IEnumerable<Item> itemsToSort, Action<SortConfigurator> configureSort)
        {
            SortConfigurator configurator = new SortConfigurator(itemsToSort);
            configureSort(configurator);
            return configurator.ToList();
        }

        public static void SortInventoryByCriteria(InventoryDisplay inventoryToSort, Action<SortConfigurator> configureSort)
        {
            // Stopwatch timer = Stopwatch.StartNew();
            Inventory targetInventory = inventoryToSort.Target;
            if (!targetInventory)
            {
                return;
            }

            try
            {
                List<Item> contentList = targetInventory.Content;
                List<Item> itemsToProcess = new List<Item>();
                List<int> unlockedIndices = new List<int>();

                for (int i = 0; i < contentList.Count; i++)
                {
                    if (!targetInventory.IsIndexLocked(i))
                    {
                        unlockedIndices.Add(i);
                        if (contentList[i] != null)
                        {
                            itemsToProcess.Add(contentList[i]);
                        }
                    }
                }

                // List<Item> mergedItems = MergeSortedItems_Hashed(itemsToProcess, out List<Item> itemsToDestroy);
                List<Item> mergedItems = MergeSortedItems_Hashed_Queue(itemsToProcess, out List<Item> itemsToDestroy);

                List<Item> sortedAndMergedItems = GetSortedItems(mergedItems, configureSort);

                Queue<Item> finalItemQueue = new Queue<Item>(sortedAndMergedItems);
                foreach (int index in unlockedIndices)
                {
                    contentList[index] = finalItemQueue.Count > 0 ? finalItemQueue.Dequeue() : null;
                }

                if (s_notifyItemRemoved != null)
                {
                    foreach (Item item in itemsToDestroy)
                    {
                        s_notifyItemRemoved(item, new object[] { targetInventory });
                    }
                }

                foreach (Item item in itemsToDestroy)
                {
                    if (item && item.gameObject)
                    {
                        UnityEngine.Object.Destroy(item.gameObject);
                    }
                }
            }
            finally
            {
                s_onTargetSorted(inventoryToSort, new object[] { targetInventory });
            }

            // timer.Stop();
            // ModLogger.Log.Info($"{}排序 {targetInventory.Content.Count} 物品花费了 {timer.ElapsedMilliseconds}ms");
        }

        public static void SortInventoryByValue(InventoryDisplay inventoryToSort, bool isAscending) =>
            SortInventoryByCriteria(inventoryToSort, config => config
                .OrderBy(item => item.GetTotalRawValue(), isAscending)
                // .ThenBy(item => item.TypeID)
            );

        public static void SortInventoryByWeight(InventoryDisplay inventoryToSort, bool isAscending) =>
            // SortInventoryByCriteria<float, IComparable>(inventoryToSort, item => item.TotalWeight, isAscending);
            SortInventoryByCriteria(inventoryToSort, config => config
                .OrderBy(item => item.TotalWeight, isAscending)
                // .ThenBy(item => item.TypeID)
            );

        public static void SortInventoryByValueWeightRatio(InventoryDisplay inventoryToSort, bool isAscending) =>
            // SortInventoryByCriteria<float, IComparable>(inventoryToSort, item => item.GetTotalRawValue() / item.TotalWeight, isAscending);
            SortInventoryByCriteria(inventoryToSort, config => config
                .OrderBy(item => item.GetTotalRawValue() / item.TotalWeight, isAscending)
                // .ThenBy(item => item.TypeID)
            );

        public static void SortInventoryByRarity(InventoryDisplay inventoryToSort, bool isAscending) =>
            // SortInventoryByCriteria(inventoryToSort, item => item.Quality, isAscending, item => item.GetTotalRawValue());
            SortInventoryByCriteria(inventoryToSort, config => config
                .OrderBy(item => item.Quality, isAscending)
                .ThenBy(item => item.TypeID)
                .ThenBy(item => item.GetTotalRawValue(), isAscending)
            );

        public static void SortInventoryByDurbility(InventoryDisplay inventoryToSort, bool isAscending) =>
            SortInventoryByCriteria(inventoryToSort, config => config
                .OrderBy(item => item.UseDurability ? 0 : 1)
                .ThenBy(item => item.Quality, isAscending)
                .ThenBy(item => item.TypeID)
                .ThenBy(item => item.Durability, isAscending)
                .ThenBy(item => item.MaxDurabilityWithLoss, isAscending)
            );

        public static void VanillaSort(InventoryDisplay inventoryToSort, bool noAscending = false)
        {
            SortInventoryByCriteria(inventoryToSort, config => config
                .OrderBy(item => GetFirstTag(item)?.Priority ?? -1)
                .ThenBy(item => (GetFirstTag(item)?.name ?? string.Empty).ToLowerInvariant())
                .ThenBy(item => item.Order)
                .ThenBy(item => item.TypeID)
            );
        }
    }
}

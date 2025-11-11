using System;
using System.Collections.Generic;
using System.Linq;
using ItemStatsSystem;

namespace tinygrox.DuckovMods.GiveMeInventoryFilter;

public class SortConfigurator
{
    private IOrderedEnumerable<Item> _orderedSource;
    private readonly IEnumerable<Item> _source;

    public SortConfigurator(IEnumerable<Item> source)
    {
        _source = source;
        _orderedSource = null;
    }

    public SortConfigurator OrderBy<TKey>(Func<Item, TKey> keySelector)
    {
        _orderedSource = _source.OrderBy(keySelector);
        return this;
    }

    public SortConfigurator OrderByDescending<TKey>(Func<Item, TKey> keySelector)
    {
        _orderedSource = _source.OrderByDescending(keySelector);
        return this;
    }

    public SortConfigurator ThenBy<TKey>(Func<Item, TKey> keySelector)
    {
        if (_orderedSource == null)
        {
            throw new InvalidOperationException("Cannot call ThenBy before OrderBy.");
        }

        _orderedSource = _orderedSource.ThenBy(keySelector);
        return this;
    }

    public SortConfigurator ThenByDescending<TKey>(Func<Item, TKey> keySelector)
    {
        if (_orderedSource == null)
        {
            throw new InvalidOperationException("Cannot call ThenByDescending before OrderBy.");
        }

        _orderedSource = _orderedSource.ThenByDescending(keySelector);
        return this;
    }

    public SortConfigurator OrderBy<TKey>(Func<Item, TKey> keySelector, bool isAscending) => isAscending ? OrderBy(keySelector) : OrderByDescending(keySelector);

    public SortConfigurator ThenBy<TKey>(Func<Item, TKey> keySelector, bool isAscending) => isAscending ? ThenBy(keySelector) : ThenByDescending(keySelector);

    public List<Item> ToList() => _orderedSource?.ToList() ?? _source.ToList();
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace BroughlikeMonoGame.Core;

public sealed class Inventory
{
    private readonly List<ItemDefinition?> _slots = [];

    public int SlotCount => _slots.Count;

    public IReadOnlyList<ItemDefinition?> Slots => _slots;

    public ItemDefinition? GetItem(int index) => index >= 0 && index < _slots.Count ? _slots[index] : null;

    public bool ContainsItem(string itemId)
        => _slots.Any(item => string.Equals(item?.Id, itemId, StringComparison.OrdinalIgnoreCase));

    public void AddSlot(ItemDefinition? item = null) => _slots.Add(item);

    public void LoadFromIds(IEnumerable<string?> itemIds, Func<string, ItemDefinition> resolver)
    {
        _slots.Clear();
        foreach (var itemId in itemIds)
        {
            _slots.Add(itemId is null ? null : resolver(itemId));
        }
    }

    public IReadOnlyList<string?> ToItemIds() => _slots.Select(item => item?.Id).ToList();

    public bool TryConsume(int index, out ItemDefinition? item)
    {
        item = null;
        if (index < 0 || index >= _slots.Count)
        {
            return false;
        }

        item = _slots[index];
        if (item is null)
        {
            return false;
        }

        _slots[index] = null;
        return true;
    }

    public bool TryStore(ItemDefinition item, int? maxSlots = null)
    {
        for (var i = 0; i < _slots.Count; i++)
        {
            if (_slots[i] is null)
            {
                _slots[i] = item;
                return true;
            }
        }

        if (maxSlots is null || _slots.Count < maxSlots.Value)
        {
            _slots.Add(item);
            return true;
        }

        return false;
    }

    public void CopyBackwardIntoEmptySlots()
    {
        for (var i = _slots.Count - 1; i > 0; i--)
        {
            if (_slots[i] is null)
            {
                _slots[i] = _slots[i - 1];
            }
        }
    }
}

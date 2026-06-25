using System;
using System.Collections.Generic;
using System.Linq;

namespace RynthRemote.AcStatus;

/// Consumer-side mirror of the StatusAgent's GET &lt;url&gt;/inventory?pid=N payload
/// (schema "rynthcore.inventory/1") — one client's full READ-ONLY inventory.
/// Deserialized case-insensitively, same convention as AcStatusModels.
public sealed class AcInventoryPayload
{
    public string? Schema { get; set; }
    /// Monotonic; bumps on each producer rescan. Lets the UI tell a real change from a re-fetch.
    public int Version { get; set; }
    public int ItemCount { get; set; }
    public List<AcInventoryContainer> Containers { get; set; } = new();
    public List<AcInventoryItem> Items { get; set; } = new();

    /// Items in one container, ordered for display: equipped by body slot, packs by slot index.
    public IEnumerable<AcInventoryItem> ItemsIn(AcInventoryContainer c) =>
        c.IsEquipped
            ? Items.Where(i => i.ContainerId == c.Id).OrderBy(i => i.WieldedLocation)
            : Items.Where(i => i.ContainerId == c.Id).OrderBy(i => i.Slot);
}

/// One container grouping: the Equipped pseudo-container, the main pack, or a side pack.
public sealed class AcInventoryContainer
{
    public uint Id { get; set; }
    public string Name { get; set; } = "";
    public string Kind { get; set; } = "";   // "main" | "side" | "equipped"
    public int Capacity { get; set; }

    public bool IsEquipped => string.Equals(Kind, "equipped", StringComparison.OrdinalIgnoreCase);
}

/// One inventory item. Its <see cref="Appraisal"/> reuses <see cref="AcEquipItem"/> (identical JSON
/// field shape), so the existing appraisal view renders it unchanged. Appraisal is null for the
/// un-identified long tail (the producer only includes it cache-hit-only — never triggers an Assess).
public sealed class AcInventoryItem
{
    public uint Id { get; set; }
    public string Name { get; set; } = "";
    public uint Wcid { get; set; }
    public int ObjectClass { get; set; }
    public uint ContainerId { get; set; }
    public int Location { get; set; }
    public int Slot { get; set; }
    public int StackCount { get; set; } = 1;
    public uint IconDid { get; set; }
    public bool Equipped { get; set; }
    public int WieldedLocation { get; set; }
    public AcEquipItem? Appraisal { get; set; }

    /// AC-style hex instance id, e.g. "0x80001234".
    public string HexId => "0x" + Id.ToString("X8");
    /// True only when there's real appraisal detail to expand to.
    public bool HasAppraisal => Appraisal is { } a && a.HasAppraisal;
    /// "×N" only for genuine stacks (>1); empty for singletons so the row stays clean.
    public string StackLabel => StackCount > 1 ? "×" + StackCount.ToString("N0") : "";
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapMetrics;


public enum ItemTier
{
    Extreme,
    High,
    Mid,
    Low
}

public static class ItemManager
{
    // Probably move to a file later or let user customize it idk
    private static readonly Dictionary<string, ItemTier> ItemTiers = new()
    {
        {"Divine Orb", ItemTier.Extreme},
        {"Orb of Annulment", ItemTier.Extreme},
        {"Orb of Chance", ItemTier.Extreme},
        {"Mirror of Kalandra", ItemTier.Extreme},
        {"Perfect Jeweller's Orb", ItemTier.Extreme},
        {"Ancient Crisis Fragment", ItemTier.Extreme},
        {"Faded Crisis Fragment", ItemTier.Extreme},
        {"Weathered Crisis Fragment", ItemTier.Extreme},
        {"Greater Essence of the Mind", ItemTier.Extreme},
        {"Greater Essence of Enhancement", ItemTier.Extreme},
        {"Greater Essence of Ice", ItemTier.Extreme},
        {"Greater Essence of Electricity", ItemTier.Extreme},
        {"Greater Essence of the Infinite", ItemTier.Extreme},
        {"Greater Essence of Torment", ItemTier.Extreme},
        {"Greater Essence of Battle", ItemTier.Extreme},
        {"Greater Essence of Sorcery", ItemTier.Extreme},
        {"Greater Essence of Ruin", ItemTier.Extreme},
        {"Greater Essence of Haste", ItemTier.Extreme},
        {"Distilled Despair", ItemTier.Extreme},
        {"Distilled Fear", ItemTier.Extreme},
        {"Distilled Suffering", ItemTier.Extreme},
        {"Distilled Isolation", ItemTier.Extreme},
        {"Expedition Logbook", ItemTier.Extreme},

        {"Distilled Greed", ItemTier.High},
        {"Distilled Paranoia", ItemTier.High},
        {"Distilled Envy", ItemTier.High},
        {"Distilled Disgust", ItemTier.High},
        {"Chaos Orb", ItemTier.High},
        {"Exalted Orb", ItemTier.High},
        {"Greater Jeweller's Orb", ItemTier.High},
        {"Gemcutter's Prism", ItemTier.High},
        {"Glassblower's Bauble", ItemTier.High},
        {"Breach Splinter", ItemTier.High},
        {"Simulacrum Splinter", ItemTier.High},
        {"Essence of the Body", ItemTier.High},
        {"Essence of the Mind", ItemTier.High},
        {"Essence of Enhancement", ItemTier.High},
        {"Essence of Electricity", ItemTier.High},
        {"Essence of the Infinite", ItemTier.High},
        {"Essence of Flames", ItemTier.High},
        {"Essence of Ice", ItemTier.High},
        {"Essence of Torment", ItemTier.High},
        {"Essence of Battle", ItemTier.High},
        {"Essence of Sorcery", ItemTier.High},
        {"Essence of Ruin", ItemTier.High},
        {"Essence of Haste", ItemTier.High},
        {"Greater Essence of the Body", ItemTier.High},
        {"Greater Essence of Flames", ItemTier.High},
        {"Exotic Coinage", ItemTier.High},
        {"Adaptive Catalyst", ItemTier.High},
        {"Cowardly Fate", ItemTier.High},
        {"Deadly Fate", ItemTier.High},
        {"Victorious Fate", ItemTier.High},

        {"Distilled Ire", ItemTier.Mid},
        {"Distilled Guilt", ItemTier.Mid},
        {"Orb of Alchemy", ItemTier.Mid},
        {"Precursor Table", ItemTier.Mid},
        {"Regal Orb", ItemTier.Mid},
        {"Vaal Orb", ItemTier.Mid},
        {"Arcanist's Etcher", ItemTier.Mid},
        {"Armourer's Scrap", ItemTier.Mid},
        {"Blacksmith's Whetstone", ItemTier.Mid},
        {"Artificer's Orb", ItemTier.Mid},
        {"Broken Circle Artifact", ItemTier.Mid},
        {"Black Scythe Artifact", ItemTier.Mid},
        {"Order Artifact", ItemTier.Mid},
        {"Sun Artifact", ItemTier.Mid},
        {"Flesh Catalyst", ItemTier.Mid},
        {"Neural Catalyst", ItemTier.Mid},
        {"Carapace Catalyst", ItemTier.Mid},
        {"Xoph's Catalyst", ItemTier.Mid},
        {"Tul's Catalyst", ItemTier.Mid},
        {"Esh's Catalyst", ItemTier.Mid},
        {"Uul-Netol's Catalyst", ItemTier.Mid},
        {"Reaver Catalyst", ItemTier.Mid},
        {"Sibilant Catalyst", ItemTier.Mid},
        {"Chayula's Catalyst", ItemTier.Mid},
        {"Skittering Catalyst", ItemTier.Mid},

        {"Orb of Transmutation", ItemTier.Low},
        {"Orb of Augmentation", ItemTier.Low},
        {"Scroll of Wisdom", ItemTier.Low},
        {"Lesser Jeweller's Orb", ItemTier.Low},
        {"Chance Shard", ItemTier.Low},
        {"Regal Shard", ItemTier.Low},
        {"Transmutation Shard", ItemTier.Low}
    };

    public static ItemTier GetItemTier(string baseName)
    {
        return ItemTiers.TryGetValue(baseName, out var tier) ? tier : ItemTier.Mid;
    }

    public static System.Numerics.Vector4 GetTierColor(ItemTier tier)
    {
        return tier switch
        {
            ItemTier.Extreme => new System.Numerics.Vector4(0.9f, 0.1f, 0.1f, 1.0f),
            ItemTier.High => new System.Numerics.Vector4(0.9f, 0.7f, 0.0f, 1.0f),
            ItemTier.Mid => new System.Numerics.Vector4(0.7f, 0.7f, 0.7f, 1.0f),
            ItemTier.Low => new System.Numerics.Vector4(0.4f, 0.4f, 0.4f, 1.0f),
            _ => new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 1.0f)
        };
    }
}


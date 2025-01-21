using System.Numerics;
using ExileCore2.Shared.Enums;

namespace MapMetrics.UI;

public static class ColorHelper
{
    public static readonly Vector4 DefaultColor = new(1.0f, 1.0f, 1.0f, 1.0f);
    public static readonly Vector4 HeaderColor = new(0.9f, 0.7f, 0.0f, 1.0f);

    public static Vector4 GetRarityColor(MonsterRarity rarity)
    {
        return rarity switch
        {
            MonsterRarity.White => new Vector4(0.8f, 0.8f, 0.8f, 1.0f),
            MonsterRarity.Magic => new Vector4(0.3f, 0.3f, 0.9f, 1.0f),
            MonsterRarity.Rare => new Vector4(0.9f, 0.9f, 0.1f, 1.0f),
            MonsterRarity.Unique => new Vector4(0.8f, 0.4f, 0.0f, 1.0f),
            _ => DefaultColor
        };
    }
}
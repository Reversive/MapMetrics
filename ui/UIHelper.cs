using ImGuiNET;
using System.Numerics;

namespace MapMetrics.UI;

public static class UIHelper
{
    public static void DrawHeaderText(string text)
    {
        ImGui.TextColored(ColorHelper.HeaderColor, text);
    }

    public static bool BeginMainTable(string id, int columns, ImGuiTableFlags flags = ImGuiTableFlags.None)
    {
        return ImGui.BeginTable(id, columns, flags | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg);
    }

    public static void WithTable(string id, int columns, ImGuiTableFlags flags, System.Action tableContent)
    {
        if (BeginMainTable(id, columns, flags))
        {
            tableContent();
            ImGui.EndTable();
        }
    }
}
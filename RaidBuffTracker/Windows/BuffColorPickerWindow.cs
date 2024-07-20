using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using RaidBuffTracker.Toolbox;

namespace RaidBuffTracker.Windows;

public class BuffColorPickerWindow : Window, IDisposable
{
    private Configuration Configuration;
    private RaidBuffTrackerPlugin raidBuffTrackerPlugin;
    private ImmutableSortedSet<UIColor> colors;

    public BuffColorPickerWindow(RaidBuffTrackerPlugin raidBuffTrackerPlugin, ExcelSheet<UIColor>? uiColorExcel) : base(
        "Buff Color Picker", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize |
                                    ImGuiWindowFlags.NoScrollWithMouse)
    {

        this.raidBuffTrackerPlugin = raidBuffTrackerPlugin;

        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(320, 360),
            MaximumSize = new Vector2(320, 360)
        };

        this.SizeCondition = ImGuiCond.Always;

        this.Configuration = raidBuffTrackerPlugin.Configuration;
        if (uiColorExcel != null)
        {
            // Hardly anyone is going to notice this feature, but I'm happy that its here.
            /*
             * A haiku:
             * non-natural sort,
             * Hacky Implementation
             * Colors are complex.
             */
            colors = uiColorExcel
                .ToImmutableSortedSet(Comparer<UIColor>.Create((c1, c2) =>
                {
                    var c1Bytes = BitConverter.GetBytes(c1.UIForeground);
                    var r1 = (float)c1Bytes[3] / 255;
                    var g1 = (float)c1Bytes[2] / 255;
                    var b1 = (float)c1Bytes[1] / 255;
                    var val1 = Math.Acos(DegreesToRadians((r1 - g1 / 2 - b1 / 2) /
                                                          Math.Sqrt(Math.Pow(r1, 2) + Math.Pow(g1, 2) +
                                                                    Math.Pow(b1, 2) - (r1 * g1) - (r1 * b1) -
                                                                    (g1 * b1))));
                    var h1 = (g1 >= b1)
                                 ? val1
                                 : 6.28319 - val1;
                    var c2Bytes = BitConverter.GetBytes(c2.UIForeground);
                    var r2 = (float)c2Bytes[3] / 255;
                    var g2 = (float)c2Bytes[2] / 255;
                    var b2 = (float)c2Bytes[1] / 255;
                    var val2 = Math.Acos(DegreesToRadians((r2 - g2 / 2 - b2 / 2) /
                                                          Math.Sqrt(Math.Pow(r2, 2) + Math.Pow(g2, 2) +
                                                                    Math.Pow(b2, 2) - (r2 * g2) - (r2 * b2) -
                                                                    (g2 * b2))));
                    var h2 = (g2 >= b2)
                                 ? val2
                                 : 6.28319 - val2;
                    return h1.CompareTo(h2);
                }));
        }
    }

    double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
    public override void Draw()
    {
        float size = ImGui.CalcTextSize("Test Color").X + ImGui.GetStyle().FramePadding.X * 2.0f;
        float avail = ImGui.GetContentRegionAvail().X;

        float off = (avail - size) * 0.5f;
        if (off > 0.0f)
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + off);
        if (ImGui.Button("Test Color"))
        {
            SeStringBuilder builder = new SeStringBuilder();

            builder.Append("Player used ");

            builder.AddUiForeground((ushort)Configuration.BuffColor);
            builder.AddText("Test ");
            builder.AddUiForegroundOff();
            Service.ChatGui.Print(builder.Build());
        }
        ImGui.NewLine();
        if (raidBuffTrackerPlugin.UiColors != null)
        {
            for (var index = 0; index < colors.Count; index++)
            {
                var z = colors[index];
                var temp = BitConverter.GetBytes(z.UIForeground);
                var x = (float)temp[3] / 255;
                var y = (float)temp[2] / 255;
                var zz = (float)temp[1] / 255;
                if (x + y + zz == 0)
                {
                    continue;
                }

                if (index % 10 != 0) //hacky but it works
                {
                    ImGui.SameLine();
                }

                if (ImGui.ColorButton(z.RowId.ToString(), new Vector4(
                                          (float)temp[3] / 255,
                                          (float)temp[2] / 255,
                                          (float)temp[1] / 255,
                                          (float)temp[0] / 255)))
                {
                    Configuration.BuffColor = z.RowId;
                    Configuration.Save();
                }
            }
        }
    }
    public void Dispose() { }

}

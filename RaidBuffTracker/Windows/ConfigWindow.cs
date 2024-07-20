using System;
using System.Numerics;
using Dalamud.Game.Text;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using RaidBuffTracker.Toolbox;

namespace RaidBuffTracker.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;
    private RaidBuffTrackerPlugin raidBuffTrackerPlugin;
    public ConfigWindow(RaidBuffTrackerPlugin raidBuffTrackerPlugin) : base(
        "RaidBuffTracker Configuration")
    {
        this.raidBuffTrackerPlugin = raidBuffTrackerPlugin;

        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(500, 425),
            MaximumSize = new Vector2(800, 1000)
        };

        this.SizeCondition = ImGuiCond.Always;

        this.Configuration = raidBuffTrackerPlugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var buffColorCheckbox = this.Configuration.BuffColorCheckbox;
        var mitigationColorCheckbox = this.Configuration.MitigationColorCheckbox;
        var combatTimestamp = Configuration.CombatTimestamp;
        var chatType = this.Configuration.ChatType;
        var Mitigation = this.Configuration.Mitigation; 
        
        if (ImGui.Checkbox("Track Mitigation", ref Mitigation))
        {
            this.Configuration.Mitigation = Mitigation;
            this.Configuration.Save();
        }
        var timerColor = BitConverter.GetBytes(raidBuffTrackerPlugin.UiColors.GetRow(Configuration.CombatTimerColor).UIForeground);
        var x = (float)timerColor[3] / 255;
        var y = (float)timerColor[2] / 255;
        var z = (float)timerColor[1] / 255;
        var sat = (float)timerColor[0] / 255;
        if (ImGui.Checkbox("Show Combat Timestamp", ref combatTimestamp))
        {
            this.Configuration.CombatTimestamp = combatTimestamp;
            this.Configuration.Save();
        }
        
        ImGui.SameLine();
        if (ImGui.ColorButton("Timestamp Color Picker", new Vector4(x,y,z,sat)))
        {
            this.raidBuffTrackerPlugin.DrawTimerColorPickerUI();
        }
        
        var buffColor = BitConverter.GetBytes(raidBuffTrackerPlugin.UiColors.GetRow(Configuration.BuffColor).UIForeground);
        x = (float)buffColor[3] / 255;
        y = (float)buffColor[2] / 255;
        z = (float)buffColor[1] / 255;
        sat = (float)buffColor[0] / 255;
        if (ImGui.Checkbox("Buff Color", ref buffColorCheckbox))
        {
            this.Configuration.BuffColorCheckbox = buffColorCheckbox;
            this.Configuration.Save();
        }
        ImGui.SameLine();
        if (ImGui.ColorButton("Buff Color Picker", new Vector4(x, y, z, sat)))
        {
            this.raidBuffTrackerPlugin.DrawBuffColorPickerUI();
        }

        var mitigationColor = BitConverter.GetBytes(raidBuffTrackerPlugin.UiColors.GetRow(Configuration.MitigationColor).UIForeground);
        x = (float)mitigationColor[3] / 255;
        y = (float)mitigationColor[2] / 255;
        z = (float)mitigationColor[1] / 255;
        sat = (float)mitigationColor[0] / 255;
        if (ImGui.Checkbox("Mitigation Color", ref mitigationColorCheckbox))
        {
            this.Configuration.MitigationColorCheckbox = mitigationColorCheckbox;
            this.Configuration.Save();
        }
        ImGui.SameLine();
        if (ImGui.ColorButton("Mitigation Color Picker", new Vector4(x, y, z, sat)))
        {
            this.raidBuffTrackerPlugin.DrawMitigationColorPickerUI();
        }

        ImGui.SetNextItemWidth(ImGui.CalcTextSize("NPCDialogueAnnouncements").X + 30f ); //hacky but it works
        XivChatType[] types = Enum.GetValues<XivChatType>();
        if (ImGui.BeginCombo("Chat Output Type", chatType.ToString()))
        {
            for (int n = 0; n < types.Length; n++)
            {
                bool selected = chatType.ToString() == types[n].ToString();
                if (ImGui.Selectable(types[n].ToString(), selected))
                {
                    chatType = types[n];
                    Configuration.ChatType = types[n];
                    Configuration.Save();
                }

                if (selected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }
            ImGui.EndCombo();
        }
        
        ImGui.SameLine();
        if (ImGui.Button("Reset to Default"))
        {
            Configuration.ChatType = Service.DalamudPluginInterface.GeneralChatType;
            Configuration.Save();
        }    
    }
}

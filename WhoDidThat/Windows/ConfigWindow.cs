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
    private RaidBuffTrackerPlugin whoDidThatPlugin;
    public ConfigWindow(RaidBuffTrackerPlugin whoDidThatPlugin) : base(
        "RaidBuffTracker Configuration")
    {
        this.whoDidThatPlugin = whoDidThatPlugin;

        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(500, 425),
            MaximumSize = new Vector2(800, 1000)
        };

        this.SizeCondition = ImGuiCond.Always;

        this.Configuration = whoDidThatPlugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var applyStatusEffect = this.Configuration.StatusEffects;
        var buffColorCheckbox = this.Configuration.BuffColorCheckbox;
        var combatTimestamp = Configuration.CombatTimestamp;
        var chatType = this.Configuration.ChatType;
        var multiTarget = this.Configuration.MultiTarget;
        var targetNpc = this.Configuration.TargetNpc;
        var Mitigation = this.Configuration.Mitigation; 
        var outsideParty = this.Configuration.LogOutsideParty;
        
        var exemptRescueEsuna = this.Configuration.ShouldExemptRoleActions;
        
        if (ImGui.Checkbox("Status Application", ref applyStatusEffect))
        {
            this.Configuration.StatusEffects = applyStatusEffect;
            this.Configuration.Save();
        }
        ImGui.Indent();
        ImGui.Unindent();

        if (ImGui.Checkbox("Multi-target Abilities", ref multiTarget))
        {
            this.Configuration.MultiTarget = multiTarget;
            this.Configuration.Save();
        }
        
        ImGui.Indent();
        ImGui.Unindent();
        
        if (ImGui.Checkbox("Players outside your Party", ref outsideParty)) 
        {
            this.Configuration.LogOutsideParty = outsideParty;
            this.Configuration.Save();
        }
        
        ImGui.Indent();
        ImGui.TextWrapped("Do not log actions of jobs with only one player.");
        ImGui.Unindent();

        if (ImGui.Checkbox("Abilities with an NPC Target", ref targetNpc))
        {
            this.Configuration.TargetNpc = targetNpc;
            this.Configuration.Save();
        }
        if (Configuration.TargetNpc)
        {
            ImGui.Separator();
            if (ImGui.Checkbox("Track Mitigation", ref Mitigation))
            {
                this.Configuration.Mitigation = Mitigation;
                this.Configuration.Save();
            }

            ImGui.Separator();
        }
        
        ImGui.NewLine();
        var timerColor = BitConverter.GetBytes(whoDidThatPlugin.UiColors.GetRow(Configuration.CombatTimerColor).UIForeground);
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
            this.whoDidThatPlugin.DrawTimerColorPickerUI();
        }
        
        var temp = BitConverter.GetBytes(whoDidThatPlugin.UiColors.GetRow(Configuration.BuffColor).UIForeground);
        x = (float)temp[3] / 255;
        y = (float)temp[2] / 255;
        z = (float)temp[1] / 255;
        sat = (float)temp[0] / 255;
        if (ImGui.Checkbox("Buff Color", ref buffColorCheckbox))
        {
            this.Configuration.BuffColorCheckbox = buffColorCheckbox;
            this.Configuration.Save();
        }
        ImGui.SameLine();
        if (ImGui.ColorButton("Prefix Color Picker", new Vector4(x,y,z,sat)))
        {
            this.whoDidThatPlugin.DrawColorPickerUI();
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
        
        ImGui.NewLine();
        ImGui.Separator();
        
        if (ImGui.Button("Open Debug Menu"))
        {
            this.whoDidThatPlugin.DrawDebugUI();
        }
        
        
        
    }
}

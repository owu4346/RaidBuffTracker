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
        var heal = this.Configuration.Healing;
        var textTag = this.Configuration.TextTag;
        var combatTimestamp = Configuration.CombatTimestamp;
        var chatType = this.Configuration.ChatType;
        var multiTarget = this.Configuration.MultiTarget;
        var targetNpc = this.Configuration.TargetNpc;
        var targetedMit = this.Configuration.TargetedMit; 
        var targetedDebuffs = this.Configuration.TargetedDebuffs;
        var singleJob = this.Configuration.FilterUniqueJobs;
        var outsideParty = this.Configuration.LogOutsideParty;
        
        var filterRole = this.Configuration.ShouldFilterRoles;
        var exemptRescueEsuna = this.Configuration.ShouldExemptRoleActions;
        
        var filterTank = this.Configuration.FilterTank;
        var filterMelee = this.Configuration.FilterMelee;
        var filterHealer = this.Configuration.FilterHealer;
        var filterRanged = this.Configuration.FilterRanged;
        var filterCasters = this.Configuration.FilterCasters;
        
        if (ImGui.Checkbox("Status Application", ref applyStatusEffect))
        {
            this.Configuration.StatusEffects = applyStatusEffect;
            this.Configuration.Save();
        }
        
        if (ImGui.Checkbox("Healing Actions", ref heal))
        {
            this.Configuration.Healing = heal;
            this.Configuration.Save();
        }
        ImGui.Indent();
        ImGui.TextWrapped("Important!" +
                          "\nIf \"Healing Actions\" is disabled, Actions that grant a status *and* a heal will not be tracked. " +
                          "This affects Medica II, E. Diag, Adloquium, etc.");
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
        
        if (ImGui.Checkbox("Filter Unique Jobs", ref singleJob)) 
        {
            this.Configuration.FilterUniqueJobs = singleJob;
            this.Configuration.Save();
        }
        ImGui.Indent();
        ImGui.TextWrapped("Do not log actions of jobs with only one player.");
        ImGui.Unindent();

        if (Configuration.FilterUniqueJobs)
        {
            ImGui.Indent();
            if (ImGui.Checkbox("Exempt Role actions from this filtration", ref exemptRescueEsuna))
            {
                this.Configuration.ShouldExemptRoleActions = exemptRescueEsuna;
                this.Configuration.Save();
            }
        
            ImGui.Indent();
            ImGui.TextWrapped("(Rescue, Esuna, and shirk will still be logged even if the party has 2 different healers/tanks.)");
            ImGui.Unindent();
            ImGui.Unindent();
        }
        if (ImGui.Checkbox("Abilities with an NPC Target", ref targetNpc))
        {
            this.Configuration.TargetNpc = targetNpc;
            this.Configuration.Save();
        }
        if (Configuration.TargetNpc)
        {
            ImGui.Separator();
            if (ImGui.Checkbox("Track Mitigation", ref targetedMit))
            {
                this.Configuration.TargetedMit = targetedMit;
                this.Configuration.Save();
            }
            
            if (ImGui.Checkbox("Track Debuffs", ref targetedDebuffs))
            {
                this.Configuration.TargetedDebuffs = targetedDebuffs;
                this.Configuration.Save();
            }

            ImGui.Separator();
        }
        if (ImGui.Checkbox("Filter Roles", ref filterRole))
        {
            this.Configuration.ShouldFilterRoles = filterRole;
            this.Configuration.Save();
        }
        

        if (Configuration.ShouldFilterRoles)
        {
            ImGui.Separator();
            if (ImGui.Checkbox("Filter Tanks", ref filterTank))
            {
                this.Configuration.FilterTank = filterTank;
                this.Configuration.Save();
            }
            if (ImGui.Checkbox("Filter Healers", ref filterHealer))
            {
                this.Configuration.FilterHealer = filterHealer;
                this.Configuration.Save();
            }
            if (ImGui.Checkbox("Filter Melee", ref filterMelee))
            {
                this.Configuration.FilterMelee = filterMelee;
                this.Configuration.Save();
            }
            if (ImGui.Checkbox("Filter Ranged", ref filterRanged))
            {
                this.Configuration.FilterRanged = filterRanged;
                this.Configuration.Save();
            }
            
            if (ImGui.Checkbox("Filter Casters", ref filterCasters))
            {
                this.Configuration.FilterCasters = filterCasters;
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
        
        var temp = BitConverter.GetBytes(whoDidThatPlugin.UiColors.GetRow(Configuration.PrefixColor).UIForeground);
        x = (float)temp[3] / 255;
        y = (float)temp[2] / 255;
        z = (float)temp[1] / 255;
        sat = (float)temp[0] / 255;
        if (ImGui.Checkbox("Ability Color", ref textTag))
        {
            this.Configuration.TextTag = textTag;
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

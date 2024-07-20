using System;
using System.Diagnostics;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
using Lumina.Excel.GeneratedSheets;

namespace RaidBuffTracker.Toolbox;

public class Checks
{
    private readonly RaidBuffTrackerPlugin plugin;
    private readonly Tools tools;

    public Checks(RaidBuffTrackerPlugin plugin) {
        this.plugin = plugin;
        tools = new Tools(plugin);
        
    }
    internal unsafe bool CheckLogNPCTarget(ulong sourceId, ActionEffect* effectArray, uint actionId, int[] mitigationNpcTarget, int[] debuffActionsWithNpcTarget)
    {
        if ((Service.ClientState.LocalPlayer.StatusFlags & StatusFlags.InCombat) == 0)
        {
            return false;
        }

        if (plugin.Configuration.Mitigation && mitigationNpcTarget.Contains((int)actionId)) 
        {
            return true;   
        }

        if (debuffActionsWithNpcTarget.Contains((int)actionId)) 
        {
            return true;   
        }

        bool isInParty = Service.PartyList.Any();
        bool actorInParty = Service.PartyList.Count(member =>
        {
            return member.ObjectId == sourceId;
        }) > 0;
                        
        if (isInParty)
        {
            if (!plugin.Configuration.LogOutsideParty && !actorInParty)
            {
                return false;
            }
        }
        return false;
    }
}

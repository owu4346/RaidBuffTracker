using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Common.Lua;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace RaidBuffTracker.Toolbox;

public class ActionLogger
{
    private readonly RaidBuffTrackerPlugin plugin;
    private Dictionary<string, List<(string, int)>> actionLog;
    public ActionLogger(RaidBuffTrackerPlugin plugin) {
        this.plugin = plugin;
        actionLog = new Dictionary<string, List<(string, int)>>();
    }

    internal void LogAction(uint actionId, ulong sourceId, bool isRaidBuff)
    {
        Action? action = null;
        string? source = null;
        IGameObject? gameObject = null;
        
        action ??= Service.DataManager.Excel.GetSheet<Action>()?.GetRow(actionId);
        gameObject ??= Service.ObjectTable.SearchById(sourceId); 
        source ??= gameObject?.Name.ToString();
                        
        string actionName = action!.Name.RawString;
                        
        if(actionLog.ContainsKey(source)) //If the player already exists in the dictionary
        {
            if(IsUniqueAbility(source, actionName))  //If the used ability does not exist in that player's list
            {
                List<(string, int)> existingList = actionLog[source];
                existingList.Add((actionName, 1));
                actionLog[source] = existingList;
            } else //The used ability exists in that player's list, we need to increment it by 1
            {
                if (actionLog.TryGetValue(source, out var list))
                {
                    var abilityTuple = list.FirstOrDefault(tuple => tuple.Item1 == actionName);

                    if (abilityTuple != default)
                    {
                        var updatedTuple = (abilityTuple.Item1, abilityTuple.Item2 + 1);

                        int index = list.FindIndex(tuple => tuple.Item1 == actionName);
                        list[index] = updatedTuple;

                        actionLog[source] = list;
                    }
                }
            }
        } else //The player doesn't already exist, we need to add the player and the action
        {
            actionLog[source] = new List<(string, int)> { (actionName, 1) };
        }

        SendActionToChat(source ?? "Unknown Source", actionName, isRaidBuff);
    }
    internal string ActionLogToString()
    {
        StringBuilder sb = new StringBuilder();

        foreach (var kvp in actionLog)
        {
            sb.AppendLine($"Key: {kvp.Key}");

            foreach (var tuple in kvp.Value)
            {
                sb.AppendLine($"  Ability: {tuple.Item1}, Value: {tuple.Item2}");
            }
        }

        return sb.ToString();
    }
    internal bool IsUniqueAbility(string name, string ability)
    {
        if (actionLog.ContainsKey(name))
        {
            var list = actionLog[name];
            foreach (var tuple in list)
            {
                if (tuple.Item1 == ability)
                {
                    return false; // Ability already exists
                }
            }
            return true; // Ability doesn't already exist
        }
        else
        {
            return true; // Cannot find name
        }
    }

    private void SendActionToChat(string source, string actionName, bool isRaidBuff)
    {
        //right now this seems fine but in the future messageTag may become mandatory/very useful - change impl of the timer display?
        SeStringBuilder builder = new SeStringBuilder();
        builder.AddText(ActionLogToString());

        if (plugin.Configuration.CombatTimestamp && plugin.CombatTimer.inCombat())
        {
            builder.AddUiForeground((ushort) plugin.Configuration.CombatTimerColor); //cast to short because ???
            builder.AddText(plugin.CombatTimer.getCurrentCombatTime() + " ");
            builder.AddUiForegroundOff(); 
        }

        builder.Append(source + " used ");

        if(isRaidBuff)
        {
            builder.AddUiForeground((ushort)plugin.Configuration.BuffColor); //cast to short because ???
        } else
        {
            builder.AddUiForeground((ushort)plugin.Configuration.MitigationColor); //cast to short because ???
        }
        builder.AddText(actionName);
        builder.AddUiForegroundOff();
        builder.AddUiForeground((ushort)plugin.Configuration.BuffColor); //cast to short because ???

        Service.ChatGui.Print(new XivChatEntry()
        {
            Message = builder.Build(),
            
            Type = plugin.Configuration.ChatType 
        });
    }
}

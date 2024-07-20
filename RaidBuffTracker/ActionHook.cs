/*
 * Main Structure attributed to Kouzukii/ffxiv-deathrecap
 * https://github.com/Kouzukii/ffxiv-deathrecap/blob/master/Events/CombatEventCapture.cs
 */

using System;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using Lumina.Excel.GeneratedSheets;
using RaidBuffTracker.Toolbox;
using Action = Lumina.Excel.GeneratedSheets.Action;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;

namespace RaidBuffTracker
{

    public class ActionHook : IDisposable
    {
        private readonly RaidBuffTrackerPlugin plugin;
        private readonly ActionLogger actionLogger;

        public ActionHook(RaidBuffTrackerPlugin plugin) {
            this.plugin = plugin;
            actionLogger = new ActionLogger(plugin);
            Service.GameInteropProvider.InitializeFromAttributes(this);
            receiveAbilityEffectHook.Enable();
        }

        [Signature("40 55 56 57 41 54 41 55 41 56 48 8D AC 24 68 FF FF FF 48 81 EC 98 01 00 00",
                   DetourName = nameof(ReceiveAbilityEffectDetour))]
        private readonly Hook<ReceiveAbilityDelegate> receiveAbilityEffectHook = null!;

        private unsafe delegate void ReceiveAbilityDelegate(
            int sourceId, IntPtr sourceCharacter, IntPtr pos, ActionEffectHeader* effectHeader,
            ActionEffect* effectArray, ulong* effectTrail);


        private unsafe void ReceiveAbilityEffectDetour(
            int sourceId, IntPtr sourceCharacter, IntPtr pos, ActionEffectHeader* effectHeader,
            ActionEffect* effectArray, ulong* effectTrail)
        {
            try
            {
                if (!plugin.Configuration.Enabled)
                {
                    return;
                }

                if (Service.ClientState.IsPvP)
                {
                    return;
                }

                uint targets = effectHeader->EffectCount;

                uint actionId = effectHeader->EffectDisplayType switch
                {
                    ActionEffectDisplayType.MountName => 0xD000000 + effectHeader->ActionId,
                    ActionEffectDisplayType.ShowItemName => 0x2000000 + effectHeader->ActionId,
                    _ => effectHeader->ActionAnimationId
                };

                ulong gameObjectID = Service.ObjectTable.SearchById((uint)sourceId).GameObjectId;

                for (var i = 0; i < targets; i++)
                {
                    var actionTargetId = (uint)(effectTrail[i] & uint.MaxValue);
                }

                receiveAbilityEffectHook.Original(sourceId, sourceCharacter, pos, effectHeader, effectArray, effectTrail);

                bool actionIsTargetingNpc = plugin.Configuration.debuffActionsWithNpcTarget.Contains((int)actionId) ||
                                            plugin.Configuration.mitigationNpcTarget.Contains((int)actionId);

                bool raidBuff = plugin.Configuration.raidBuffs.Contains<int>((int)actionId);

                bool isMitigationParty = plugin.Configuration.mitigationParty.Contains<int>((int)actionId);

                bool shouldLogAction = false;
                bool isRaidBuff = false;  //false for mit, true for raidbuff
                if (actionIsTargetingNpc)
                {
                    int result = CheckLogNPCTarget(gameObjectID, effectArray, actionId, plugin.Configuration.mitigationNpcTarget, plugin.Configuration.debuffActionsWithNpcTarget);
                    if (result == 1) {
                        shouldLogAction = true;
                    } else if (result == 2)
                    {
                        shouldLogAction = true;
                        isRaidBuff = true;
                    }
                }
                else if (raidBuff)
                {
                    if ((Service.ClientState.LocalPlayer.StatusFlags & StatusFlags.InCombat) != 0)
                    {
                        shouldLogAction = true;
                        isRaidBuff = true;
                    }
                } else if (isMitigationParty)
                {
                    if (plugin.Configuration.Mitigation && (Service.ClientState.LocalPlayer.StatusFlags & StatusFlags.InCombat) != 0)
                    {
                        shouldLogAction = true;
                    }
                }

                if (shouldLogAction)
                {
                    actionLogger.LogAction(actionId, gameObjectID, isRaidBuff);
                }
            }
            catch (Exception e)
            {
                Service.PluginLog.Error(e, "oops!");
            }
        }

        internal unsafe int CheckLogNPCTarget(ulong sourceId, ActionEffect* effectArray, uint actionId, int[] mitigationNpcTarget, int[] debuffActionsWithNpcTarget)
        {
            if ((Service.ClientState.LocalPlayer.StatusFlags & StatusFlags.InCombat) == 0)
            {
                return 0;
            }

            if (plugin.Configuration.Mitigation && mitigationNpcTarget.Contains((int)actionId))
            {
                return 1;
            }

            if (debuffActionsWithNpcTarget.Contains((int)actionId))
            {
                return 2;
            }

            bool isInParty = Service.PartyList.Any();
            bool actorInParty = Service.PartyList.Count(member =>
            {
                return member.ObjectId == sourceId;
            }) > 0;

            if (isInParty && !actorInParty)
            {
                return 0;
            }
            return 0;
        }
        public void Dispose()
        {
            receiveAbilityEffectHook.Disable();
            receiveAbilityEffectHook.Dispose();
        }
    }
}

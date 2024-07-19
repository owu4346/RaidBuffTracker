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
        private readonly Checks checks;
        private readonly ActionLogger actionLogger;

        public ActionHook(RaidBuffTrackerPlugin plugin) {
            this.plugin = plugin;
            checks = new Checks(plugin);
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
                    bool targetNotInParty = Service.PartyList.Count(p => { return p.ObjectId == actionTargetId; }) == 0;
                    if (plugin.Configuration.Verbose)
                    {
                        if (actionId == 7)
                        {
                            continue;
                        }
                        Service.PluginLog.Information("S:" + sourceId + " GOID: " + gameObjectID  +  "|A: " + actionId + "|T: " + actionTargetId +
                                                      "|AN:" + Service.DataManager.Excel.GetSheet<Action>()
                                                                      ?.GetRow(actionId)?
                                                                      .Name.RawString);
                        for (var j = 0; j < 8; j++)
                        {
                            ref var actionEffect = ref effectArray[i * 8 + j];
                            if (actionEffect.EffectType == 0)
                            {
                                continue;
                            }

                            Service.PluginLog.Information("E:" + actionEffect.EffectType);

                        }
                    }
                }



                /*
                  Role Actions:
                     provoke: 7533
                 */
                receiveAbilityEffectHook.Original(sourceId, sourceCharacter, pos, effectHeader, effectArray, effectTrail);

                int[] raidBuffs =
                        {(int)ClassJobActions.Divination, (int)ClassJobActions.Brotherhood, 
                         (int)ClassJobActions.ArcaneCircle, (int)ClassJobActions.BattleLitany, 
                         (int)ClassJobActions.Embolden, (int)ClassJobActions.SearingLight,
                         (int)ClassJobActions.StarryMuse,  (int)ClassJobActions.TechnicalFinish,
                         (int)ClassJobActions.SingleTechnicalFinish, (int)ClassJobActions.DoubleTechnicalFinish,
                         (int)ClassJobActions.TripleTechnicalFinish, (int)ClassJobActions.Devilment, 
                         (int)ClassJobActions.BattleVoice, (int)ClassJobActions.RadiantFinale};
                int[] debuffActionsWithNpcTarget = 
                {
                    (int)ClassJobActions.ChainStrategem, (int)ClassJobActions.Mug,
                    (int)ClassJobActions.Dokumori,
                };
                int[] mitigationNpcTarget = new[]
                {
                    (int)ClassJobActions.Addle, (int)ClassJobActions.Feint,
                    (int)ClassJobActions.Reprisal, (int)ClassJobActions.Dismantle
                };
                int[] mitigationParty = new[]
                {
                    (int)ClassJobActions.ShakeItOff, (int)ClassJobActions.DivineVeil,
                    (int)ClassJobActions.HeartofLight, (int)ClassJobActions.DarkMissionary,
                    (int)ClassJobActions.Mantra, (int)ClassJobActions.NaturesMinne,
                    (int)ClassJobActions.MagickBarrier, (int)ClassJobActions.TemperaGrassa,
                    (int)ClassJobActions.Troubadour, (int)ClassJobActions.Tactician,
                    (int)ClassJobActions.ShieldSamba,
                };
                bool actionIsTargetingNpc = debuffActionsWithNpcTarget.Contains((int)actionId) ||
                                            mitigationNpcTarget.Contains((int)actionId);

                bool raidBuff = raidBuffs.Contains<int>((int)actionId);

                bool isMitigationParty = mitigationParty.Contains<int>((int)actionId);

                bool shouldLogAction = false;
                if (actionIsTargetingNpc)
                {
                    shouldLogAction = checks.CheckLogNPCTarget(gameObjectID, effectArray, actionId, mitigationNpcTarget, debuffActionsWithNpcTarget);
                }
                else if (raidBuff)
                {
                    if ((Service.ClientState.LocalPlayer.StatusFlags & StatusFlags.InCombat) != 0)
                    {
                        shouldLogAction = true;
                    }
                } else if (isMitigationParty)
                {
                    if ((Service.ClientState.LocalPlayer.StatusFlags & StatusFlags.InCombat) != 0)
                    {
                        shouldLogAction = true;
                    }
                }
                    
                if (shouldLogAction)
                {
                    actionLogger.LogAction(actionId, gameObjectID);
                }
            }
            catch (Exception e)
            {
                Service.PluginLog.Error(e, "oops!");
            }
        }

        public void Dispose()
        {
            receiveAbilityEffectHook.Disable();
            receiveAbilityEffectHook.Dispose();
        }
    }
}

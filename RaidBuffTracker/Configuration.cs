using System;
using Dalamud.Configuration;
using Dalamud.Game.Text;
using Dalamud.Plugin;
using RaidBuffTracker.Toolbox;


namespace RaidBuffTracker
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public bool Enabled { get; set; } = true;
        public bool Mitigation { get; set; } = true;
        public bool BuffColorCheckbox { get; set; } = true;
        public uint BuffColor { get; set; } = 10;

        public bool CombatTimestamp { get; set; } = false;
        public bool ShouldExemptRoleActions { get; set; } = true;
        public bool MitigationColorCheckbox { get; set; } = true;
        public uint MitigationColor { get; set; } = 10;
        public uint CombatTimerColor { get; set; } = 10;

        public int[] raidBuffs = {(int)ClassJobActions.Divination, (int)ClassJobActions.Brotherhood,
                                  (int)ClassJobActions.ArcaneCircle, (int)ClassJobActions.BattleLitany,
                                  (int)ClassJobActions.Embolden, (int)ClassJobActions.SearingLight,
                                  (int)ClassJobActions.StarryMuse, (int)ClassJobActions.TechnicalFinish,
                                  (int)ClassJobActions.SingleTechnicalFinish, (int)ClassJobActions.DoubleTechnicalFinish,
                                  (int)ClassJobActions.TripleTechnicalFinish, (int)ClassJobActions.QuadtripleTechnicalFinish,
                                  (int)ClassJobActions.Devilment, (int)ClassJobActions.BattleVoice, (int)ClassJobActions.RadiantFinale};
        public int[] debuffActionsWithNpcTarget = {(int)ClassJobActions.ChainStrategem, (int)ClassJobActions.Mug,
                                                   (int)ClassJobActions.Dokumori};
        public int[] mitigationNpcTarget = {(int)ClassJobActions.Addle, (int)ClassJobActions.Feint,
                                            (int)ClassJobActions.Reprisal, (int)ClassJobActions.Dismantle};
        public int[] mitigationParty = {(int)ClassJobActions.ShakeItOff, (int)ClassJobActions.DivineVeil,
                                        (int)ClassJobActions.HeartofLight, (int)ClassJobActions.DarkMissionary,
                                        (int)ClassJobActions.Mantra, (int)ClassJobActions.NaturesMinne,
                                        (int)ClassJobActions.MagickBarrier, (int)ClassJobActions.TemperaGrassa,
                                        (int)ClassJobActions.Troubadour, (int)ClassJobActions.Tactician,
                                        (int)ClassJobActions.ShieldSamba};

        public bool resetLog = false;
        public XivChatType ChatType { get; set; } = XivChatType.Debug;
        
        [NonSerialized]
        private IDalamudPluginInterface? PluginInterface;

        public void Initialize(IDalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.PluginInterface!.SavePluginConfig(this);
        }
    }
}

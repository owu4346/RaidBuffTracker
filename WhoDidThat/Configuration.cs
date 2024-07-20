using System;
using Dalamud.Configuration;
using Dalamud.Game.Text;
using Dalamud.Plugin;


namespace RaidBuffTracker
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public bool Enabled { get; set; } = true;
        public bool StatusEffects { get; set; } = true;
        public bool Mitigation { get; set; } = true;
        public bool BuffColorCheckbox { get; set; } = true;
        public bool CombatTimestamp { get; set; } = false;
        public bool LogOutsideParty { get; set; } = false;
        public bool ShouldExemptRoleActions { get; set; } = true;
        public uint BuffColor { get; set; } = 10;
        public bool MitColorCheckbox { get; set; } = true;

        public uint MitColor { get; set; } = 10;
        public uint CombatTimerColor { get; set; } = 10;
        public XivChatType ChatType { get; set; } = XivChatType.Debug;
        
        public bool SelfLog { get; set; } = false;
        public bool Verbose { get; set; } = false;
        public bool OnlyLogPlayerCharacters { get; set; } = true; //Battle NPC applied buffs are still found in the battle log so this toggle shouldn't be an issue

        

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

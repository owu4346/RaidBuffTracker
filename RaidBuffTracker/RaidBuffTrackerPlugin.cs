using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using RaidBuffTracker.Timer;
using RaidBuffTracker.Toolbox;
using RaidBuffTracker.Windows;

namespace RaidBuffTracker
{
    //todo: Granular filtering
    public sealed class RaidBuffTrackerPlugin : IDalamudPlugin
    {
        public string Name => "RaidBuffTracker";
        private const string CommandName = "/rbt";
        private const string CommandConfigName = "/rbtc";

        private IDalamudPluginInterface PluginInterface { get; init; }
        public Configuration Configuration { get; init; }
        public ActionHook ActionHook { get; }
        public WindowSystem WindowSystem = new("RaidBuffTracker");

        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }
        private BuffColorPickerWindow BuffColorPickerWindow { get; init; }
        private MitigationColorPickerWindow MitigationColorPickerWindow { get; init; }

        private TimerColorPickerWindow TimerColorPickerWindow { get; init; }
        public CombatTimer CombatTimer { get; init; }
        
        public ExcelSheet<UIColor>? UiColors { get; init; }

        public RaidBuffTrackerPlugin(
            IDalamudPluginInterface pluginInterface)
        {
            Service.Initialize(pluginInterface);

            UiColors = Service.DataManager.Excel.GetSheet<UIColor>();
            this.PluginInterface = pluginInterface;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);
            ActionHook = new ActionHook(this);
            
            ConfigWindow = new ConfigWindow(this);
            MainWindow = new MainWindow(this);
            BuffColorPickerWindow = new BuffColorPickerWindow(this, Service.DataManager.Excel.GetSheet<UIColor>());
            MitigationColorPickerWindow = new MitigationColorPickerWindow(this, Service.DataManager.Excel.GetSheet<UIColor>());
            TimerColorPickerWindow = new TimerColorPickerWindow(this, Service.DataManager.Excel.GetSheet<UIColor>());

            CombatTimer = new CombatTimer(this);
            
            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);
            WindowSystem.AddWindow(BuffColorPickerWindow);
            WindowSystem.AddWindow(MitigationColorPickerWindow);
            WindowSystem.AddWindow(TimerColorPickerWindow);

            Service.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Type /rbt to get started."
            });
            
            Service.CommandManager.AddHandler(CommandConfigName, new CommandInfo(OnConfigCommand)
            {
                HelpMessage = "Type /rbtc for the plugin config."
            });
            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            Service.Framework.Update += CombatTimer.onUpdateTimer;
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();
            ActionHook.Dispose();
            ConfigWindow.Dispose();
            MainWindow.Dispose();
            BuffColorPickerWindow.Dispose();
            MitigationColorPickerWindow.Dispose();
            TimerColorPickerWindow.Dispose();
            Service.CommandManager.RemoveHandler(CommandName);
            Service.CommandManager.RemoveHandler(CommandConfigName);
        }

        private void OnCommand(string command, string args)
        {
            MainWindow.IsOpen = true;
        }
        
        private void OnConfigCommand(string command, string args)
        {
            DrawConfigUI();
        }
        
        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            ConfigWindow.IsOpen = true;
        }
        public void DrawBuffColorPickerUI()
        {
            BuffColorPickerWindow.IsOpen = true;
        }
        public void DrawMitigationColorPickerUI()
        {
            MitigationColorPickerWindow.IsOpen = true;
        }
        public void DrawTimerColorPickerUI()
        {
            TimerColorPickerWindow.IsOpen = true;
        }
    }
}

using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace RaidBuffTracker.Windows;

public class MainWindow : Window, IDisposable
{
    private RaidBuffTrackerPlugin raidBuffTrackerPlugin;
    private Configuration Configuration;

    public MainWindow(RaidBuffTrackerPlugin raidBuffTrackerPlugin) : base(
        "RaidBuffTracker", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(450, 200),
            MaximumSize = new Vector2(450, 200)
        };
        this.Configuration = raidBuffTrackerPlugin.Configuration;
        this.raidBuffTrackerPlugin = raidBuffTrackerPlugin;
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        var enabled = this.Configuration.Enabled;

        if (ImGui.Checkbox("Enabled", ref enabled))
        {
            this.Configuration.Enabled = enabled;
            this.Configuration.Save();
        }
        ImGui.Spacing();
        
        if (ImGui.Button("Show Settings"))
        {
            this.raidBuffTrackerPlugin.DrawConfigUI();
        }
        ImGui.Spacing();
        ImGui.Text("Attributions");
        ImGui.BulletText("DeathRecap for critical backend hooking");
        ImGui.BulletText("Mutant Standard for the plugin icon (CC BY-NC-SA) - https://mutant.tech");
        
    }
}

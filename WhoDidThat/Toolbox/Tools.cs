using System.Diagnostics;
using System.Linq;
using Dalamud.Logging;
using Lumina.Excel.GeneratedSheets;

namespace RaidBuffTracker.Toolbox;

public class Tools
{
    private readonly RaidBuffTrackerPlugin plugin;
    

    public Tools(RaidBuffTrackerPlugin plugin)
    {
        this.plugin = plugin;
    }


    //tank = 1
    //healer = 2
    //melee = 3
    //phys = 4
    //caster = 5
    
    //have to use party bonus because of square's indie game code
    
    public static bool twoOrMoreRolePresent(int role)
    {
        bool greaterThan1 = Service.PartyList.Count(p =>
        {
            Debug.Assert(p.ClassJob.GameData != null, "p.ClassJob.GameData != null");
            return p.ClassJob.GameData.PartyBonus == role;
        }) > 1;

        return greaterThan1;
    }
    
    internal unsafe int[] getEffects(int targetIdx, ActionEffect* effectArray)
    {
        var effects = new int[8];
        for (var j = 0; j < 8; j++)
        {
            ref var actionEffect = ref effectArray[targetIdx * 8 + j];
            if (actionEffect.EffectType == 0)
            {
                continue;
            }
                            
            effects[j] = (int) actionEffect.EffectType;
            if (plugin.Configuration.Verbose)
            {
                Service.PluginLog.Information("Effect:" + actionEffect.EffectType);
            }
        }

        return effects;
    }
    
    
    public bool ShouldLogEffects(int[] effectArray)
    {
        Service.PluginLog.Information("Checking log - 9");
        
        if (effectArray.Contains((int)ActionEffectType.ApplyStatusEffectTarget) && plugin.Configuration.StatusEffects)
        {
            return true;
        }
        
        return false;
        
        
    }
}

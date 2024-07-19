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
    public bool ShouldLogRole(byte partyBonus)
    {
        if (!plugin.Configuration.ShouldFilterRoles)
        {
            return true;
        }

        if (plugin.Configuration.FilterTank && partyBonus == 1)
        {
            return false;
        }

        if (plugin.Configuration.FilterMelee && partyBonus == 3)
        {
            return false;
        }

        if (plugin.Configuration.FilterRanged && partyBonus == 4)
        {
            return false;
        }
        
        if (plugin.Configuration.FilterHealer && partyBonus == 2)
        {
            return false;
        }
        if (plugin.Configuration.FilterCasters && partyBonus == 5)
        {
            return false;
        }

        return true;

    }

    internal bool IsDuplicate(ClassJob originJob)
    {
        string originJobName = originJob.Name;
        bool duplicate = Service.PartyList.Count(p =>
        {
            Debug.Assert(p.ClassJob.GameData != null, "p.ClassJob.GameData != null");
            return p.ClassJob.GameData.Name == originJobName;
        }) > 1;

        return duplicate;
    }
    
    public static bool twoOrMoreRolePresent(int role)
    {
        bool greaterThan1 = Service.PartyList.Count(p =>
        {
            Debug.Assert(p.ClassJob.GameData != null, "p.ClassJob.GameData != null");
            return p.ClassJob.GameData.PartyBonus == role;
        }) > 1;

        return greaterThan1;
    }
    
    internal bool twoOrMoreRoleActionUsersPresent(int roleAction)
    {
        switch (roleAction)
        {
            case (int)ClassJobActions.Addle:
                return twoOrMoreRolePresent(5); //caster
            case (int)ClassJobActions.Feint:
                return twoOrMoreRolePresent(3); //melee
            case (int)ClassJobActions.Reprisal:
                return twoOrMoreRolePresent(1); // tank 
        }
        return false;
    }


    internal unsafe bool ShouldLogEffects(uint targets, ulong* effectTrail, ActionEffect* effectArray, ulong localPlayerId)
    {

        for (var i = 0; i < targets; i++)
        {

            var actionTargetId = (uint)(effectTrail[i] & uint.MaxValue);
            if (actionTargetId != localPlayerId) 
            {
                continue;
            }

            var effects = getEffects(i, effectArray);
            return ShouldLogEffects(effects);
        }
        return false;

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

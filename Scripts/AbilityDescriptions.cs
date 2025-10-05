using System.Collections.Generic;

public static class AbilityDescriptions
{
    public static readonly Dictionary<AbilityType, string> Descriptions = new Dictionary<AbilityType, string>()
    {
        { AbilityType.None, "No ability" },

        // --- Old abilities ---
        { AbilityType.BuffFrontHP4, "Increases the health of the nearest ally in front by 4" },
        { AbilityType.BuffBackHP5, "Increases the health of the nearest ally behind by 5" },
        { AbilityType.BuffBackATK3, "Increases the attack of the nearest ally behind by 3" },
        { AbilityType.BuffBackAllATK1, "Increases the attack of all allies behind by 1" },
        { AbilityType.BuffBackAllHP1, "Increases the health of all allies behind by 1" },
        { AbilityType.BuffAllHP2, "Increases the health of all allies by 2" },
        { AbilityType.BuffAllATK1, "Increases the attack of all allies by 1" },

        // --- New abilities ---
        { AbilityType.SelfBuff1HP1ATK, "Increases own health and attack by 1" },
        { AbilityType.Block1Damage, "Blocks 1 point of incoming damage" },
        { AbilityType.DoubleAttack, "Attacks twice per turn" },
        { AbilityType.SummonInFront, "Summons a creature in front of oneself" }
    };

    public static string GetDescription(AbilityType ability)
    {
        if (Descriptions.TryGetValue(ability, out string desc))
            return desc;

        return "No description";
    }
}

using System.Collections.Generic;

public static class AbilityDescriptions
{
    public static readonly Dictionary<AbilityType, string> Descriptions = new Dictionary<AbilityType, string>()
    {
        { AbilityType.None, "Нет способности" },
        { AbilityType.BuffFrontHP4, "Увеличивает здоровье ближайшего союзника спереди на 4" },
        { AbilityType.BuffBackHP5, "Увеличивает здоровье ближайшего союзника сзади на 5" },
        { AbilityType.BuffBackATK3, "Увеличивает атаку ближайшего союзника сзади на 3" },
        { AbilityType.BuffBackAllATK1, "Увеличивает атаку всех союзников сзади на 1" },
        { AbilityType.BuffBackAllHP1, "Увеличивает здоровье всех союзников сзади на 1" },
        { AbilityType.BuffAllHP2, "Увеличивает здоровье всех союзников на 2" },
        { AbilityType.BuffAllATK1, "Увеличивает атаку всех союзников на 1" }
    };
}

using UnityEngine;
using System.Collections.Generic;

public class EnemySlot : MonoBehaviour
{
    public List<CreatureInstance> GetCreatures()
    {
        List<CreatureInstance> list = new List<CreatureInstance>();
        foreach (Transform child in transform)
        {
            CreatureInstance c = child.GetComponent<CreatureInstance>();
            if (c != null && !c.isDead) list.Add(c);
        }
        return list;
    }
}

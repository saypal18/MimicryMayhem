using System;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EntityDistanceActivator
{
    [SerializeField] public bool enableDistanceActivation = false;
    [SerializeField] public int activationDistance = 10;
    
    public void TickActivations(EntitySpawner entitySpawner)
    {
        var activeEntities = entitySpawner.GetActiveEntities();
        if (activeEntities == null || activeEntities.Count == 0) return;

        if (!enableDistanceActivation)
        {
            for (int i = 0; i < activeEntities.Count; i++)
            {
                if (!activeEntities[i].IsActiveForTurns)
                {
                    activeEntities[i].SetActiveForTurns(true);
                }
            }
            return;
        }

        Entity player = null;
        for (int i = 0; i < activeEntities.Count; i++)
        {
            if (activeEntities[i].IsPlayer)
            {
                player = activeEntities[i];
                break;
            }
        }
        
        if (player == null)
        {
            for (int i = 0; i < activeEntities.Count; i++)
            {
                activeEntities[i].SetActiveForTurns(false);
            }
            return;
        }
        
        Vector2Int pPos = player.Position;

        for (int i = 0; i < activeEntities.Count; i++)
        {
            Entity e = activeEntities[i];
            if (e.IsPlayer) continue;

            Vector2Int ePos = e.Position;
            int dist = Mathf.Max(Mathf.Abs(pPos.x - ePos.x), Mathf.Abs(pPos.y - ePos.y));
            
            bool shouldBeActive = dist <= activationDistance;
            e.SetActiveForTurns(shouldBeActive);
        }
    }
}

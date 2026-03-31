using System;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EntityDistanceActivator
{
    [SerializeField] public bool enableDistanceActivation = false;
    [SerializeField] public int activationDistance = 10;
    private Entity cachedPlayer;

    public void ResetEnvironment()
    {
        cachedPlayer = null;
    }

    public void TickActivations(EntitySpawner entitySpawner)
    {
        var activeEntities = entitySpawner.GetActiveEntities();
        if (activeEntities == null || activeEntities.Count == 0) return;

        if (cachedPlayer == null || !cachedPlayer.gameObject.activeInHierarchy || !cachedPlayer.IsPlayer)
        {
            cachedPlayer = null;
            for (int i = 0; i < activeEntities.Count; i++)
            {
                if (activeEntities[i].IsPlayer)
                {
                    cachedPlayer = activeEntities[i];
                    break;
                }
            }
        }

        Entity player = cachedPlayer;

        int playerTier = 0;
        int playerGrip = 0;
        if (player != null && player.equippedItem != null)
        {
            if (player.equippedItem.Get() is WeaponItem weapon)
            {
                playerTier = weapon.tier;
                playerGrip = weapon.currentGrip;
            }
        }

        if (!enableDistanceActivation)
        {
            for (int i = 0; i < activeEntities.Count; i++)
            {
                Entity e = activeEntities[i];
                if (!e.IsActiveForTurns)
                {
                    e.SetActiveForTurns(true);
                }
                if (e.IsPlayer) continue;

                if (e.tierDisplay != null)
                {
                    int enemyTier = 0;
                    int enemyGrip = 0;
                    if (e.equippedItem != null && e.equippedItem.Get() is WeaponItem eWeapon)
                    {
                        enemyTier = eWeapon.tier;
                        enemyGrip = eWeapon.currentGrip;
                    }
                    bool isOneShot = playerTier > 0 && enemyGrip < playerTier;
                    bool isStronger = playerTier > 0 && enemyTier > 0 && playerGrip < enemyTier;
                    e.tierDisplay.SetFeedback(isOneShot, isStronger);
                }
            }
            return;
        }

        if (player == null)
        {
            for (int i = 0; i < activeEntities.Count; i++)
            {
                Entity e = activeEntities[i];
                e.SetActiveForTurns(false);
                if (e.tierDisplay != null) e.tierDisplay.SetFeedback(false, false);
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

            if (e.tierDisplay != null)
            {
                if (shouldBeActive)
                {
                    int enemyTier = 0;
                    int enemyGrip = 0;
                    if (e.equippedItem != null && e.equippedItem.Get() is WeaponItem eWeapon)
                    {
                        enemyTier = eWeapon.tier;
                        enemyGrip = eWeapon.currentGrip;
                    }
                    bool isOneShot = playerTier > 0 && enemyGrip < playerTier;
                    bool isStronger = playerTier > 0 && enemyTier > 0 && playerGrip < enemyTier;
                    e.tierDisplay.SetFeedback(isOneShot, isStronger);
                }
                else
                {
                    e.tierDisplay.SetFeedback(false, false);
                }
            }
        }
    }
}

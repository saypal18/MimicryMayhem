using UnityEngine;
using System.Collections.Generic;
using FMODUnity;
using FMOD.Studio;

public class PlayerTeleporter : MonoBehaviour
{
    public InputManager inputManager;
    public Camera cam;
    public event System.Action<GameInitializer> OnTeleported;
    private DoorTile lastDoorTeleportedTo;

    [Header("Audio")]
    public EventReference areaTransitionSoundEvent;

    public void TeleportIfOnDoor(Entity player, Vector3 visualPosition)
    {
        if (player == null || player.CurrentGrid == null) return;

        Vector2Int gridPos = player.CurrentGrid.GetGridPosition(visualPosition);
        List<GridPlaceable> tile = player.CurrentGrid.GetTile(gridPos);
        bool isOnDoor = false;

        if (tile != null)
        {
            foreach (var placeable in tile)
            {
                if (placeable.Type == GridPlaceable.PlaceableType.Door && placeable.TryGetComponent(out DoorTile door))
                {
                    isOnDoor = true;
                    if (door.targetEnvironment != null)
                    {
                        if (door == lastDoorTeleportedTo) return; // Prevent bouncing back immediately

                        TeleportPlayer(player, door);
                        return;
                    }
                }
            }
        }
        
        if (!isOnDoor)
        {
            // If they are not on any door, clear the last teleported door
            lastDoorTeleportedTo = null;
        }
    }

    private void TeleportPlayer(Entity player, DoorTile sourceDoor)
    {
        GameInitializer oldEnv = sourceDoor.parentEnvironment;
        GameInitializer newEnv = sourceDoor.targetEnvironment;
        Vector2Int dropPos = sourceDoor.targetDoorPosition;

        if (oldEnv != null) oldEnv.entitySpawner.RemoveEntitySafely(player);

        ITick newTick = null;
        if (newEnv != null)
        {
            newTick = newEnv.turnManager.GetTeams()[player.TeamId];
        }

        player.TransferToNewEnvironment(newEnv.grid, dropPos, newTick, newEnv.entitySpawner);

        if (newEnv != null) newEnv.entitySpawner.AddEntitySafely(player);

        if (inputManager != null)
            inputManager.InitializeClickMap(newEnv.grid, player.playerActionHighlighter, cam != null ? cam : Camera.main);

        DoorTile targetDoorTile = null;
        if (newEnv != null && newEnv.grid != null)
        {
            var tileParams = newEnv.grid.GetTile(dropPos);
            if (tileParams != null)
            {
                foreach (var placeable in tileParams)
                {
                    if (placeable.Type == GridPlaceable.PlaceableType.Door && placeable.TryGetComponent(out DoorTile dt))
                    {
                        targetDoorTile = dt;
                        break;
                    }
                }
            }
        }
        
        lastDoorTeleportedTo = targetDoorTile;
        PlayAreaTransitionSound();
        OnTeleported?.Invoke(newEnv);
    }

    private void PlayAreaTransitionSound()
    {
        if (Trainer.IsTraining) return;
        if (SoundManager.CheckEventNull(areaTransitionSoundEvent, "AreaTransition")) return;

        EventInstance instance = RuntimeManager.CreateInstance(areaTransitionSoundEvent);
        instance.start();
        instance.release();
    }
}

using Unity.MLAgents.Sensors;
using UnityEngine;

public class EquippedItemObservation
{
    private EquippedItem equippedItem;

    public EquippedItemObservation(EquippedItem equippedItem)
    {
        this.equippedItem = equippedItem;
    }

    public void CollectObservations(VectorSensor sensor)
    {
        InventoryItem item = equippedItem?.Get();

        float isSword = 0f;
        float isShield = 0f;
        float isBow = 0f;

        if (item != null)
        {
            switch (item.itemType)
            {
                case ItemType.Sword:
                    isSword = 1f;
                    break;
                case ItemType.Shield:
                    isShield = 1f;
                    break;
                case ItemType.Bow:
                    isBow = 1f;
                    break;
            }
        }

        sensor.AddObservation(isSword);
        sensor.AddObservation(isShield);
        sensor.AddObservation(isBow);
    }
}

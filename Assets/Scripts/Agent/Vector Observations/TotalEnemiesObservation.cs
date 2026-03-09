using Unity.MLAgents.Sensors;
using UnityEngine;

public class TotalEnemiesObservation : SensorComponent
{
    private EntitySpawner pendingSpawner;
    private Grid pendingGrid;
    private TotalEnemiesSensor sensor;

    public void SetAgentReferences(EntitySpawner spawner, Grid gridRef)
    {
        if (sensor != null)
        {
            sensor.SetAgentReferences(spawner, gridRef);
        }
        else
        {
            pendingSpawner = spawner;
            pendingGrid = gridRef;
        }
    }

    public override ISensor[] CreateSensors()
    {
        sensor = new TotalEnemiesSensor();
        if (pendingSpawner != null && pendingGrid != null)
        {
            sensor.SetAgentReferences(pendingSpawner, pendingGrid);
            pendingSpawner = null;
            pendingGrid = null;
        }
        return new ISensor[] { sensor };
    }

    public class TotalEnemiesSensor : ISensor
    {
        private EntitySpawner spawner;
        private Grid grid;

        public void SetAgentReferences(EntitySpawner entitySpawner, Grid gridRef)
        {
            spawner = entitySpawner;
            grid = gridRef;
        }

        public string GetName() => "TotalEnemiesObservation";
        public ObservationSpec GetObservationSpec() => ObservationSpec.Vector(1);

        public int Write(ObservationWriter writer)
        {
            if (spawner != null && grid != null && grid.Size.x > 0 && grid.Size.y > 0)
            {
                int enemyCount = spawner.ActiveEntityCount;
                float gridArea = grid.Size.x * grid.Size.y;
                writer[0] = enemyCount / gridArea;
            }
            else
            {
                writer[0] = 0f;
            }
            return 1;
        }

        public byte[] GetCompressedObservation() => null;
        public void Update() { }
        public void Reset() { }
        public CompressionSpec GetCompressionSpec() => CompressionSpec.Default();
    }
}

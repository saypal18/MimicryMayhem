using Unity.MLAgents.Sensors;
using UnityEngine;

public class TotalPickupObservation : SensorComponent
{
    private PickupPlacer pendingPlacer;
    private Grid pendingGrid;
    private TotalPickupSensor sensor;

    public void SetAgentReferences(PickupPlacer placer, Grid gridRef)
    {
        if (sensor != null)
        {
            sensor.SetAgentReferences(placer, gridRef);
        }
        else
        {
            pendingPlacer = placer;
            pendingGrid = gridRef;
        }
    }

    public override ISensor[] CreateSensors()
    {
        sensor = new TotalPickupSensor();
        if (pendingPlacer != null && pendingGrid != null)
        {
            sensor.SetAgentReferences(pendingPlacer, pendingGrid);
            pendingPlacer = null;
            pendingGrid = null;
        }
        return new ISensor[] { sensor };
    }

    public class TotalPickupSensor : ISensor
    {
        private PickupPlacer placer;
        private Grid grid;

        public void SetAgentReferences(PickupPlacer pickupPlacer, Grid gridRef)
        {
            placer = pickupPlacer;
            grid = gridRef;
        }

        public string GetName() => "TotalPickupObservation";
        public ObservationSpec GetObservationSpec() => ObservationSpec.Vector(1);

        public int Write(ObservationWriter writer)
        {
            if (placer != null && grid != null && grid.Size.x > 0 && grid.Size.y > 0)
            {
                int pickupCount = placer.ActivePickupCount;
                float gridArea = grid.Size.x * grid.Size.y;
                writer[0] = pickupCount / gridArea;
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

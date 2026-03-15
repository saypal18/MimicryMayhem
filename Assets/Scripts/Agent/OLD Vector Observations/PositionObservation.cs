using Unity.MLAgents.Sensors;
using UnityEngine;

public class PositionObservation : SensorComponent
{
    private GridPlaceable pendingPlaceable;
    private Grid pendingGrid;
    private PositionSensor sensor;

    public void SetAgentReferences(GridPlaceable placeable, Grid gridRef)
    {
        if (sensor != null)
        {
            sensor.SetAgentReferences(placeable, gridRef);
        }
        else
        {
            pendingPlaceable = placeable;
            pendingGrid = gridRef;
        }
    }

    public override ISensor[] CreateSensors()
    {
        sensor = new PositionSensor();
        if (pendingPlaceable != null && pendingGrid != null)
        {
            sensor.SetAgentReferences(pendingPlaceable, pendingGrid);
            // pendingPlaceable = null;
            // pendingGrid = null;
        }
        return new ISensor[] { sensor };
    }

    public class PositionSensor : ISensor
    {
        private GridPlaceable gridPlaceable;
        private Grid grid;

        public void SetAgentReferences(GridPlaceable placeable, Grid gridRef)
        {
            gridPlaceable = placeable;
            grid = gridRef;
        }

        public string GetName() => "PositionObservation";
        public ObservationSpec GetObservationSpec() => ObservationSpec.Vector(2);

        public int Write(ObservationWriter writer)
        {
            if (gridPlaceable != null && grid != null && grid.Size.x > 0 && grid.Size.y > 0)
            {
                Vector2Int pos = gridPlaceable.Position;
                Vector2Int size = grid.Size;
                writer[0] = (float)pos.x / size.x;
                writer[1] = (float)pos.y / size.y;
            }
            else
            {
                writer[0] = 0f;
                writer[1] = 0f;
            }
            return 2;
        }

        public byte[] GetCompressedObservation() => null;
        public void Update() { }
        public void Reset() { }
        public CompressionSpec GetCompressionSpec() => CompressionSpec.Default();
    }
}

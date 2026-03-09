using Unity.MLAgents.Sensors;
using UnityEngine;

public class PowerObservation : SensorComponent
{
    private DamageResolver pendingDamageResolver;
    private Grid pendingGrid;
    private PowerSensor sensor;

    public void SetAgentReferences(DamageResolver damageResolver, Grid gridRef)
    {
        if (sensor != null)
        {
            sensor.SetAgentReferences(damageResolver, gridRef);
        }
        else
        {
            pendingDamageResolver = damageResolver;
            pendingGrid = gridRef;
        }
    }

    public override ISensor[] CreateSensors()
    {
        sensor = new PowerSensor();
        if (pendingDamageResolver != null && pendingGrid != null)
        {
            sensor.SetAgentReferences(pendingDamageResolver, pendingGrid);
            pendingDamageResolver = null;
            pendingGrid = null;
        }
        return new ISensor[] { sensor };
    }

    public class PowerSensor : ISensor
    {
        private DamageResolver damageResolver;
        private Grid grid;

        public void SetAgentReferences(DamageResolver dmgResolver, Grid gridRef)
        {
            damageResolver = dmgResolver;
            grid = gridRef;
        }

        public string GetName() => "PowerObservation";
        public ObservationSpec GetObservationSpec() => ObservationSpec.Vector(1);
        
        public int Write(ObservationWriter writer)
        {
            if (damageResolver != null && grid != null && grid.Size.x > 0 && grid.Size.y > 0)
            {
                int currentPower = damageResolver.power;
                float gridSize = (grid.Size.x + grid.Size.y) / 2f;
                // Clamp the normalized power between 0 and 1
                writer[0] = Mathf.Clamp01((float)currentPower / gridSize);
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

# C:\Users\Sayantan Pal\Unity\ml-agents\ml-agents\standalone_export.py
import os
import argparse
import torch
import cattr
from mlagents.trainers.torch_entities.model_serialization import ModelSerializer
from mlagents.trainers.policy.torch_policy import TorchPolicy
from mlagents_envs.base_env import BehaviorSpec, ActionSpec, ObservationSpec, ObservationType, DimensionProperty
from mlagents.trainers.cli_utils import load_config
from mlagents.trainers.settings import SerializationSettings, TrainerSettings, NetworkSettings
from mlagents.trainers.upgrade_config import convert as convert_config

# ==========================================================
# CONFIGURATION: Change these to match your Unity Agent!
# ==========================================================
GRID_SIZE = (11, 11)   # The view size of your Grid Sensor
GRID_CHANNELS = 5      # Number of channels in CustomGridSensor.cs
ACTION_SIZE = 5        # Number of discrete actions (Up, Down, Left, Right, NoAction)
MEMORY_SIZE = 256      # MUST match 'memory_size' in your YAML
# ==========================================================

def get_behavior_spec():
    grid_obs = ObservationSpec(
        shape=(GRID_CHANNELS, GRID_SIZE[0], GRID_SIZE[1]), # (5, 11, 11) - channels must be first
        dimension_property=(
            DimensionProperty.NONE, 
            DimensionProperty.TRANSLATIONAL_EQUIVARIANCE, 
            DimensionProperty.TRANSLATIONAL_EQUIVARIANCE
        ),
        observation_type=ObservationType.DEFAULT,
        name="CustomGridSensor"
    )
    action = ActionSpec(continuous_size=0, discrete_branches=(ACTION_SIZE,))
    return BehaviorSpec(observation_specs=[grid_obs], action_spec=action)

def standalone_export():
    parser = argparse.ArgumentParser(description="Standalone ML-Agents Exporter")
    parser.add_argument("--run-id", type=str, required=True)
    parser.add_argument("--behavior", type=str, required=True)
    parser.add_argument("--checkpoint", type=str, required=True)
    parser.add_argument("--config", type=str, required=True)
    parser.add_argument("--results-dir", type=str, default="results")
    
    args = parser.parse_args()

    # Path discovery
    pt_file = f"{args.behavior}-{args.checkpoint}.pt"
    pt_path = os.path.join(args.results_dir, args.run_id, args.behavior, pt_file)
    
    if not os.path.exists(pt_path):
        pt_path = os.path.abspath(os.path.join(args.results_dir, args.run_id, args.behavior, pt_file))
        if not os.path.exists(pt_path):
            print(f"FAILED: Could not find {pt_path}")
            return

    # Enable ONNX
    SerializationSettings.convert_to_onnx = True
    SerializationSettings.onnx_opset = 13

    # Load and Upgrade Config
    print(f"--- Processing YAML Config ---")
    raw_config = load_config(args.config)
    upgraded_dict = convert_config(raw_config, None, None)
    
    # Extract the behavior data
    behaviors_dict = upgraded_dict.get("behaviors", {})
    target_behavior = args.behavior
    if target_behavior not in behaviors_dict:
        if len(behaviors_dict) == 1:
            target_behavior = list(behaviors_dict.keys())[0]
            print(f"Defaulting to only available behavior: {target_behavior}")
        else:
            print(f"Error: Could not find behavior in YAML.")
            return

    # !!! CRITICAL: Convert dict to Class Object !!!
    # ML-Agents uses cattr.structure to do this conversion
    # We use a try/except because 'load_config' + 'convert' might return inconsistent types
    try:
        behavior_settings = cattr.structure(behaviors_dict[target_behavior], TrainerSettings)
        print(f"  [OK] Parsed behavior settings from YAML.")
    except Exception as e:
        print(f"  [!!] Info: Could not auto-parse full TrainerSettings ({e}). Using targeted fallback.")
        # Manual fallback for the specific things we need
        from mlagents.trainers.settings import NetworkSettings
        
        # Extract network settings from the dictionary manually
        net_dict = behaviors_dict[target_behavior].get("network_settings", {})
        network_settings = cattr.structure(net_dict, NetworkSettings)
        
        # FORCED SYNC: Ensure the memory is actually enabled and correct size
        # This fixes the Unity "reshaped length" error.
        network_settings.memory = NetworkSettings.MemorySettings(
            sequence_length=64,
            memory_size=MEMORY_SIZE
        )
        
        # Create a mock object with just the attribute we need
        behavior_settings = type('obj', (object,), {'network_settings': network_settings})
        print(f"  [OK] Reconstructed network settings with {MEMORY_SIZE} memory units.")

    # Reconstruct Policy
    print(f"Reconstructing policy architecture...")
    from mlagents.trainers.torch_entities.networks import SimpleActor
    policy = TorchPolicy(
        seed=0,
        behavior_spec=get_behavior_spec(),
        network_settings=behavior_settings.network_settings,
        actor_cls=SimpleActor,
        actor_kwargs={"conditional_sigma": False, "tanh_squash": False}
    )
    
    # Load Weights
    print(f"Loading weights from: {pt_path}")
    checkpoint_data = torch.load(pt_path, map_location='cpu')
    modules = policy.get_modules()
    for name, mod in modules.items():
        if name in checkpoint_data:
            if hasattr(mod, "load_state_dict"):
                mod.load_state_dict(checkpoint_data[name])
                print(f"  [OK] Loaded module: {name}")
            else:
                print(f"  [--] Skipped non-weight object: {name}")
        else:
            print(f"  [!!] Warning: Module '{name}' not found in checkpoint")

    # Export
    print(f"Initializing ModelSerializer...")
    serializer = ModelSerializer(policy)
    onnx_path = pt_path.replace(".pt", "")
    
    print(f"🚀 CALLING MODEL EXPORT (This is where it usually crashes on Windows)...")
    try:
        serializer.export_policy_model(onnx_path)
    except Exception as e:
        print(f"❌ Python Error during export: {e}")
        return

    expected_onnx = f"{onnx_path}.onnx"
    if os.path.exists(expected_onnx):
        print(f"🏁 --- SUCCESS! ---")
        print(f"File created: {expected_onnx}")
    else:
        print(f"❓ --- FINISHED BUT NO FILE FOUND ---")
        print(f"Check logs for hidden errors.")

if __name__ == "__main__":
    standalone_export()

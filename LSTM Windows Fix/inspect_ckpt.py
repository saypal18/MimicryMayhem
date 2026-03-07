# C:\Users\Sayantan Pal\Unity\ml-agents\ml-agents\inspect_ckpt.py
import torch
import os

pt_path = r'C:\Users\Sayantan Pal\Unity\ml-agents\ml-agents\results\run6\SurvivorAgent\SurvivorAgent-4980.pt'
if not os.path.exists(pt_path):
    # Try relative path
    pt_path = r'results\run6\SurvivorAgent\SurvivorAgent-4980.pt'

if os.path.exists(pt_path):
    ckpt = torch.load(pt_path, map_location='cpu')
    print("Keys in state_dict:")
    for k in ckpt.keys():
        print(f"Module: {k}")
        for param in ckpt[k]:
            print(f"  {param}: {ckpt[k][param].shape}")
            break # just first one
else:
    print(f"File not found: {pt_path}")

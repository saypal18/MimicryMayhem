# C:\Users\Sayantan Pal\Unity\ml-agents\ml-agents\mlagents\trainers\model_saver\torch_model_saver.py
    def save_checkpoint(self, behavior_name: str, step: int) -> Tuple[str, List[str]]:
        if not os.path.exists(self.model_path):
            os.makedirs(self.model_path)
        checkpoint_path = os.path.join(self.model_path, f"{behavior_name}-{step}")
        state_dict = {
            name: module.state_dict() for name, module in self.modules.items()
        }
        pytorch_ckpt_path = f"{checkpoint_path}.pt"
        export_ckpt_path = f"{checkpoint_path}.onnx"
        torch.save(state_dict, f"{checkpoint_path}.pt")
        torch.save(state_dict, os.path.join(self.model_path, DEFAULT_CHECKPOINT_NAME))
        if SerializationSettings.convert_to_onnx:
            self.export(checkpoint_path, behavior_name)
        return export_ckpt_path, [pytorch_ckpt_path]
using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomModifiers : RoomComponent
{
	[NonSerialized]
	public List<RoomModifierBase> modifierInstances = new List<RoomModifierBase>();

	public override void OnRoomStartServer()
	{
		base.OnRoomStartServer();
		List<ModifierData> modifiers = NetworkedManagerBase<ZoneManager>.instance.currentNode.modifiers;
		for (int i = 0; i < modifiers.Count; i++)
		{
			HandleRuntimeAddition(modifiers[i].id);
		}
	}

	internal void HandleRuntimeRemoval(int id)
	{
		int num = modifierInstances.FindIndex((RoomModifierBase m) => m.id == id);
		if (num < 0)
		{
			Debug.Log($"Runtime removal failed; Modifier instance with id {id} not found");
			return;
		}
		if (!modifierInstances[num].IsNullOrInactive())
		{
			modifierInstances[num].Destroy();
		}
		modifierInstances.RemoveAt(num);
	}

	internal void HandleRuntimeAddition(int id, Action<RoomModifierBase> beforePrepare = null)
	{
		List<ModifierData> modifiers = NetworkedManagerBase<ZoneManager>.instance.currentNode.modifiers;
		int num = NetworkedManagerBase<ZoneManager>.instance.currentNode.modifiers.FindIndex((ModifierData m) => m.id == id);
		if (num < 0)
		{
			Debug.Log($"Runtime addition failed; Modifier instance with id {id} not found");
			return;
		}
		ModifierData modifierData = modifiers[num];
		RoomModifierBase byShortTypeName = DewResources.GetByShortTypeName<RoomModifierBase>(modifierData.type, default(ResourceLoadSettings));
		ModifierServerData data = NetworkedManagerBase<ZoneManager>.instance.modifierServerData[id];
		bool isNewInstance = !data.didCreateInstance;
		RoomModifierBase newMod = Dew.CreateActor(byShortTypeName, Vector3.zero, null, null, (RoomModifierBase mod) =>
		{
			mod.isNewInstance = isNewInstance;
			mod.id = id;
			DewPersistence.DeserializeOnGameObject(((Component)(object)mod).gameObject, data.persistentData, null, SaveVarFlags.Default, SaveVarFlags.ApplyAfterCreation | SaveVarFlags.ApplyAfterFrameDelay);
			try
			{
				beforePrepare?.Invoke(mod);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		});
		DewPersistence.DeserializeOnGameObject(((Component)(object)newMod).gameObject, data.persistentData, null, SaveVarFlags.ApplyAfterCreation);
		Dew.CallDelayed(() =>
		{
			DewPersistence.DeserializeOnGameObject(((Component)(object)newMod).gameObject, data.persistentData, null, SaveVarFlags.ApplyAfterFrameDelay);
		});
		if (isNewInstance)
		{
			NetworkedManagerBase<ZoneManager>.instance.modifierServerData[id].didCreateInstance = true;
		}
		modifierInstances.Add(newMod);
		Debug.Log("Added modifier of type " + modifierData.type);
	}

	private void MirrorProcessed()
	{
	}
}

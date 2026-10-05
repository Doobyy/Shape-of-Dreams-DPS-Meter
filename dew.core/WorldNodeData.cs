using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct WorldNodeData
{
	public WorldNodeType type;

	public WorldNodeStatus status;

	public string room;

	public string roomOverride;

	public float roomRotValue;

	public int roomRotIndex;

	public List<ModifierData> modifiers;

	public Vector2 position;

	public bool IsSidetrackNode()
	{
		return position.x < 0f;
	}

	public bool IsModifierVisible(int index)
	{
		if (modifiers[index].isForceRevealed)
		{
			return true;
		}
		RoomModifierBase byShortTypeName = DewResources.GetByShortTypeName<RoomModifierBase>(modifiers[index].type, default(ResourceLoadSettings));
		if ((UnityEngine.Object)(object)byShortTypeName == null)
		{
			return false;
		}
		if (byShortTypeName.hiddenOnVisitedNode && status == WorldNodeStatus.HasVisited)
		{
			return false;
		}
		return byShortTypeName.visibilityOnWorld switch
		{
			NodeModifierVisibility.OnRevealed => status >= WorldNodeStatus.Revealed, 
			NodeModifierVisibility.OnRevealedFull => status >= WorldNodeStatus.RevealedFull, 
			NodeModifierVisibility.Hidden => false, 
			NodeModifierVisibility.Always => true, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public bool HasMainModifier()
	{
		foreach (ModifierData modifier in modifiers)
		{
			RoomModifierBase byShortTypeName = DewResources.GetByShortTypeName<RoomModifierBase>(modifier.type, default(ResourceLoadSettings));
			if ((UnityEngine.Object)(object)byShortTypeName != null && byShortTypeName.isMain)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasModifier<T>() where T : RoomModifierBase
	{
		return HasModifier(typeof(T).Name);
	}

	public bool HasModifier(string modName)
	{
		if (modifiers != null)
		{
			return modifiers.FindIndex((ModifierData m) => m.type == modName) != -1;
		}
		return false;
	}

	public bool HasModifier(int id)
	{
		if (modifiers != null)
		{
			return modifiers.FindIndex((ModifierData m) => m.id == id) != -1;
		}
		return false;
	}

	public int FindModifierIndex<T>() where T : RoomModifierBase
	{
		return FindModifierIndex(typeof(T).Name);
	}

	public int FindModifierIndex(string modName)
	{
		if (modifiers == null)
		{
			return -1;
		}
		return modifiers.FindIndex((ModifierData m) => m.type == modName);
	}
}

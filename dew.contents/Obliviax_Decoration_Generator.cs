using System.Collections.Generic;
using UnityEngine;

public class Obliviax_Decoration_Generator : Actor
{
	public bool enableInnerDecorations;

	public DecorationSettings innerDecorations;

	public bool enableOuterDecorations;

	public DecorationSettings outerDecorations;

	private List<GameObject> _spawnedProps = new List<GameObject>();

	public override void OnStart()
	{
		base.OnStart();
		_spawnedProps.Clear();
		if (enableInnerDecorations)
		{
			_spawnedProps.AddRange(RoomModifierBase.PlaceDecorations(innerDecorations, SingletonDewNetworkBehaviour<Room>.instance.map.mapData.innerPropNodeIndices));
		}
		if (enableOuterDecorations)
		{
			_spawnedProps.AddRange(RoomModifierBase.PlaceDecorations(outerDecorations, SingletonDewNetworkBehaviour<Room>.instance.map.mapData.outerPropNodeIndices));
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_spawnedProps == null)
		{
			return;
		}
		foreach (GameObject spawnedProp in _spawnedProps)
		{
			if (spawnedProp != null)
			{
				Object.Destroy(spawnedProp);
			}
		}
		_spawnedProps.Clear();
		_spawnedProps = null;
	}

	private void MirrorProcessed()
	{
	}
}

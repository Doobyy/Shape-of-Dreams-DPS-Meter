using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class RoomMod_EngulfedInFlame : RoomModifierBase
{
	public GameObject treeEffect;

	public float effectChance;

	public Color treeTint;

	private List<GameObject> _treeEffects = new List<GameObject>();

	private TerrainData _originalTerrainData;

	public override void OnStartServer()
	{
		base.OnStartServer();
		Entity[] array = NetworkedManagerBase<ActorManager>.instance.allEntities.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] is Monster { campPosition: not null } monster)
			{
				monster.Destroy();
			}
		}
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd += new Action<Entity>(OnEntityAdd);
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			OnEntityAdd(allEntity);
		}
	}

	public override void OnStart()
	{
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		base.OnStart();
		Terrain val = UnityEngine.Object.FindObjectOfType<Terrain>();
		if ((UnityEngine.Object)(object)val == null)
		{
			return;
		}
		_originalTerrainData = val.terrainData;
		val.terrainData = UnityEngine.Object.Instantiate<TerrainData>(_originalTerrainData);
		TreePrototype[] array = _originalTerrainData.treePrototypes.ToArray();
		bool[] array2 = new bool[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			TreePrototype val2 = array[i];
			array2[i] = val2.prefab.TryGetComponent<Forest_FlammableTree>(out var _);
			GameObject gameObject = UnityEngine.Object.Instantiate(val2.prefab);
			Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				Material[] array3 = renderer.sharedMaterials.ToArray();
				for (int k = 0; k < array3.Length; k++)
				{
					array3[k] = UnityEngine.Object.Instantiate(array3[k]);
					Color color = array3[k].GetColor("_BaseColor");
					array3[k].SetColor("_BaseColor", treeTint * color);
				}
				renderer.sharedMaterials = array3;
			}
			val2.prefab = gameObject;
		}
		val.terrainData.treePrototypes = array;
		Vector3 size = _originalTerrainData.size;
		Vector3 vector = ((Component)(object)val).transform.position;
		TreeInstance[] treeInstances = val.terrainData.treeInstances;
		foreach (TreeInstance val3 in treeInstances)
		{
			if (!(UnityEngine.Random.value > effectChance) && array2[val3.prototypeIndex])
			{
				Vector3 vector2 = Vector3.Scale(val3.position, size) + vector;
				Quaternion quaternion = Quaternion.AngleAxis(val3.rotation * 57.29578f, Vector3.up);
				_treeEffects.Add(UnityEngine.Object.Instantiate(treeEffect, vector2, quaternion));
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		foreach (GameObject treeEffect in _treeEffects)
		{
			if (treeEffect != null)
			{
				UnityEngine.Object.Destroy(treeEffect);
			}
		}
		_treeEffects.Clear();
		if ((UnityEngine.Object)(object)_originalTerrainData == null)
		{
			return;
		}
		Terrain val = UnityEngine.Object.FindObjectOfType<Terrain>();
		if ((UnityEngine.Object)(object)val == null)
		{
			return;
		}
		val.terrainData = _originalTerrainData;
		if (!((NetworkBehaviour)this).isServer || (UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance == null)
		{
			return;
		}
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd -= new Action<Entity>(OnEntityAdd);
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			if (allEntity.Status.TryGetStatusEffect<Se_EngulfedInFlame>(out var effect))
			{
				effect.Destroy();
			}
		}
	}

	private void OnEntityAdd(Entity obj)
	{
		if (obj is Monster monster)
		{
			monster.CreateStatusEffect<Se_EngulfedInFlame>(monster, new CastInfo(monster));
		}
	}

	public override bool CanSpawnAtNode(int nodeIndex)
	{
		if (NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex <= 0)
		{
			return NetworkedManagerBase<ZoneManager>.instance.GetNodeDistance(0, nodeIndex) > 2;
		}
		return true;
	}

	private void MirrorProcessed()
	{
	}
}

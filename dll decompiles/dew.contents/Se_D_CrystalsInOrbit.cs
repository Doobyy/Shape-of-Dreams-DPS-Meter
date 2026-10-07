using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Se_D_CrystalsInOrbit : StatusEffect
{
	public GameObject crystalTemplate;

	public float defaultOrbitRadius;

	public float orbitRadiusSmoothDampTime;

	public Transform crystalParent;

	public ScalingValue crystalCount;

	[SyncVar]
	private int _count;

	[SyncVar]
	private float _currentOrbitRadiusTarget;

	private float _currentOrbitRadius;

	private float _cv;

	private List<GameObject> _effects;

	public int Network_count
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
		[param: In]
		set
		{
		}
	}

	public float Network_currentOrbitRadiusTarget
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
		[param: In]
		set
		{
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
	}

	protected override void ActiveFrameUpdate()
	{
	}

	protected override void OnDestroyActor()
	{
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
	}
}

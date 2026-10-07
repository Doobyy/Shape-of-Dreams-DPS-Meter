using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_GhostBlade_Atk : StandardProjectile
{
	[NonSerialized]
	[SyncVar]
	public bool isFirst;

	public GameObject firstFlyEffect;

	public GameObject secondFlyEffect;

	public ScalingValue dmgFactor;

	public override bool reuseInRoom
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	public bool NetworkisFirst
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

	protected override void OnCreate()
	{
	}

	protected override void OnEntity(EntityHit hit)
	{
	}

	protected override void OnComplete()
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

using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(GenericTransformSync))]
public class Ai_Mon_Sky_BossNyx_StarBuff_Instance : InstantDamageInstance
{
	public GameObject fxTelegraph;

	[NonSerialized]
	[SyncVar]
	public float angularSpeed;

	[SyncVar]
	private float _angle;

	public float NetworkangularSpeed
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

	public float Network_angle
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

	protected override void ActiveFrameUpdate()
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

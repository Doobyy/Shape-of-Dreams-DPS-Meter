using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class PropEnt_GlacialBarrier : PropEntity, IDisableOcclusionTest
{
	[HideInInspector]
	public float spawnDelay;

	public Vector2 modelScale;

	public AnimationCurve curve;

	public float existTimer;

	public NavMeshObstacle obstacle;

	[SyncVar]
	public bool enableSpawnEffect;

	[SyncVar]
	private float _scale;

	private float _startRadius;

	private Vector3 _baseFxDeathScale;

	private bool _didCaptureFxDeathScale;

	public float scale
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	public bool NetworkenableSpawnEffect
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

	public float Network_scale
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

	protected override void OnPrepare()
	{
	}

	protected override void OnCreate()
	{
	}

	[ClientRpc]
	private void RpcActivateObstacle()
	{
	}

	[ClientRpc]
	private void RpcApplyTransform()
	{
	}

	protected override void OnDestroyActor()
	{
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcActivateObstacle()
	{
	}

	protected static void InvokeUserCode_RpcActivateObstacle(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
	}

	protected void UserCode_RpcApplyTransform()
	{
	}

	protected static void InvokeUserCode_RpcApplyTransform(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
	}

	static PropEnt_GlacialBarrier()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
	}
}

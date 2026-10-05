using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Se_MorasDomain_AppearFromGround : StatusEffect
{
	public SafeAction onDisappearAnimFinish;

	public DewEase disappearEase;

	public DewEase appearEase;

	public GameObject fxDisappearOnGround;

	public GameObject fxDisappear;

	public GameObject fxAppearOnGround;

	public GameObject fxAppear;

	[NonSerialized]
	[SyncVar]
	public bool skipDisappearAnim;

	[NonSerialized]
	public bool dontDoInvulnerable;

	private EntityTransformModifier _mod;

	public override bool reuseInRoom
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	public bool NetworkskipDisappearAnim
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

	protected override void OnDisable()
	{
	}

	protected override void OnCreate()
	{
	}

	protected override void OnDestroyActor()
	{
	}

	private void DisappearLocal(bool skipAnimation)
	{
	}

	[Server]
	public void Appear()
	{
	}

	[ClientRpc]
	private void RpcAppear()
	{
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcAppear()
	{
	}

	protected static void InvokeUserCode_RpcAppear(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
	}

	static Se_MorasDomain_AppearFromGround()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
	}
}

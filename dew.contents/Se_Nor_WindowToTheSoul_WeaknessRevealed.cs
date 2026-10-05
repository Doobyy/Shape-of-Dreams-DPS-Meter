using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Se_Nor_WindowToTheSoul_WeaknessRevealed : StatusEffect
{
	public DewCollider activatableRange;

	public GameObject weaknessEffect;

	public GameObject useEffect;

	[SyncVar]
	public bool reverseSpin;

	[SyncVar(hook = "SetRotation")]
	public Quaternion currentRotation;

	[SyncVar]
	private float _currentWeaknessStartTime;

	public float spinSpeed;

	public float minActivationTime;

	private float _lastRotationSyncTime;

	public Action<Quaternion, Quaternion> _Mirror_SyncVarHookDelegate_currentRotation;

	public override bool reuseInRoom
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	public bool NetworkreverseSpin
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

	public Quaternion NetworkcurrentRotation
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

	public float Network_currentWeaknessStartTime
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

	protected override void OnDestroyActor()
	{
	}

	public bool CanActivate(Entity activator)
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	[Server]
	public void UseWeakness()
	{
	}

	private void SetRotation(Quaternion oldVal, Quaternion newVal)
	{
	}

	protected override void ActiveFrameUpdate()
	{
	}

	protected override void ActiveLogicUpdate(float dt)
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

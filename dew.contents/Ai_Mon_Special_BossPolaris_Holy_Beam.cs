using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;

public class Ai_Mon_Special_BossPolaris_Holy_Beam : TickDamageInstance
{
	public float trackAngularSpeed;

	public float consecutiveHitDmgReduction;

	[SyncVar]
	private bool _isGoingClockwise;

	private List<Entity> _hitEntities;

	public bool Network_isGoingClockwise
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

	protected override void ActiveFrameUpdate()
	{
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
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

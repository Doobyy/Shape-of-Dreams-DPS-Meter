using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class St_D_DanceOfValor : SkillTrigger
{
	public int maxChargeCount;

	public GameObject fxCharged;

	[SyncVar(hook = "OnChargeCountChanged")]
	private int _chargeCount;

	private int _previousMaxCharges;

	private float _firstChargeTime;

	public Action<int, int> _Mirror_SyncVarHookDelegate__chargeCount;

	public int Network_chargeCount
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

	protected override void OnEquip(Entity newOwner)
	{
	}

	protected override void OnUnequip(Entity formerOwner)
	{
	}

	private void ClientHeroEventOnSkillUse(EventInfoSkillUse obj)
	{
	}

	private void EntityEventOnAttackEffectTriggered(EventInfoAttackEffect obj)
	{
	}

	private void OnChargeCountChanged(int prev, int newVal)
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

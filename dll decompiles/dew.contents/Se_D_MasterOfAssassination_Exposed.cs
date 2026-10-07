using System;
using System.Collections.Generic;
using UnityEngine;

public class Se_D_MasterOfAssassination_Exposed : StatusEffect
{
	private List<Quaternion> _prevRots;

	private int _prevRotIndex;

	private Action<EventInfoAttackEffect> _onAttackEffectTriggered;

	public override bool reuseInRoom
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	protected override void OnDisable()
	{
	}

	protected override void OnCreate()
	{
	}

	private void EntityEventOnAttackEffectTriggered(EventInfoAttackEffect obj)
	{
	}

	public bool IsBackstab()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	protected override void OnDestroyActor()
	{
	}

	protected override void ActiveLogicUpdate(float dt)
	{
	}

	private void MirrorProcessed()
	{
	}
}

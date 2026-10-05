using System;
using Mirror;
using UnityEngine;

public class Se_R_Inspire_Buff : StatusEffect
{
	public GameObject fxHit;

	public float duration;

	public ScalingValue haste;

	public ScalingValue damage;

	public float selfMultiplier;

	[NonSerialized]
	public Se_R_Inspire_Buff receivingFrom;

	[NonSerialized]
	public Se_R_Inspire_Buff givingTo;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		receivingFrom = null;
		givingTo = null;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoHaste(GetValue(haste) * (((UnityEngine.Object)(object)info.caster == (UnityEngine.Object)(object)victim) ? selfMultiplier : 1f));
		DoAttackEmpower((EventInfoAttackEffect effect, int i) =>
		{
			if (givingTo.IsNullOrInactive() && !effect.chain.DidReact(this))
			{
				float num = 1f;
				Se_R_Inspire_Buff se_R_Inspire_Buff = receivingFrom;
				while (!se_R_Inspire_Buff.IsNullOrInactive())
				{
					num++;
					se_R_Inspire_Buff = se_R_Inspire_Buff.receivingFrom;
				}
				Damage(damage).ApplyStrength(effect.strength * num).ApplyRawMultiplier(((UnityEngine.Object)(object)info.caster == (UnityEngine.Object)(object)victim) ? selfMultiplier : 1f).SetOriginPosition(victim.agentPosition)
					.SetAttr(DamageAttribute.ForceMergeNumber)
					.Dispatch(effect.victim, effect.chain.New(this));
				FxPlayNewNetworked(fxHit, effect.victim);
			}
		});
		SetTimer(duration);
		ShowOnScreenTimer();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)givingTo != null)
			{
				givingTo.receivingFrom = receivingFrom;
			}
			if ((UnityEngine.Object)(object)receivingFrom != null)
			{
				receivingFrom.givingTo = givingTo;
			}
			givingTo = null;
			receivingFrom = null;
		}
	}

	private void MirrorProcessed()
	{
	}
}

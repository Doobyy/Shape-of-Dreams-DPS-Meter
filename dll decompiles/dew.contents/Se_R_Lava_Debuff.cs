using Mirror;
using UnityEngine;

public class Se_R_Lava_Debuff : StatusEffect
{
	public ScalingValue fireDamageAmp;

	public float debuffDuration = 1f;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(debuffDuration);
			victim.takenDamageProcessor.Add(AmpFireDamage);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (bool)(Object)(object)victim)
		{
			victim.takenDamageProcessor.Remove(AmpFireDamage);
		}
	}

	private void AmpFireDamage(ref DamageData data, Actor actor, Entity target)
	{
		if (data.elemental == ElementalType.Fire && !data.IsAmountModifiedBy(this))
		{
			data.ApplyAmplification(GetValue(fireDamageAmp));
			data.SetAttr(DamageAttribute.IsCrit);
			data.SetAmountModifiedBy(this);
		}
	}

	private void MirrorProcessed()
	{
	}
}

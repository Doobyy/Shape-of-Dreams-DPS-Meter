using Mirror;

public class Se_Star_D_ShieldDamageAmp : StarEffect
{
	public StarScalingValue damageToShieldAmp;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.dealtDamageProcessor.Add(Processor, 999999);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !victim.IsNullOrInactive())
		{
			victim.dealtDamageProcessor.Remove(Processor);
		}
	}

	private void Processor(ref DamageData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this) && victim.CheckEnemyOrNeutral(target) && !data.HasAttr(DamageAttribute.IgnoreShield) && !(target.Status.currentShield <= 0.0001f))
		{
			data.SetAmountModifiedBy(this);
			data.ApplyDamageToShieldMultiplier(1f + GetValue(damageToShieldAmp));
			data.SetAttr(DamageAttribute.IsCrit);
		}
	}

	private void MirrorProcessed()
	{
	}
}

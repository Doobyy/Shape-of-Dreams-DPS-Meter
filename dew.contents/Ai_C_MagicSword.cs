using Mirror;

public class Ai_C_MagicSword : InstantDamageInstance
{
	public Dash dash;

	public ScalingValue adDamage;

	public ScalingValue apDamage;

	private bool _isUsingAdDamage;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_isUsingAdDamage = false;
	}

	protected override void OnCreate()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			base.OnCreate();
			return;
		}
		float value = GetValue(adDamage);
		float value2 = GetValue(apDamage);
		_isUsingAdDamage = value >= value2;
		dmgFactor = (_isUsingAdDamage ? adDamage : apDamage);
		type = (_isUsingAdDamage ? DamageData.SourceType.Physical : DamageData.SourceType.Magic);
		base.OnCreate();
		dash.ApplyByDirection(info.caster, info.forward);
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		if (_isUsingAdDamage)
		{
			dmg.DoAttackEffect(AttackEffectType.Others);
		}
	}

	private void MirrorProcessed()
	{
	}
}

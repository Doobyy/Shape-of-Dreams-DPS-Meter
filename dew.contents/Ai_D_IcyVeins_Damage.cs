using System;

public class Ai_D_IcyVeins_Damage : InstantDamageInstance
{
	[NonSerialized]
	public bool isCrit;

	[NonSerialized]
	private ScalingValue _baseDmgFactor;

	[NonSerialized]
	private float _baseStrengthMultiplier;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseDmgFactor = dmgFactor;
		_baseStrengthMultiplier = strengthMultiplier;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		dmgFactor = _baseDmgFactor;
		strengthMultiplier = _baseStrengthMultiplier;
		isCrit = false;
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		if (isCrit)
		{
			dmg.SetAttr(DamageAttribute.IsCrit);
		}
	}

	private void MirrorProcessed()
	{
	}
}

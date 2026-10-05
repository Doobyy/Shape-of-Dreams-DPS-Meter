using System;
using UnityEngine;

public class Ai_Mon_Ink_BossDarkMoon_ThrowSpear_TeleportAtk : InstantDamageInstance
{
	public float slowStrength;

	public float slowDuration;

	[NonSerialized]
	public bool isRage;

	protected override void OnDisable()
	{
		base.OnDisable();
		isRage = false;
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		CreateBasicEffect(entity, new SlowEffect
		{
			decay = true,
			strength = slowStrength
		}, slowDuration, "darkmoon_slow");
	}

	protected override void OnAfterDelay()
	{
		base.OnAfterDelay();
		info.caster.Control.StartDaze(0.75f);
		if (isRage)
		{
			CreateAbilityInstance<Ai_Mon_Ink_BossDarkMoon_ThrowSpear_TeleportAtk_AfterAtk>(info.caster.agentPosition, info.caster.rotation, new CastInfo(info.caster, CastInfo.GetAngle(((Component)(object)info.caster).transform.forward)));
		}
	}

	private void MirrorProcessed()
	{
	}
}

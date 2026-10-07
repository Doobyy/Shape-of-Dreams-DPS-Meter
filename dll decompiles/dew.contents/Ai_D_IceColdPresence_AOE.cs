using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_D_IceColdPresence_AOE : InstantDamageInstance
{
	public ScalingValue addedHealth;

	public float postDelay;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		yield return base.OnCreateSequenced();
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			info.caster.Control.StartDaze(postDelay);
			CreateBasicEffect(info.caster, new InvulnerableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
			yield return new SI.WaitForSeconds(postDelay);
			DestroyIfActive();
		}
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		if (!info.caster.Status.TryGetStatusEffect<Se_D_IceColdPresence_PersistentBuff>(out var se))
		{
			se = CreateStatusEffect<Se_D_IceColdPresence_PersistentBuff>(info.caster, new CastInfo(info.caster));
		}
		float addedHealthValue = GetValue(addedHealth);
		St_D_IceColdPresence trigger = firstTrigger as St_D_IceColdPresence;
		Dew.CallDelayed(() =>
		{
			if (!((Object)(object)se == null) && se.isActive && !((Object)(object)trigger == null))
			{
				se.bonus.maxHealthFlat += addedHealthValue;
				trigger.UpdateStack(se.bonus.maxHealthFlat);
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}

using System.Collections;
using UnityEngine;

public class Ai_C_BackStep_Bomb : StandardProjectile
{
	public ScalingValue dmgFactor;

	public DewCollider range;

	public Knockback Knockback;

	public float procCoefficient;

	public GameObject hitEffect;

	public float stunDuration;

	public override bool reuseInRoom => true;

	protected override void OnComplete()
	{
		base.OnComplete();
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
		{
			FxPlayNewNetworked(hitEffect, entity);
			Damage(dmgFactor, procCoefficient).SetOriginPosition(info.point).Dispatch(entity);
			Knockback.ApplyWithOrigin(info.point, entity);
			if (stunDuration > 0f)
			{
				CreateBasicEffect(entity, new StunEffect(), stunDuration, "backstepStun", DuplicateEffectBehavior.UsePrevious);
			}
		}
		handle.Return();
		static IEnumerator Routine()
		{
			yield return new WaitForSeconds(1f);
		}
	}

	private void MirrorProcessed()
	{
	}
}

using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossErebos_Ripple : AbilityInstance
{
	public float delay;

	public float addedDelay;

	public DewCollider[] ranges;

	public GameObject[] telegraphs;

	public GameObject[] fxInstance;

	public GameObject fxHit;

	public ScalingValue dmgFactor;

	public Knockback Knockback;

	public DewAnimationClip clip;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		for (int i = 0; i < ranges.Length; i++)
		{
			DewCollider range = ranges[i];
			GameObject effect = telegraphs[i];
			GameObject instance = fxInstance[i];
			range.transform.position = info.point;
			FxPlayNetworked(effect, info.point, Quaternion.identity);
			if (i == 0)
			{
				info.caster.Animation.PlayAbilityAnimation(clip);
			}
			yield return new SI.WaitForSeconds(delay + addedDelay * (float)i);
			FxPlayNetworked(instance, info.point, Quaternion.identity);
			float value = GetValue(dmgFactor);
			value += value * 0.25f * (float)i;
			ListReturnHandle<Entity> handle;
			foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
			{
				FxPlayNewNetworked(fxHit, entity);
				CreateDamage(DamageData.SourceType.Default, value).SetOriginPosition(info.point).Dispatch(entity);
				Knockback.ApplyWithOrigin(info.point, entity);
			}
			handle.Return();
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}

using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossObliviax_Artillery : AbilityInstance
{
	public float delay;

	public float interval;

	public int count;

	public Vector2 artilleryRange;

	public float artilleryRandomMag;

	public float targetingChance;

	public DewAnimationClip clip;

	public GameObject fxCast;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		CreateBasicEffect(info.caster, new UnstoppableEffect(), 10f, "ObliviaxUnstoppable").DestroyOnDestroy(this);
		FxPlayNetworked(fxCast, info.caster);
		float totalTime = interval * (float)count;
		info.caster.Control.StartDaze(totalTime);
		info.caster.Animation.PlayAbilityAnimation(clip);
		float startTime = Time.time;
		int doneShots = 0;
		while (true)
		{
			float num = Time.time - startTime;
			int num2 = Mathf.RoundToInt(Mathf.Lerp(0f, count, num / totalTime));
			int num3 = num2 - doneShots;
			if (num3 <= 0 && num > totalTime)
			{
				break;
			}
			doneShots = num2;
			for (int i = 0; i < num3; i++)
			{
				Vector3 point = info.caster.agentPosition + Random.insideUnitCircle.ToXZ() * Random.Range(artilleryRange.x, artilleryRange.y);
				List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, info.caster.agentPosition, artilleryRange.y, tvDefaultHarmfulEffectTargets);
				if (Random.value < targetingChance && list.Count > 0)
				{
					Entity target = list[Random.Range(0, list.Count)];
					point = AbilityTrigger.PredictPoint_Simple(info.caster, Random.Range(0.5f, 1f), target, delay) + Random.insideUnitCircle.ToXZ() * artilleryRandomMag;
				}
				handle.Return();
				CreateAbilityInstance(point, null, new CastInfo(info.caster, point), (Ai_Mon_Special_BossObliviax_Artillery_Instance b) =>
				{
					b.delay = delay;
				});
			}
			yield return null;
		}
		FxStopNetworked(fxCast);
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && fxCast != null)
		{
			FxStopNetworked(fxCast);
		}
	}

	private void MirrorProcessed()
	{
	}
}

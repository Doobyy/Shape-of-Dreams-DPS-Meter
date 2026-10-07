using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossLightElemental_Lightning_Spawner : AbilityInstance
{
	public DewCollider targetRange;

	public int fallCount;

	public float fallInterval;

	public float randomMagnitude;

	public float fallPredictionDelay;

	public float postDelay;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		BossMonster.RevealStealthedBeforeSpecialAttack();
		DestroyOnDeath(info.caster);
		float duration = postDelay + (float)fallCount * fallInterval;
		info.caster.Control.StartDaze(duration);
		Vector3 fallPos = info.caster.position + ((Component)(object)info.caster).transform.forward * 3f;
		for (int i = 0; i < fallCount; i++)
		{
			List<Entity> entities = targetRange.GetEntities(out var handle, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
			{
				includeUncollidable = true
			});
			if (entities.Count > 0)
			{
				Entity target = entities[Random.Range(0, entities.Count)];
				fallPos = AbilityTrigger.PredictPoint_Simple(info.caster, Random.value, target, fallPredictionDelay) + Random.insideUnitSphere.Flattened() * randomMagnitude;
				fallPos = Dew.GetPositionOnGround(fallPos);
			}
			handle.Return();
			CreateAbilityInstance<Ai_Mon_Special_BossLightElemental_Lightning>(fallPos, null, new CastInfo(info.caster));
			yield return new SI.WaitForSeconds(fallInterval);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}

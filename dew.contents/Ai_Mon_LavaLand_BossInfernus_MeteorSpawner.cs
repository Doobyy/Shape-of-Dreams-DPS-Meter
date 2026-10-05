using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_LavaLand_BossInfernus_MeteorSpawner : AbilityInstance
{
	public float targetedMeteorChance;

	public float interval;

	[NonSerialized]
	public int count;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		float delay = DewResources.GetByType<Ai_Mon_LavaLand_BossInfernus_Meteor>(default(ResourceLoadSettings)).damageDelay;
		if (info.caster.section == null)
		{
			Destroy();
			yield break;
		}
		for (int j = 0; j < count; j++)
		{
			Vector3 vector;
			if (UnityEngine.Random.value < targetedMeteorChance)
			{
				Hero target = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
				vector = AbilityTrigger.PredictPoint_Simple(info.caster, UnityEngine.Random.Range(0.25f, 1f), target, delay) + UnityEngine.Random.insideUnitSphere.Flattened() * 1.5f;
			}
			else
			{
				if (!(info.caster.section != null))
				{
					continue;
				}
				vector = info.caster.section.GetAnyRandomNode() + UnityEngine.Random.insideUnitSphere.Flattened() * 1.5f;
			}
			vector = Dew.GetPositionOnGround(vector);
			CreateAbilityInstance<Ai_Mon_LavaLand_BossInfernus_Meteor>(vector, Quaternion.identity, new CastInfo(info.caster, vector));
			yield return new SI.WaitForSeconds(interval);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}

using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Gem_C_Sharp_ArrowSpawner : StatusEffect
{
	public int shotArrows = 4;

	public float preDelay = 0.2f;

	public float shootInterval = 0.5f;

	public float radius = 7.5f;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		yield return new SI.WaitForSeconds(preDelay);
		for (int i = 0; i < shotArrows; i++)
		{
			List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, info.caster.agentPosition, radius, tvDefaultHarmfulEffectTargets);
			if (list.Count > 0)
			{
				CreateAbilityInstance<Ai_Gem_C_Sharp_Arrow>(info.caster.position, null, new CastInfo(info.caster, list[Random.Range(0, list.Count)]));
			}
			handle.Return();
			gem.NotifyUse();
			yield return new SI.WaitForSeconds(shootInterval);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}

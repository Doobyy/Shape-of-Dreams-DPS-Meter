using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossErebos_SpawnTormentor_Slow : AbilityInstance
{
	public float duration;

	public float radius;

	public GameObject fxInstance;

	public GameObject fxEnd;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			FxPlayNetworked(fxInstance, ((Component)(object)this).transform.position, Quaternion.identity);
			yield return new SI.WaitForSeconds(duration);
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxInstance);
			FxPlayNewNetworked(fxEnd, ((Component)(object)this).transform.position, Quaternion.identity);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, ((Component)(object)this).transform.position, radius, tvDefaultHarmfulEffectTargets))
		{
			if (!(item is Monster) && !item.Status.hasCrowdControlImmunity)
			{
				if (item.Status.TryGetStatusEffect<Se_Mon_Special_BossErebos_SpawnTormentor_Slow>(out var effect))
				{
					effect.ResetTimer();
				}
				else
				{
					CreateStatusEffect<Se_Mon_Special_BossErebos_SpawnTormentor_Slow>(item);
				}
			}
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}

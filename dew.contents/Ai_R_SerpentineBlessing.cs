using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Ai_R_SerpentineBlessing : AbilityInstance
{
	[NonSerialized]
	public bool dontDoSecondTarget;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		dontDoSecondTarget = false;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		Entity entity = null;
		if ((UnityEngine.Object)(object)info.target == (UnityEngine.Object)(object)info.caster)
		{
			List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, info.caster.agentPosition, 10f, tvDefaultUsefulEffectTargets, new CollisionCheckSettings
			{
				sortComparer = CollisionCheckSettings.DistanceFromCenter
			});
			for (int i = 0; i < list.Count; i++)
			{
				Entity entity2 = list[i];
				if (!((UnityEngine.Object)(object)entity2 == (UnityEngine.Object)(object)info.target))
				{
					entity = entity2;
					break;
				}
			}
			handle.Return();
		}
		else
		{
			entity = info.caster;
		}
		if (dontDoSecondTarget)
		{
			entity = null;
		}
		StatusEffect buff0 = ApplyBuff(info.target);
		StatusEffect buff1 = null;
		if ((UnityEngine.Object)(object)entity != null)
		{
			buff1 = ApplyBuff(entity);
		}
		while (!buff0.IsNullOrInactive() || !buff1.IsNullOrInactive())
		{
			yield return new SI.WaitForSeconds(0.25f);
		}
		Destroy();
	}

	private StatusEffect ApplyBuff(Entity target)
	{
		List<StatusEffect> list = (from s in target.Status.statusEffects
			where s is Se_R_SerpentineBlessing_Buff
			orderby s.creationTime
			select s).ToList();
		while (list.Count > 4)
		{
			list[0].Destroy();
			list.RemoveAt(0);
		}
		return CreateStatusEffect<Se_R_SerpentineBlessing_Buff>(target);
	}

	private void MirrorProcessed()
	{
	}
}

using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossPolaris_Holy_Purgatory_Spawner : AbilityInstance
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		Ai_Mon_Special_BossPolaris_Holy_Purgatory_Instance byType = DewResources.GetByType<Ai_Mon_Special_BossPolaris_Holy_Purgatory_Instance>(default(ResourceLoadSettings));
		List<Vector3> list = DewPool.GetList(out ListReturnHandle<Vector3> handle);
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			if (!allHero.IsNullInactiveDeadOrKnockedOut())
			{
				Vector3 item = AbilityTrigger.PredictPoint_Simple(info.caster, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), allHero, byType.damageDelay);
				list.Add(item);
			}
		}
		int num = 1;
		Mon_Special_BossPolaris mon_Special_BossPolaris = (Mon_Special_BossPolaris)info.caster;
		if (mon_Special_BossPolaris.Holy_CanCastPhase2Abilities())
		{
			num += 2;
		}
		if (mon_Special_BossPolaris.Holy_CanCastPhase3Abilities())
		{
			num += 3;
		}
		list.Shuffle();
		while (list.Count > num)
		{
			list.RemoveAt(0);
		}
		while (list.Count < num)
		{
			Vector3 randomPathablePosition = SingletonBehaviour<Room_BossArena>.instance.GetRandomPathablePosition();
			list.Add(randomPathablePosition);
		}
		for (int i = 0; i < 10; i++)
		{
			for (int j = 0; j < list.Count; j++)
			{
				Vector3 vector = info.caster.agentPosition;
				float num2 = Vector3.Distance(list[j], info.caster.agentPosition);
				for (int k = 0; k < list.Count; k++)
				{
					if (j != k)
					{
						float num3 = Vector3.Distance(list[j], list[k]);
						if (!(num3 >= num2))
						{
							num2 = num3;
							vector = list[k];
						}
					}
				}
				if (!(num2 > 5f))
				{
					list[j] = Dew.GetValidAgentDestination_Closest(list[j], list[j] + (list[j] - vector).normalized * 1.5f);
				}
			}
		}
		foreach (Vector3 item2 in list)
		{
			CreateAbilityInstance<Ai_Mon_Special_BossPolaris_Holy_Purgatory_Instance>(item2, null, new CastInfo(info.caster));
		}
		handle.Return();
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}

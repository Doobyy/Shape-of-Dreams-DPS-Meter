using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Mon_Sky_LittleBaam : Monster, IPrewarmMonsterContributor
{
	[HideInInspector]
	public bool isSummoned;

	public int summonCount = 2;

	public float summonMinDis = 1f;

	public float summonDelay = 0.3f;

	public void ContributeMonsterPrewarm(Dictionary<Monster, int> counts, int instanceCount)
	{
		counts.TryGetValue(this, out var value);
		counts[this] = value + summonCount * instanceCount;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && !isSummoned)
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(summonDelay);
			List<Vector3> spawnedPos = new List<Vector3> { ((Component)(object)this).transform.position };
			int num = 0;
			int loopCount = 0;
			for (int i = 0; i < summonCount; i++)
			{
				if (SingletonDewNetworkBehaviour<Room>.instance.didClearRoom)
				{
					break;
				}
				bool flag = true;
				loopCount++;
				Vector3 vector = Random.insideUnitCircle.ToXZ() * 1.5f;
				Vector3 vector2 = spawnedPos[num] + vector;
				vector2 = Dew.GetValidAgentDestination_Closest(vector2, vector2 + vector);
				for (int j = 0; j < spawnedPos.Count; j++)
				{
					if (loopCount > 5)
					{
						loopCount = 0;
						break;
					}
					Vector3 b = spawnedPos[j];
					if (Vector3.Distance(vector2, b) < summonMinDis)
					{
						i--;
						flag = false;
						break;
					}
				}
				if (flag)
				{
					spawnedPos.Add(vector2);
					num++;
					Hero hero = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
					Mon_Sky_LittleBaam mon_Sky_LittleBaam = Dew.SpawnEntity(vector2, Quaternion.LookRotation(hero.GetAIPosition(this) - vector2), null, new CastInfo(this).caster.owner, NetworkedManagerBase<GameManager>.instance.ambientLevel, (Mon_Sky_LittleBaam s) =>
					{
						s.populationCost = 0f;
						s.isSummoned = true;
					});
					if ((Object)(object)mon_Sky_LittleBaam != null)
					{
						mon_Sky_LittleBaam.InvalidatePoolReuseEverywhere();
					}
					yield return new WaitForSeconds(summonDelay);
				}
			}
		}
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if ((Object)(object)context.targetEnemy == null)
		{
			Hero hero = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
			if ((Object)(object)hero != null && !hero.Status.isUndetectableByNonAllies && hero.GetRelation(this) == EntityRelation.Enemy)
			{
				AI.Aggro(hero);
			}
		}
		else if (!AI.Helper_IsTargetInRangeOfAttack() && AI.Helper_CanBeCast<At_Mon_Sky_LittleBaam_Reposition>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Sky_LittleBaam_Reposition>();
		}
		else
		{
			AI.Helper_ChaseTarget();
		}
	}

	private void MirrorProcessed()
	{
	}
}

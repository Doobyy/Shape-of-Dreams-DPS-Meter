using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Mon_Forest_SpiderWarrior : Monster, ISpawnableAsMiniBoss, IPrewarmMiniBossContributor
{
	public float jumpRandomPositionMag = 3f;

	public float jumpBehindOfTargetDistance = 2f;

	public float jumpChance = 0.25f;

	public void ContributeMiniBossPrewarm(Dictionary<Monster, int> counts, int instanceCount)
	{
		Monster miniBossSpawnedMonster = GetMiniBossSpawnedMonster();
		if (!((UnityEngine.Object)(object)miniBossSpawnedMonster == null))
		{
			Ai_Mon_Forest_SpiderWarrior_SpawnScarabs byType = DewResources.GetByType<Ai_Mon_Forest_SpiderWarrior_SpawnScarabs>(default(ResourceLoadSettings));
			if (!((UnityEngine.Object)(object)byType == null))
			{
				int num = 3 * byType.numberOfScarabs * instanceCount;
				counts.TryGetValue(miniBossSpawnedMonster, out var value);
				counts[miniBossSpawnedMonster] = value + num;
			}
		}
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((UnityEngine.Object)(object)context.targetEnemy == null))
		{
			if (UnityEngine.Random.value < jumpChance && !AI.Helper_IsTargetInRangeOfAttack() && AI.Helper_CanBeCast<At_Mon_Forest_SpiderWarrior_Jump>())
			{
				Vector3 vector = context.targetEnemy.GetAIPosition(this) - position;
				Vector3 point = position + vector.normalized * (vector.magnitude + jumpBehindOfTargetDistance) + UnityEngine.Random.insideUnitCircle.ToXZ() * jumpRandomPositionMag;
				AI.Helper_CastAbility<At_Mon_Forest_SpiderWarrior_Jump>(new CastInfo(this, point));
			}
			else
			{
				AI.Helper_ChaseTarget();
			}
		}
	}

	public void OnBeforeSpawnAsMiniBoss()
	{
	}

	public void OnCreateAsMiniBoss()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		ISpawnableAsMiniBoss.GiveGenericMiniBossBonus(this);
		RefValue<float> last = new RefValue<float>(1f);
		float[] spawnThresholds = new float[3] { 0.25f, 0.5f, 0.75f };
		EntityEvent_OnTakeDamage += (Action<EventInfoDamage>)((EventInfoDamage info) =>
		{
			float num = normalizedHealth;
			if (!((float)last < num))
			{
				float[] array = spawnThresholds;
				foreach (float num2 in array)
				{
					if (num < num2 && (float)last >= num2)
					{
						CreateAbilityInstance(position, Quaternion.identity, new CastInfo(this), (Ai_Mon_Forest_SpiderWarrior_SpawnScarabs se) =>
						{
							se.spawnedMonster = GetMiniBossSpawnedMonster();
						});
					}
				}
				last.value = num;
			}
		});
	}

	protected override void OnDeath(EventInfoKill info)
	{
		base.OnDeath(info);
		if (!((NetworkBehaviour)this).isServer || type != MonsterType.MiniBoss)
		{
			return;
		}
		Actor[] array = children.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] is Monster monster && !monster.IsNullInactiveDeadOrKnockedOut())
			{
				monster.Kill();
			}
		}
	}

	protected virtual Monster GetMiniBossSpawnedMonster()
	{
		return DewResources.GetByType<Mon_Forest_Scarab>(default(ResourceLoadSettings));
	}

	public override void LoadEntityModelLocal()
	{
		if (DewSave.profileMain.gameplay.enableArachnophobia)
		{
			Visual.LoadModelLocal(null);
		}
		else
		{
			base.LoadEntityModelLocal();
		}
	}

	private void MirrorProcessed()
	{
	}
}

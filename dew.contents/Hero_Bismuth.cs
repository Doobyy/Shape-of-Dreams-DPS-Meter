using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Hero_Bismuth : Hero
{
	private static AbilityTargetValidator _hittableEnemy = new AbilityTargetValidator
	{
		targets = EntityRelation.Enemy
	};

	private static AbilityTargetValidator _hittableNeutral = new AbilityTargetValidator
	{
		targets = EntityRelation.Neutral
	};

	private bool? _hasSameTravelerMemory;

	public Hero_Bismuth_BookController book { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		book = ((Component)(object)this).GetComponent<Hero_Bismuth_BookController>();
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		Skill.ClientHeroEvent_OnSkillEquip += (Action<SkillTrigger>)((SkillTrigger _) =>
		{
			_hasSameTravelerMemory = null;
		});
		Skill.ClientHeroEvent_OnSkillUnequip += (Action<SkillTrigger>)((SkillTrigger _) =>
		{
			_hasSameTravelerMemory = null;
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		UnityEngine.Object.Destroy((UnityEngine.Object)(object)book);
	}

	[Server]
	public void SpendAttack()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Hero_Bismuth::SpendAttack()' called when server was not active");
		}
		else if (Status.HasStatusEffect<Se_D_PrismaticEyes>())
		{
			AbilityTrigger attackAbility = Ability.attackAbility;
			attackAbility.SetChargeAll(0);
			attackAbility.SetCooldownTimeAll(attackAbility.currentConfigMaxCooldownTime, scaled: false);
		}
	}

	public static List<Entity> GetTargetEntities(out ListReturnHandle<Entity> handle, Entity self, bool canBeNeutral, float range)
	{
		List<Entity> list = DewPool.GetList(out handle);
		List<Entity> list2 = DewPhysics.OverlapCircleAllEntities(out var handle2, self.position, range, _hittableEnemy, self, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.DistanceFromCenter
		});
		if (list2.Count > 0)
		{
			foreach (Entity item in list2)
			{
				list.Add(item);
			}
			handle2.Return();
			return list;
		}
		handle2.Return();
		list2 = DewPhysics.OverlapCircleAllEntities(out handle2, self.position, range, _hittableNeutral, self, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.DistanceFromCenter
		});
		if (list2.Count > 0)
		{
			foreach (Entity item2 in list2)
			{
				if (canBeNeutral || !item2.AI.excludeFromAutoTargeting)
				{
					list.Add(item2);
				}
			}
			handle2.Return();
			return list;
		}
		handle2.Return();
		return list;
	}

	public bool HasSameTravelerMemory()
	{
		if (_hasSameTravelerMemory.HasValue)
		{
			return _hasSameTravelerMemory.Value;
		}
		bool hasSame = false;
		List<Type> seenTravelerMemoryTypes = DewPool.GetList(out ListReturnHandle<Type> handle);
		Check(Skill.Q);
		Check(Skill.W);
		Check(Skill.E);
		Check(Skill.R);
		handle.Return();
		_hasSameTravelerMemory = hasSame;
		return hasSame;
		void Check(SkillTrigger st)
		{
			if (!hasSame && (bool)(UnityEngine.Object)(object)st && st.rarity == Rarity.Character)
			{
				Type type = ((object)st).GetType();
				if (seenTravelerMemoryTypes.Contains(type))
				{
					hasSame = true;
				}
				else
				{
					seenTravelerMemoryTypes.Add(type);
				}
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}

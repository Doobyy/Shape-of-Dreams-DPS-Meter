using System;
using Mirror;
using UnityEngine;

public class Se_Star_Mist_L_AddedDodgeWithPenalty : StarEffect
{
	public StarScalingValue takenDamageAmpPenalty;

	private SkillBonus _bonus;

	public override Type heroType => typeof(Hero_Mist);

	public override bool isMovementSkillType => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus = new SkillBonus
			{
				addedCharge = 1
			};
			hero.Skill.Movement.AddSkillBonus(_bonus);
			victim.takenDamageProcessor.Add(Processor);
		}
	}

	private void Processor(ref DamageData data, Actor actor, Entity target)
	{
		Entity entity = actor.firstEntity;
		if (!((UnityEngine.Object)(object)entity == null) && victim.CheckEnemyOrNeutral(entity) && !data.IsAmountModifiedBy(this))
		{
			data.SetAmountModifiedBy(this);
			data.ApplyAmplification(GetValue(takenDamageAmpPenalty));
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if (_bonus != null)
			{
				_bonus.Stop();
				_bonus = null;
			}
			if ((UnityEngine.Object)(object)victim != null)
			{
				victim.takenDamageProcessor.Remove(Processor);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}

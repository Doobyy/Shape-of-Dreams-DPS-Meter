using Mirror;
using UnityEngine;

public class Mon_LavaLand_InfernoSpider : Monster, ISpawnableAsMiniBoss
{
	public float jumpChance = 0.25f;

	public float jumpRandomPositionMag = 3f;

	public float jumpBehindOfTargetDistance = 2f;

	public override void OnStartServer()
	{
		base.OnStartServer();
		AddData(new LavaLand_Lava.Ad_Lava
		{
			isImmune = true
		});
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((Object)(object)context.targetEnemy == null))
		{
			if (!AI.Helper_IsTargetInRangeOfAttack() && AI.Helper_CanBeCast<At_Mon_LavaLand_InfernoSpider_LavaAtk>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_LavaLand_InfernoSpider_LavaAtk>();
			}
			else if (Random.value < jumpChance && !AI.Helper_IsTargetInRangeOfAttack() && AI.Helper_CanBeCast<At_Mon_Forest_SpiderWarrior_Jump>())
			{
				Vector3 vector = context.targetEnemy.position - position;
				Vector3 point = position + vector.normalized * (vector.magnitude + jumpBehindOfTargetDistance) + Random.insideUnitCircle.ToXZ() * jumpRandomPositionMag;
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
		if (((NetworkBehaviour)this).isServer)
		{
			ISpawnableAsMiniBoss.GiveGenericMiniBossBonus(this);
			At_Mon_LavaLand_InfernoSpider_LavaAtk ability = Ability.GetAbility<At_Mon_LavaLand_InfernoSpider_LavaAtk>();
			ability.spawnAdditionalProjectile = true;
			ability.configs[0].castMethod._range += 5f;
			Status.AddStatBonus(new StatBonus
			{
				attackSpeedPercentage = 15f
			});
		}
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

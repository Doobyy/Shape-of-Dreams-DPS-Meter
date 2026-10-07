using Mirror;
using UnityEngine;

public class Mon_Forest_SpiderSpitter : Monster, ISpawnableAsMiniBoss
{
	public float triggerBackdashDist = 5f;

	public float backdashChancePerSecond = 0.5f;

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if ((Object)(object)context.targetEnemy == null)
		{
			return;
		}
		if (Random.value < backdashChancePerSecond * context.deltaTime && AI.Helper_CanBeCast<At_Mon_Forest_SpiderSpitter_Backdash>() && Vector3.Distance(position, context.targetEnemy.position) < triggerBackdashDist)
		{
			AI.Helper_CastAbility<At_Mon_Forest_SpiderSpitter_Backdash>(new CastInfo(this, context.targetEnemy));
		}
		else if (AI.Helper_IsTargetInRange<At_Mon_Forest_SpiderSpitter_Melee>())
		{
			if (AI.Helper_CanBeCast<At_Mon_Forest_SpiderSpitter_Melee>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Forest_SpiderSpitter_Melee>();
			}
		}
		else
		{
			AI.Helper_ChaseTarget();
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
			At_Mon_Forest_SpiderSpitter_Atk ability = Ability.GetAbility<At_Mon_Forest_SpiderSpitter_Atk>();
			ability.spawnAdditionalProjectile = true;
			ability.configs[0].castMethod._range += 5f;
			Ability.GetAbility<At_Mon_Forest_SpiderSpitter_Melee>().configs[0].maxCharges = 0;
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

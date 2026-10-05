using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossPolaris_Monster_Atk_Spawner : AbilityInstance
{
	public float secondAtkDelay = 0.25f;

	public float secondAtkRotateDuration = 1f;

	public float secondAtkMaxDistance = 7f;

	public bool restartAttackCooldownOnSecondAtk = true;

	private float attackSpeed
	{
		get
		{
			if (!info.caster.IsNullInactiveDeadOrKnockedOut())
			{
				return info.caster.Status.attackSpeedMultiplier;
			}
			return 1f;
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		CreateAbilityInstance(position, info.rotation, info, (Ai_Mon_Special_BossPolaris_Monster_Atk_FirstAtk ai) =>
		{
			ai.onChannelCanceled += new Action(DestroyIfActive);
			ai.onChannelCompleted += (Action)(() =>
			{
				ai.DestroyIfActive();
				if (isActive)
				{
					((MonoBehaviour)(object)this).StartCoroutine(Routine());
				}
			});
		});
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(secondAtkDelay / attackSpeed);
			TryCastSecondAtk();
			DestroyIfActive();
		}
	}

	private void TryCastSecondAtk()
	{
		if (info.caster.IsNullInactiveDeadOrKnockedOut() || info.caster.Control.IsActionBlocked(EntityControl.BlockableAction.Ability) != EntityControl.BlockStatus.Allowed || info.caster.Control.isDisplacing || info.caster.Status.hasStun)
		{
			return;
		}
		float num = 2f * info.caster.Status.missingHealth / info.caster.maxHealth * NetworkedManagerBase<GameManager>.instance.GetSpecialSkillChanceMultiplier();
		if (info.caster.Status.HasStatusEffect<Se_Mon_Special_BossPolaris_Monster_PizzaLightning_Rage>())
		{
			num *= 2f;
		}
		if (UnityEngine.Random.value >= num)
		{
			return;
		}
		Hero closestAliveHero = Dew.GetClosestAliveHero(info.caster.agentPosition, fallbackToDead: true, info.caster);
		if (!closestAliveHero.IsNullInactiveDeadOrKnockedOut() && !(Vector3.Distance(closestAliveHero.GetAIAgentPosition(info.caster), info.caster.agentPosition) >= secondAtkMaxDistance))
		{
			Quaternion value = Quaternion.LookRotation(closestAliveHero.GetAIAgentPosition(info.caster) - info.caster.agentPosition).Flattened();
			info.caster.Control.Rotate(value, immediately: false, secondAtkRotateDuration / attackSpeed);
			CreateAbilityInstance<Ai_Mon_Special_BossPolaris_Monster_Atk_SecondAtk>(position, value, new CastInfo(info.caster, value.eulerAngles.y));
			if (restartAttackCooldownOnSecondAtk && parentActor is AbilityTrigger abilityTrigger)
			{
				abilityTrigger.SetCooldownTimeAll(abilityTrigger.GetMaxCooldownTime(abilityTrigger.currentConfigIndex, scaled: false), scaled: false);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}

using System.Collections;
using Mirror;
using UnityEngine;

public class St_L_HerosReturn : SkillTrigger
{
	public ScalingValue deadHealAmount;

	public ScalingValue aliveHealAmount;

	public ScalingValue aliveAdGain;

	public ScalingValue aliveApGain;

	public float reviveInvulDuration = 3f;

	public GameObject fxAutoCast;

	public float deathAfterDelay;

	public DewAnimationClip reviveStartClip;

	public DewAnimationClip reviveEndClip;

	public float reviveDuration;

	public float postDelay;

	public GameObject fxCastStart;

	public GameObject fxCastComplete;

	public GameObject fxBuff;

	private AbilityLockHandle _handle;

	private ActorRef<Se_L_HerosReturn_Interrupt> _interrupt;

	private bool _isAutoCasting;

	protected override void OnEquip(Entity newOwner)
	{
		base.OnEquip(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			_interrupt = CreateStatusEffect<Se_L_HerosReturn_Interrupt>(newOwner, new CastInfo(newOwner));
		}
	}

	protected override void OnUnequip(Entity formerOwner)
	{
		base.OnUnequip(formerOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			if (!_interrupt.IsNullOrInactive())
			{
				_interrupt.Get().Destroy();
			}
			_interrupt = null;
		}
	}

	public override void OnCastStart(int configIndex, CastInfo info)
	{
		base.OnCastStart(configIndex, info);
		float num = currentConfig.channel.duration * GetChannelDurationMultiplier();
		float num2 = currentConfig.postDelay;
		CreateBasicEffect(owner, new InvulnerableEffect(), num + num2);
		FxPlayNetworked(fxCastStart, owner);
	}

	public override AbilityInstance OnCastComplete(int configIndex, CastInfo info)
	{
		AbilityInstance result = base.OnCastComplete(configIndex, info);
		if (((NetworkBehaviour)this).isServer)
		{
			FxPlayNetworked(fxCastComplete, owner);
			ExecuteCastCompleteRoutines(treatSelfAsDead: false);
		}
		return result;
	}

	[Server]
	public void TriggerAutoCast()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void St_L_HerosReturn::TriggerAutoCast()' called when server was not active");
		}
		else if (!this.IsNullOrInactive() && !_isAutoCasting)
		{
			_isAutoCasting = true;
			_handle = owner.Ability.GetNewAbilityLockHandle();
			_handle.LockAllActiveMainSkillsCast();
			_handle.LockAllMainSkillsEdit();
			((MonoBehaviour)(object)this).StartCoroutine(AutoCastRoutine());
		}
	}

	private IEnumerator AutoCastRoutine()
	{
		float duration = deathAfterDelay + reviveDuration + postDelay;
		owner.Control.StartDaze(duration);
		CreateBasicEffect(owner, new InvulnerableEffect(), duration);
		owner.Animation.StartDeathAnimation();
		FxPlayNetworked(fxAutoCast, owner);
		yield return new WaitForSeconds(deathAfterDelay - reviveDuration);
		FxStopNetworked(fxAutoCast);
		owner.Animation.StopDeathAnimation();
		owner.Animation.PlayAbilityAnimation(reviveStartClip);
		yield return new WaitForSeconds(reviveDuration);
		FxPlayNetworked(fxCastComplete, owner);
		ExecuteCastCompleteRoutines(treatSelfAsDead: true);
		owner.Animation.PlayAbilityAnimation(reviveEndClip);
	}

	[Server]
	private void ExecuteCastCompleteRoutines(bool treatSelfAsDead)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void St_L_HerosReturn::ExecuteCastCompleteRoutines(System.Boolean)' called when server was not active");
			return;
		}
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			if ((Object)(object)allHero == null)
			{
				continue;
			}
			if (allHero.isKnockedOut || (treatSelfAsDead && (Object)(object)allHero == (Object)(object)owner))
			{
				if (allHero.Status.TryGetStatusEffect<Se_HeroKnockedOut>(out var effect))
				{
					effect.Revive(0.01f);
					if ((Object)(object)allHero != (Object)(object)owner)
					{
						FxPlayNewNetworked(fxBuff, allHero);
						Vector3 end = owner.agentPosition + Random.onUnitSphere * 4f;
						end = Dew.GetValidAgentDestination_LinearSweep(owner.agentPosition, end);
						Teleport(allHero, end);
					}
				}
				Heal(GetValue(deadHealAmount)).Dispatch(allHero);
				allHero.CreateBasicEffect(allHero, new InvulnerableEffect(), reviveInvulDuration);
				continue;
			}
			if ((Object)(object)allHero != (Object)(object)owner)
			{
				FxPlayNewNetworked(fxBuff, allHero);
			}
			Heal(GetValue(aliveHealAmount)).Dispatch(allHero);
			if (allHero.Status.TryGetStatusEffect<Se_L_HerosReturn_Buff>(out var effect2))
			{
				effect2.bonus.attackDamageFlat += GetValue(aliveAdGain);
				effect2.bonus.abilityPowerFlat += GetValue(aliveApGain);
				continue;
			}
			allHero.CreateStatusEffect(allHero, new CastInfo(owner), (Se_L_HerosReturn_Buff reward) =>
			{
				reward.bonus = new StatBonus
				{
					attackDamageFlat = GetValue(aliveAdGain),
					abilityPowerFlat = GetValue(aliveApGain)
				};
			});
		}
		ConsumeMemory();
	}

	[Server]
	private void ConsumeMemory()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void St_L_HerosReturn::ConsumeMemory()' called when server was not active");
			return;
		}
		SkillTrigger skillTrigger = owner.Skill.UnequipSkill(skillType, owner.position, ignoreCanReplace: true);
		if (!skillTrigger.IsNullOrInactive())
		{
			skillTrigger.Destroy();
		}
		ReleaseAbilityLock();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			ReleaseAbilityLock();
			_isAutoCasting = false;
			_interrupt = null;
		}
	}

	private void ReleaseAbilityLock()
	{
		if (_handle != null)
		{
			_handle.Stop();
			_handle = null;
		}
	}

	private void MirrorProcessed()
	{
	}
}

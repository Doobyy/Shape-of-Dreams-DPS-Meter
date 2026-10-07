using System;
using Mirror;
using UnityEngine;

public class Se_D_IceColdPresence : StatusEffect
{
	private string _originalSkill;

	private int _maxCharges;

	private ActorRef<StatusEffect> _se;

	private SkillTrigger _skill;

	private Action<EventInfoCast> _onCastComplete;

	private St_D_IceColdPresence _trigger => firstTrigger as St_D_IceColdPresence;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		Entity entity = victim;
		Hero h = entity as Hero;
		if (h == null || (UnityEngine.Object)(object)_trigger == null)
		{
			return;
		}
		_originalSkill = _trigger.originalSkill;
		_maxCharges = _trigger.maxCharges;
		SkillTrigger skill = h.Skill.Movement;
		int num = 1;
		if (_originalSkill == null)
		{
			_originalSkill = DewPersistence.ToJson(new DewPersistence.GeneralData().Capture(skill));
			_trigger.originalSkill = _originalSkill;
			num = skill.level;
			_maxCharges = skill.configs[0].maxCharges;
			_trigger.maxCharges = _maxCharges;
			skill.Destroy();
			St_M_IceColdPresence skill2 = Dew.CreateSkillTrigger<St_M_IceColdPresence>(h.agentPosition, num, h.owner);
			h.Skill.EquipSkill(HeroSkillLocation.Movement, skill2, ignoreCanReplace: true);
		}
		Dew.CallDelayed(() =>
		{
			skill = h.Skill.Movement;
			skill.LockCooldown(0);
			skill.configs[0].canReceiveCooldownReduction = false;
			skill.configs[0].maxCharges = _maxCharges;
			skill.configs[0].addedCharges = 99;
			skill.SetCharge(0, _maxCharges);
			_skill = skill;
			_onCastComplete = (EventInfoCast cast) =>
			{
				_se = CreateBasicEffect(victim, new InvulnerableEffect(), float.PositiveInfinity, "invul_icecold");
				cast.instance.ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
				{
					CreateAbilityInstance<Ai_D_IceColdPresence_AOE>(info.caster.agentPosition, null, new CastInfo(info.caster, info.caster.agentPosition)).ClientActorEvent_OnCreate += (Action<Actor>)((Actor actor) =>
					{
						if (!_se.IsNullOrInactive())
						{
							_se.Get().Destroy();
						}
					});
				});
				if (skill.currentCharges[0] == 0)
				{
					Dew.CallDelayed(DestroyIfActive);
				}
			};
			skill.TriggerEvent_OnCastComplete += _onCastComplete;
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if ((UnityEngine.Object)(object)_skill != null)
		{
			_skill.TriggerEvent_OnCastComplete -= _onCastComplete;
			_skill = null;
			_onCastComplete = null;
		}
		if ((UnityEngine.Object)(object)_trigger == null)
		{
			return;
		}
		_trigger.SetCharge(0, 0);
		string originalSkill = _originalSkill;
		if (originalSkill != null && !victim.IsNullOrInactive() && victim is Hero hero)
		{
			_trigger.originalSkill = null;
			_trigger.maxCharges = 1;
			hero.Skill.Movement.Destroy();
			DewPersistence.GeneralData data = DewPersistence.FromJson<DewPersistence.GeneralData>(originalSkill);
			SkillTrigger originalSkill2 = DewPersistence.CreateActor<SkillTrigger>(data);
			Dew.CallDelayed(() =>
			{
				if (!originalSkill2.IsNullOrInactive())
				{
					originalSkill2.SetCharge(0, 0);
				}
			});
			hero.Skill.EquipSkill(HeroSkillLocation.Movement, originalSkill2, ignoreCanReplace: true);
		}
		if (!_se.IsNullOrInactive())
		{
			_se.Get().Destroy();
		}
		_se = null;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_se = null;
	}

	private void MirrorProcessed()
	{
	}
}

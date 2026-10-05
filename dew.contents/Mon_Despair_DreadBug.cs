using System;
using Mirror;
using UnityEngine;

public class Mon_Despair_DreadBug : Monster
{
	public float minJumpDistance = 3f;

	public float backStepChance = 0.9f;

	private EntityAIContext _context;

	private bool _isDodgeStart;

	private float _dodgeTimer;

	private float _baseBackStepChance;

	protected override void Awake()
	{
		base.Awake();
		_baseBackStepChance = backStepChance;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_isDodgeStart = false;
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd += new Action<Actor>(OnAbilityInstanceCreated);
			backStepChance = _baseBackStepChance * NetworkedManagerBase<GameManager>.instance.GetSpecialSkillChanceMultiplier();
		}
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((UnityEngine.Object)(object)context.targetEnemy == null))
		{
			if ((context.targetEnemy.agentPosition - agentPosition).sqrMagnitude > minJumpDistance && AI.Helper_CanBeCast<At_Mon_Despair_DreadBug_Jump>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Despair_DreadBug_Jump>();
			}
			else
			{
				AI.Helper_ChaseTarget();
			}
		}
	}

	private void OnAbilityInstanceCreated(Actor actor)
	{
		if (((NetworkBehaviour)this).isServer && !_isDodgeStart && !((UnityEngine.Object)(object)AI.context.targetEnemy == null) && !((UnityEngine.Object)(object)AI.context.targetEnemy != (UnityEngine.Object)(object)actor.firstEntity) && !((UnityEngine.Object)(object)actor.firstTrigger == null) && !((UnityEngine.Object)(object)actor.firstTrigger == (UnityEngine.Object)(object)actor.firstEntity.Ability.attackAbility) && (!(actor.firstTrigger is SkillTrigger skillTrigger) || !((UnityEngine.Object)(object)skillTrigger).name.StartsWith("St_M_")) && !(UnityEngine.Random.value >= backStepChance))
		{
			_dodgeTimer = Time.time;
			At_Mon_Despair_DreadBug_JumpBack ability = Ability.GetAbility<At_Mon_Despair_DreadBug_JumpBack>();
			if (!((UnityEngine.Object)(object)ability == null) && ability.CanBeCast())
			{
				Control.Stop();
				Control.Cast(ability, new CastInfo(this, AI.context.targetEnemy));
				_isDodgeStart = true;
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd -= new Action<Actor>(OnAbilityInstanceCreated);
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (((NetworkBehaviour)this).isServer && _isDodgeStart && !(Time.time - _dodgeTimer <= 0.3f))
		{
			_isDodgeStart = false;
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
			Status.AddStatBonus(new StatBonus
			{
				attackSpeedPercentage = 15f
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}

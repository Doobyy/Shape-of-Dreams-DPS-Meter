using Mirror;
using UnityEngine;

public class Ai_Mon_SnowMountain_BossSkoll_Whirlwind : DamageInstance
{
	public DewAnimationClip animSpin;

	public float duration;

	public float selfSlowAmount;

	public float tickInterval;

	public float postDelay;

	public DewAnimationClip animSpinEnd;

	public float spinSpeed;

	private float _lastTickTime;

	private EntityTransformModifier _mod;

	private float _currentAngle;

	private ActorRef<StatusEffect> _slow;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		_mod = info.caster.Visual.GetNewTransformModifier();
		if (((NetworkBehaviour)this).isServer)
		{
			((Component)(object)this).transform.position = info.caster.position;
			CreateBasicEffect(info.caster, new UnstoppableEffect(), 60f).DestroyOnDestroy(this);
			_slow = CreateBasicEffect(info.caster, new SlowEffect
			{
				strength = selfSlowAmount
			}, duration, "whirlwind_slow");
			info.caster.Control.StartChannel(new Channel
			{
				duration = duration,
				blockedActions = (Channel.BlockedAction.Ability | Channel.BlockedAction.Attack),
				onCancel = DestroyIfActive,
				onComplete = DestroyIfActive
			});
			info.caster.Animation.PlayAbilityAnimation(animSpin);
			info.caster.Control.Rotate(Quaternion.identity, immediately: true, duration);
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		((Component)(object)this).transform.position = info.caster.position;
		_currentAngle += spinSpeed * Time.deltaTime;
		_mod.rotation = Quaternion.Euler(0f, _currentAngle, 0f);
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && Time.time - _lastTickTime > tickInterval)
		{
			_lastTickTime = Time.time;
			DoCollisionChecks();
		}
		if (((NetworkBehaviour)this).isServer)
		{
			Hero closestAliveHero = Dew.GetClosestAliveHero(info.caster.agentPosition, fallbackToDead: true, info.caster);
			if ((Object)(object)closestAliveHero != null)
			{
				info.caster.Control.MoveToDestination(closestAliveHero.GetAIAgentPosition(info.caster), immediately: false);
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_mod != null)
		{
			_mod.Stop();
			_mod = null;
		}
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		info.caster.Control.StopOverrideRotation();
		if (!_slow.IsNullOrInactive())
		{
			_slow.Get().Destroy();
		}
		_slow = null;
		if (info.caster.isActive)
		{
			info.caster.Animation.PlayAbilityAnimation(animSpinEnd);
			info.caster.Control.StartDaze(postDelay);
			Hero closestAliveHero = Dew.GetClosestAliveHero(info.caster.agentPosition, fallbackToDead: true, info.caster);
			if ((Object)(object)closestAliveHero != null)
			{
				info.caster.Control.RotateTowards(closestAliveHero.GetAIAgentPosition(info.caster), immediately: true);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}

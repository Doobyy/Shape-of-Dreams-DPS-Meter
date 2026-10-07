using Mirror;
using UnityEngine;

public class Se_U_HerWorld_Blackhole : StatusEffect
{
	public ScalingValue armorAmount;

	public GameObject blackholeRepeatedEffect;

	public float repeatedEffectInterval;

	public AnimationCurve attractStrengthByDist;

	public Vector2 distanceBounds;

	public float duration;

	public float tickInterval = 0.25f;

	public ScalingValue perTickDamage;

	public float tickDamageRadius;

	public DewAnimationClip endClip;

	public float endDaze;

	private float _lastRepeatedEffectTime;

	private float _lastTickTime;

	private AbilityLockHandle _handle;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_lastRepeatedEffectTime = 0f;
		_lastTickTime = 0f;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoDeathInterrupt((EventInfoKill _) =>
			{
				victim.Status.SetHealth(1f);
			}, -9999);
			DoUnstoppable();
			DoSpeed(-30f);
			DoArmorBoost(GetValue(armorAmount));
			SetTimer(duration);
			ShowOnScreenTimer();
			_handle = info.caster.Ability.GetNewAbilityLockHandle();
			_handle.LockAllAbilitiesCast();
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		((Component)(object)this).transform.position = info.caster.position;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (Time.time - _lastRepeatedEffectTime > repeatedEffectInterval)
		{
			_lastRepeatedEffectTime = Time.time;
			FxPlay(blackholeRepeatedEffect);
		}
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			if (!((Object)(object)allEntity == (Object)(object)info.caster) && !allEntity.IsNullInactiveDeadOrKnockedOut() && allEntity.Control.isLocalMovementProcessor && !allEntity.Control.isDisplacing && !allEntity.Status.hasCrowdControlImmunity && info.caster.CheckEnemyOrNeutral(allEntity))
			{
				float time = Mathf.Clamp01((Vector2.Distance(position.ToXY(), allEntity.agentPosition.ToXY()) - distanceBounds.x) / (distanceBounds.y - distanceBounds.x));
				float num = attractStrengthByDist.Evaluate(time);
				allEntity.Control.SetAgentPosition(allEntity.agentPosition + (position - allEntity.agentPosition).normalized * (num * dt));
			}
		}
		if (!((NetworkBehaviour)this).isServer || !(Time.time - _lastTickTime > tickInterval))
		{
			return;
		}
		_lastTickTime = Time.time;
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, info.caster.position, tickDamageRadius, tvDefaultHarmfulEffectTargets))
		{
			if (!((Object)(object)item == (Object)(object)info.caster) && !item.IsNullInactiveDeadOrKnockedOut())
			{
				Damage(perTickDamage, 0.5f).SetElemental(ElementalType.Light).SetAttr(DamageAttribute.DamageOverTime).SetOriginPosition(position)
					.Dispatch(item);
			}
		}
		handle.Return();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if (_handle != null)
			{
				_handle.Stop();
				_handle = null;
			}
			if (!victim.IsNullInactiveDeadOrKnockedOut())
			{
				victim.Animation.PlayAbilityAnimation(endClip);
				victim.Control.StartDaze(endDaze);
				CreateBasicEffect(victim, new SlowEffect
				{
					strength = 50f,
					decay = true
				}, 1f);
			}
			CreateAbilityInstance<Ai_U_HerWorld_Explosion>(info.caster.position, null, new CastInfo(info.caster));
		}
	}

	private void MirrorProcessed()
	{
	}
}

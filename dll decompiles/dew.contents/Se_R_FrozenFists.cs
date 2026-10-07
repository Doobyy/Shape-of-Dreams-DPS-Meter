using Mirror;
using UnityEngine;

public class Se_R_FrozenFists : StatusEffect
{
	public float duration = 6f;

	public ScalingValue hasteAmount;

	public ScalingValue addedDamageOnAttack;

	public ScalingValue armorAmount;

	public GameObject fxAttack;

	[Header("Animations")]
	public AnimationClip idle;

	public AnimationClip runF;

	public AnimationClip runFL;

	public AnimationClip runL;

	public AnimationClip runBL;

	public AnimationClip runB;

	public AnimationClip runBR;

	public AnimationClip runR;

	public AnimationClip runFR;

	public float walkSpeed = 1f;

	private AbilityTrigger.ChangedConfigHandle _handle;

	private float _originalWalkSpeed;

	private AbilityLockHandle _lock;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		victim.Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.Idle, idle);
		victim.Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunForward, runF);
		victim.Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunForwardRight, runFR);
		victim.Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunRight, runR);
		victim.Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunBackwardRight, runBR);
		victim.Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunBackward, runB);
		victim.Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunBackwardLeft, runBL);
		victim.Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunLeft, runL);
		victim.Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunForwardLeft, runFL);
		_originalWalkSpeed = victim.Visual.model.walkAnimationSpeed;
		victim.Visual.model.walkAnimationSpeed = walkSpeed;
		((Hero)victim).isWeaponHolstered = true;
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoAttackOverride<At_R_FrozenFists_Attack>(null);
		DoAttackEmpower((EventInfoAttackEffect effect, int i) =>
		{
			if (!effect.chain.DidReact(this))
			{
				Damage(addedDamageOnAttack).ApplyStrength(effect.strength).SetOriginPosition(victim.agentPosition).Dispatch(effect.victim, effect.chain.New(this));
				FxPlayNewNetworked(fxAttack, effect.victim);
			}
		});
		DoHaste(GetValue(hasteAmount));
		DoArmorBoost(GetValue(armorAmount));
		SetTimer(duration);
		ShowOnScreenTimer();
		_handle = firstTrigger.ChangeConfigTimed(1, 3600f, null, null, setFillAmount: false);
		victim.Status.CalculateStats();
		_lock = victim.Ability.GetNewAbilityLockHandle();
		_lock.LockAllMainSkillsCast();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if ((bool)(Object)(object)victim)
		{
			((Hero)victim).isWeaponHolstered = false;
			EntityModel model = victim.Visual.model;
			if ((bool)model)
			{
				victim.Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.Idle, model.idle);
				victim.Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunForward, model.runForwardClip);
				victim.Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunForwardRight, model.runForwardRightClip);
				victim.Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunRight, model.runRightClip);
				victim.Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunBackwardRight, model.runBackwardRightClip);
				victim.Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunBackward, model.runBackwardClip);
				victim.Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunBackwardLeft, model.runBackwardLeftClip);
				victim.Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunLeft, model.runLeftClip);
				victim.Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunForwardLeft, model.runForwardLeftClip);
				model.walkAnimationSpeed = _originalWalkSpeed;
			}
		}
		if (((NetworkBehaviour)this).isServer)
		{
			if (_lock != null)
			{
				_lock.Stop();
			}
			if ((bool)(Object)(object)victim)
			{
				victim.Status.CalculateStats();
			}
			if (_handle != null && _handle.isActive)
			{
				_handle.Stop();
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}

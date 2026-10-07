using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_E_JusticeGuillotine : AbilityInstance
{
	public float dashDuartion;

	public float destGap = 1f;

	public float postDelay;

	public float descendTime;

	public float maxAmpThreshold;

	public float dmgAmplitude;

	public float critStrengthThreshold;

	public float procCoefficient;

	public ScalingValue dmgFactor;

	public DewCollider range;

	public Knockback Knockback;

	public DewAnimationClip clip;

	public GameObject fxCast;

	public GameObject fxAttack;

	public GameObject fxHit;

	private Vector3 _destination;

	private DamageData _damageData;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		if (info.target.IsNullOrInactive())
		{
			ResetCooldown(firstTrigger);
			yield break;
		}
		DestroyOnDeath(info.caster);
		CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
		CreateBasicEffect(info.caster, new UncollidableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
		float t = info.target.Status.missingHealth / info.target.Status.maxHealth;
		float t2 = info.caster.Status.missingHealth / info.caster.Status.maxHealth;
		float num = Mathf.Lerp(1f, dmgAmplitude, t);
		float num2 = Mathf.Lerp(1f, dmgAmplitude, t2);
		float num3 = num * num2;
		_destination = info.target.agentPosition;
		range.transform.position = _destination;
		Vector3 vector = _destination - info.caster.agentPosition;
		if (vector.magnitude < destGap)
		{
			_destination = info.target.agentPosition;
		}
		else
		{
			_destination -= vector.normalized * destGap;
		}
		_damageData = Damage(dmgFactor, procCoefficient).SetOriginPosition(range.transform.position).ApplyRawMultiplier(num3).SetSourceType(DamageData.SourceType.Physical);
		if (num3 > critStrengthThreshold)
		{
			_damageData.SetAttr(DamageAttribute.IsCrit);
		}
		FxPlayNetworked(fxCast, info.caster);
		info.caster.Control.StartDaze(dashDuartion + postDelay + descendTime);
		info.caster.Control.StartDisplacement(new DispByDestination
		{
			affectedByMovementSpeed = false,
			canGoOverTerrain = true,
			destination = _destination,
			ease = DewEase.EaseOutQuad,
			duration = dashDuartion,
			isCanceledByCC = false,
			isFriendly = true,
			rotateForward = true,
			onCancel = DestroyIfActive,
			onFinish = OnFinish
		});
	}

	private void OnFinish()
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			if (!info.target.IsNullInactiveDeadOrKnockedOut())
			{
				range.transform.position = info.target.agentPosition;
			}
			Vector3 point = range.transform.position;
			info.caster.Animation.PlayAbilityAnimation(clip);
			FxStopNetworked(startEffect);
			yield return new SI.WaitForSeconds(descendTime);
			FxStopNetworked(fxCast);
			FxPlayNetworked(fxAttack, point, null);
			ListReturnHandle<Entity> handle;
			foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
			{
				if (!entity.IsNullInactiveDeadOrKnockedOut())
				{
					FxPlayNewNetworked(fxHit, entity);
					_damageData.Dispatch(entity);
					Knockback.ApplyWithOrigin(point, entity);
					entity.Visual.KnockUp(KnockUpStrength.Big, isFriendly: false);
				}
			}
			handle.Return();
			yield return new SI.WaitForSeconds(postDelay);
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}

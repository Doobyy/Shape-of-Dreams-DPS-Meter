using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Primus_BossPrimusAeron_Adapt_IceClaw : AbilityInstance
{
	public GameObject fxHit;

	public Dash firstDash;

	public GameObject fxFirstPrepare;

	public GameObject fxFirstSwing;

	public DewCollider firstRange;

	public ScalingValue firstDamage;

	public Knockback firstKnockback;

	public float firstPrepareTime = 0.45f;

	public float firstDaze = 0.35f;

	[Space]
	public Dash secondDash;

	public GameObject fxSecondPrepare;

	public GameObject fxSecondSwing;

	public DewCollider secondRange;

	public ScalingValue secondDamage;

	public Knockback secondKnockback;

	public float secondPrepareTime = 0.65f;

	public float postDaze = 0.65f;

	private List<Entity> _hitTargets = new List<Entity>();

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Control.IncrementBlockCounters(Channel.BlockedAction.Everything);
			DestroyOnCondition(() => ((Mon_Primus_BossPrimusAeron)info.caster).phase != Mon_Primus_BossPrimusAeron.PhaseType.Adapt);
			info.caster.Control.StartChannel(new Channel
			{
				duration = firstPrepareTime,
				blockedActions = Channel.BlockedAction.Everything,
				onCancel = Destroy,
				onComplete = OnCompleteFirst
			});
			FxPlayNetworked(fxFirstPrepare, info.caster);
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		position = info.caster.position;
	}

	private void OnCompleteFirst()
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			FxStopNetworked(fxFirstPrepare);
			FxPlayNetworked(fxFirstSwing, info.caster);
			yield return null;
			firstDash.ApplyByDirection(info.caster, info.forward);
			List<Entity> entities = firstRange.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity = entities[i];
				Damage(firstDamage).SetElemental(ElementalType.Cold).SetDirection(info.forward).Dispatch(entity);
				FxPlayNewNetworked(fxHit, entity);
				firstKnockback.ApplyWithDirection(info.forward, entity);
				if (!_hitTargets.Contains(entity))
				{
					_hitTargets.Add(entity);
					CreateAbilityInstance<Ai_Mon_Primus_BossPrimusAeron_Adapt_IceClaw_Frost>(position, null, new CastInfo(info.caster, entity));
				}
			}
			handle.Return();
			yield return new WaitForSeconds(firstDaze);
			if (isActive)
			{
				FxPlayNetworked(fxSecondPrepare, info.caster);
				info.caster.Control.StartChannel(new Channel
				{
					duration = secondPrepareTime,
					blockedActions = Channel.BlockedAction.Everything,
					onCancel = Destroy,
					onComplete = OnCompleteSecond
				});
			}
		}
	}

	private void OnCompleteSecond()
	{
		if (isActive)
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			secondDash.ApplyByDirection(info.caster, info.forward);
			FxStopNetworked(fxSecondPrepare);
			FxPlayNetworked(fxSecondSwing, info.caster);
			List<Entity> entities = secondRange.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity = entities[i];
				Damage(secondDamage).SetElemental(ElementalType.Cold).SetDirection(info.forward).Dispatch(entity);
				FxPlayNewNetworked(fxHit, entity);
				secondKnockback.ApplyWithDirection(info.forward, entity);
				if (!_hitTargets.Contains(entity))
				{
					_hitTargets.Add(entity);
					CreateAbilityInstance<Ai_Mon_Primus_BossPrimusAeron_Adapt_IceClaw_Frost>(position, null, new CastInfo(info.caster, entity));
				}
			}
			handle.Return();
			yield return new WaitForSeconds(postDaze);
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((Object)(object)info.caster != null)
			{
				info.caster.Control.DecrementBlockCounters(Channel.BlockedAction.Everything);
			}
			FxStopNetworked(fxFirstPrepare);
			FxStopNetworked(fxSecondPrepare);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_hitTargets.Clear();
	}

	private void MirrorProcessed()
	{
	}
}

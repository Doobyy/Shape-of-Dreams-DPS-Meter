using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Primus_BossPrimusAeron_Adapt_Atk : StandardProjectile
{
	public GameObject fxFirstHitTime;

	public float groundHitChainRadius = 3f;

	public float entityHitChainRadius = 5.5f;

	public int totalHits = 6;

	public float intervalBetweenChain = 0.3f;

	public ScalingValue damage = "0.6ap";

	private List<Entity> _hitTargets = new List<Entity>();

	private int _doneHits;

	private ProjectileMode _pristineMode;

	private StartPositionType _pristineStart;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_pristineMode = mode;
		_pristineStart = start;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && _doneHits == 0)
		{
			FxPlayNetworked(fxFirstHitTime, info.caster);
		}
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, position, ((UnityEngine.Object)(object)info.target != null) ? entityHitChainRadius : groundHitChainRadius, tvDefaultHarmfulEffectTargets);
		if (list.Count > 0)
		{
			bool forceSummon = info.target is Summon && ContainsSummon(list);
			DoHit(Dew.SelectBestWithScore((IList<Entity>)list, (Func<Entity, int, float>)((Entity entity, int i) =>
			{
				float num = ((!((UnityEngine.Object)(object)entity == (UnityEngine.Object)(object)info.target)) ? 1 : (-1));
				if (forceSummon && entity is Summon)
				{
					num += (float)(((UnityEngine.Object)(object)entity == (UnityEngine.Object)(object)info.target) ? 100 : 1000);
				}
				return num;
			}), 0.1f, (DewRandom)null));
		}
		else
		{
			DoHit(null);
		}
		handle.Return();
	}

	private static bool ContainsSummon(List<Entity> ents)
	{
		foreach (Entity ent in ents)
		{
			if (ent is Summon)
			{
				return true;
			}
		}
		return false;
	}

	private void DoHit(Entity nextChainTarget)
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			if (_doneHits >= totalHits)
			{
				Destroy();
			}
			else
			{
				if ((UnityEngine.Object)(object)nextChainTarget != null)
				{
					if (_hitTargets.Contains(nextChainTarget))
					{
						Damage(damage).ApplyStrength(0.15f).SetElemental(ElementalType.Light).SetOriginPosition(info.caster.agentPosition)
							.Dispatch(nextChainTarget);
					}
					else
					{
						_hitTargets.Add(nextChainTarget);
						Damage(damage).SetElemental(ElementalType.Light).SetOriginPosition(info.caster.agentPosition).Dispatch(nextChainTarget);
					}
				}
				yield return new WaitForSeconds(intervalBetweenChain);
				if (info.caster.IsNullOrInactive())
				{
					Destroy();
				}
				else
				{
					Actor actor = ((parentActor is AbilityTrigger) ? this : parentActor);
					if ((UnityEngine.Object)(object)nextChainTarget != null)
					{
						actor.CreateAbilityInstance(position, null, new CastInfo(info.caster, nextChainTarget), (Ai_Mon_Primus_BossPrimusAeron_Adapt_Atk ai) =>
						{
							ai.start = StartPositionType.Custom;
							ai.SetCustomStartPosition(position);
							ai.mode = ProjectileMode.Target;
							ai._doneHits = _doneHits + 1;
							ai._hitTargets = _hitTargets;
						});
					}
					else
					{
						actor.CreateAbilityInstance(position, null, new CastInfo(info.caster, position + (position - info.caster.agentPosition).normalized * 2f + UnityEngine.Random.insideUnitCircle.ToXZ() * 1.5f), (Ai_Mon_Primus_BossPrimusAeron_Adapt_Atk ai) =>
						{
							ai.start = StartPositionType.Custom;
							ai.SetCustomStartPosition(position);
							ai.mode = ProjectileMode.Point;
							ai._doneHits = _doneHits + 1;
							ai._hitTargets = _hitTargets;
						});
					}
					Destroy();
				}
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		mode = _pristineMode;
		start = _pristineStart;
		_doneHits = 0;
		_hitTargets = new List<Entity>();
	}

	private void MirrorProcessed()
	{
	}
}

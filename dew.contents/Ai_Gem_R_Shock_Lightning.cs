using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Gem_R_Shock_Lightning : StandardProjectile, ACH_THEYRE_JUST_BIG_CATS.ILightingActor
{
	public float chainDelay;

	public DewCollider chainRange;

	public ScalingValue chainDamage;

	public float firstProcCoefficient = 1f;

	public float chainProcCoefficient = 0.75f;

	public bool canChainToSelf;

	public GameObject firstEffect;

	[NonSerialized]
	public int maxHitCount;

	private Dictionary<Entity, int> _affectedEntities;

	private int _chainedCount;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && _chainedCount <= 0)
		{
			FxPlayNetworked(firstEffect);
		}
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		StartSequence(Sequence());
		IEnumerator Sequence()
		{
			Damage(chainDamage, (_chainedCount <= 0) ? firstProcCoefficient : chainProcCoefficient).SetElemental(ElementalType.Light).Dispatch(info.target);
			if (_chainedCount >= maxHitCount - 1)
			{
				Destroy();
			}
			else
			{
				yield return new SI.WaitForSeconds(chainDelay);
				if (_affectedEntities == null)
				{
					_affectedEntities = new Dictionary<Entity, int>();
				}
				if (!_affectedEntities.ContainsKey(info.target))
				{
					_affectedEntities.Add(info.target, 1);
				}
				else
				{
					_affectedEntities[info.target]++;
				}
				List<Entity> entities = chainRange.GetEntities(out var _, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
				{
					sortComparer = CollisionCheckSettings.DistanceFromCenter
				});
				Entity entity = null;
				float num = float.NegativeInfinity;
				for (int i = 0; i < entities.Count; i++)
				{
					Entity entity2 = entities[i];
					if (!((UnityEngine.Object)(object)entity2 == (UnityEngine.Object)(object)info.target) || canChainToSelf)
					{
						float num2 = (float)(CollectionExtensions.GetValueOrDefault<Entity, int>((IReadOnlyDictionary<Entity, int>)_affectedEntities, entity2, 0) * -1000) + Vector3.SqrMagnitude(position - entity2.position);
						if (num < num2)
						{
							entity = entity2;
							num = num2;
						}
					}
				}
				if ((UnityEngine.Object)(object)entity == null)
				{
					Destroy();
				}
				else
				{
					Vector3 vector = (info.target.IsNullInactiveDeadOrKnockedOut() ? position : info.target.Visual.GetCenterPosition());
					CreateAbilityInstance(vector, Quaternion.identity, new CastInfo(info.caster, entity), (Ai_Gem_R_Shock_Lightning c) =>
					{
						c.maxHitCount = maxHitCount;
						c._affectedEntities = _affectedEntities;
						c._chainedCount = _chainedCount + 1;
					});
					Destroy();
				}
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_chainedCount = 0;
		_affectedEntities = null;
	}

	private void MirrorProcessed()
	{
	}
}

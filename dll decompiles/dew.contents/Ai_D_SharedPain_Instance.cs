using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Mirror;
using UnityEngine;

public class Ai_D_SharedPain_Instance : AbilityInstance
{
	public ScalingValue shareDmgRatio;

	public DewBeamRenderer beam;

	public GameObject fxBeamStart;

	public GameObject fxHit;

	[NonSerialized]
	public Entity lastEntity;

	[NonSerialized]
	public int maxSharedCount;

	[NonSerialized]
	public FinalDamageData originalDamage;

	[NonSerialized]
	public float checkRadius;

	[NonSerialized]
	private List<Entity> hitEntities;

	private int _currentSharedCount;

	private Sequence _beamSequence;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		if (_beamSequence != null)
		{
			TweenExtensions.Kill((Tween)(object)_beamSequence, false);
			_beamSequence = null;
		}
		_currentSharedCount = 0;
		hitEntities = null;
		lastEntity = null;
		if (beam != null)
		{
			beam.enabled = false;
		}
	}

	protected override IEnumerator OnCreateSequenced()
	{
		Quaternion quaternion = Quaternion.LookRotation((info.target.position - position).normalized);
		((Component)(object)this).transform.rotation = quaternion;
		Vector3 currentEndPos;
		Vector3 currentStartPos;
		Vector3 vector = (currentEndPos = (currentStartPos = (((UnityEngine.Object)(object)lastEntity == null) ? info.caster.Visual.GetCenterPosition() : lastEntity.Visual.GetCenterPosition())));
		FxPlay(fxHit, info.target);
		FxPlayNew(fxBeamStart, vector, rotation);
		beam.SetPoints(vector, vector);
		beam.enabled = true;
		float num = (vector - info.target.Visual.GetCenterPosition()).magnitude / 30f;
		Sequence val = (_beamSequence = DOTween.Sequence());
		TweenSettingsExtensions.SetLink<Sequence>(val, ((Component)(object)this).gameObject);
		TweenSettingsExtensions.OnKill<Sequence>(val, (TweenCallback)(() =>
		{
			if (((NetworkBehaviour)this).isServer)
			{
				DestroyIfActive();
			}
		}));
		TweenSettingsExtensions.Append(val, (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.OnUpdate<TweenerCore<Vector3, Vector3, VectorOptions>>(DOTween.To((DOGetter<Vector3>)(() => currentEndPos), (DOSetter<Vector3>)((Vector3 x) =>
		{
			currentEndPos = x;
		}), info.target.Visual.GetCenterPosition(), num), (TweenCallback)(() =>
		{
			beam.SetEndPoint(currentEndPos);
		})), (TweenCallback)(() =>
		{
			if (((NetworkBehaviour)this).isServer)
			{
				Vector3 vector2 = (((UnityEngine.Object)(object)info.target == null) ? currentEndPos : info.target.position);
				if (info.caster.CheckEnemyOrNeutral(info.target))
				{
					DamageData damageData = PureDamage((originalDamage.amount + originalDamage.discardedAmount) * GetValue(shareDmgRatio)).SetSourceType(originalDamage.type).SetAmountOrigin(originalDamage).SetOriginPosition(vector2);
					if (originalDamage.elemental.HasValue)
					{
						damageData.SetElemental(originalDamage.elemental.Value);
					}
					damageData.Dispatch(info.target);
				}
				if (hitEntities == null)
				{
					hitEntities = new List<Entity>();
				}
				hitEntities.Add(info.target);
				OnCompleteSequence(vector2);
			}
		})));
		TweenSettingsExtensions.Append(val, (Tween)(object)TweenSettingsExtensions.OnUpdate<TweenerCore<Vector3, Vector3, VectorOptions>>(DOTween.To((DOGetter<Vector3>)(() => currentStartPos), (DOSetter<Vector3>)((Vector3 x) =>
		{
			currentStartPos = x;
		}), info.target.Visual.GetCenterPosition(), num), (TweenCallback)(() =>
		{
			beam.SetStartPoint(currentStartPos);
		})));
		TweenSettingsExtensions.OnComplete<Sequence>(val, (TweenCallback)(() =>
		{
			beam.enabled = false;
			if (((NetworkBehaviour)this).isServer)
			{
				Destroy();
			}
		}));
		yield break;
	}

	private void OnCompleteSequence(Vector3 sourcePos)
	{
		if (_currentSharedCount >= maxSharedCount - 1)
		{
			return;
		}
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, sourcePos, checkRadius, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.Random
		});
		Entity entity = null;
		foreach (Entity item in list)
		{
			if (!((info.caster.GetRelation(item) == EntityRelation.Ally) | (info.caster.GetRelation(item) == EntityRelation.Neutral)) && !((UnityEngine.Object)(object)item == (UnityEngine.Object)(object)info.target) && !((UnityEngine.Object)(object)item == (UnityEngine.Object)(object)info.caster) && !hitEntities.Contains(item) && !item.IsNullInactiveDeadOrKnockedOut())
			{
				entity = item;
				break;
			}
		}
		if ((UnityEngine.Object)(object)entity == null)
		{
			foreach (Entity item2 in list)
			{
				if (!((info.caster.GetRelation(item2) == EntityRelation.Ally) | (info.caster.GetRelation(item2) == EntityRelation.Neutral)) && !((UnityEngine.Object)(object)item2 == (UnityEngine.Object)(object)info.target) && !((UnityEngine.Object)(object)item2 == (UnityEngine.Object)(object)info.caster) && !item2.IsNullInactiveDeadOrKnockedOut())
				{
					entity = item2;
					break;
				}
			}
		}
		if ((UnityEngine.Object)(object)entity == null && (UnityEngine.Object)(object)info.target != (UnityEngine.Object)(object)info.caster && Vector3.Distance(sourcePos, info.caster.position) <= checkRadius && !info.caster.IsNullInactiveDeadOrKnockedOut())
		{
			entity = info.caster;
		}
		if ((UnityEngine.Object)(object)entity != null)
		{
			CreateProjectile(entity);
		}
		handle.Return();
		void CreateProjectile(Entity target)
		{
			if (((NetworkBehaviour)this).isServer)
			{
				CreateAbilityInstance(sourcePos, null, new CastInfo(info.caster, target), (Ai_D_SharedPain_Instance p) =>
				{
					p.maxSharedCount = maxSharedCount;
					p.originalDamage = originalDamage;
					p.checkRadius = checkRadius;
					p._currentSharedCount = _currentSharedCount + 1;
					p.hitEntities = hitEntities;
					p.lastEntity = info.target;
				});
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (beam != null)
		{
			beam.enabled = false;
		}
	}

	private void MirrorProcessed()
	{
	}
}

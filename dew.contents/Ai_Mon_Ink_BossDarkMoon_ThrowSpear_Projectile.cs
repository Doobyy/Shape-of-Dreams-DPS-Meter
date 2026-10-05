using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossDarkMoon_ThrowSpear_Projectile : StandardProjectile
{
	private class Ad_ThrowSpearProjectile
	{
		public float time;
	}

	public float delayOnDestroy;

	public float interval;

	public ScalingValue dmgFactor;

	public Knockback knockback;

	public GameObject fxHit;

	public GameObject fxComplete;

	private bool _isComplete;

	private float _baseCollisionRadius;

	private Vector3 _baseEffectOnFlyScale;

	protected override void Awake()
	{
		base.Awake();
		_baseCollisionRadius = collisionRadius;
		if (effectOnFly != null)
		{
			_baseEffectOnFlyScale = effectOnFly.transform.localScale;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_isComplete = false;
		collisionRadius = _baseCollisionRadius;
		if (effectOnFly != null)
		{
			effectOnFly.transform.localScale = _baseEffectOnFlyScale;
		}
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			_isComplete = true;
			FxPlayNetworked(fxComplete);
			yield return new SI.WaitForSeconds(delayOnDestroy);
			FxStopNetworked(fxComplete);
			Destroy();
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (!((NetworkBehaviour)this).isServer || _isComplete)
		{
			return;
		}
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, ((Component)(object)this).transform.position, collisionRadius, tvDefaultHarmfulEffectTargets))
		{
			if (item.TryGetData<Ad_ThrowSpearProjectile>(out var data))
			{
				if (Time.time - data.time < interval)
				{
					continue;
				}
				data.time = Time.time;
			}
			else
			{
				item.AddData(new Ad_ThrowSpearProjectile
				{
					time = Time.time
				});
			}
			CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(((Component)(object)this).transform.position).Dispatch(item);
			knockback.ApplyWithOrigin(((Component)(object)this).transform.position, item);
			FxPlayNewNetworked(fxHit, item);
		}
		handle.Return();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			if (allEntity.TryGetData<Ad_ThrowSpearProjectile>(out var data))
			{
				allEntity.RemoveData(data);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}

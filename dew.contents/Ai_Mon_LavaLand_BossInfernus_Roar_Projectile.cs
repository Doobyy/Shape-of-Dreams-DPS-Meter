using Mirror;
using UnityEngine;

public class Ai_Mon_LavaLand_BossInfernus_Roar_Projectile : StandardProjectile
{
	private class Ad_RoarProjectile
	{
		public float time;
	}

	public float interval;

	public ScalingValue dmgFactor;

	public Knockback knockback;

	public GameObject fxHit;

	public override bool reuseInRoom => true;

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, ((Component)(object)this).transform.position, collisionRadius, tvDefaultHarmfulEffectTargets))
		{
			if (item.TryGetData<Ad_RoarProjectile>(out var data))
			{
				if (Time.time - data.time < interval)
				{
					continue;
				}
				data.time = Time.time;
			}
			else
			{
				item.AddData(new Ad_RoarProjectile
				{
					time = Time.time
				});
			}
			CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(position).SetElemental(ElementalType.Fire).SetDirection(info.forward)
				.Dispatch(item);
			knockback.ApplyWithDirection(info.forward, item);
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
			if (allEntity.TryGetData<Ad_RoarProjectile>(out var data))
			{
				allEntity.RemoveData(data);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}

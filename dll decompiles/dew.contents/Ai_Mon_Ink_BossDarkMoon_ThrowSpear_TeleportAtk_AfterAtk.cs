using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossDarkMoon_ThrowSpear_TeleportAtk_AfterAtk : StandardProjectile
{
	private class Ad_ThrowSpearAfterAtk
	{
		public float time;
	}

	public float interval;

	public ScalingValue dmgFactor;

	public Knockback knockback;

	public GameObject fxHit;

	public DewCollider range;

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
		{
			if (entity.TryGetData<Ad_ThrowSpearAfterAtk>(out var data))
			{
				if (Time.time - data.time < interval)
				{
					continue;
				}
				data.time = Time.time;
			}
			else
			{
				entity.AddData(new Ad_ThrowSpearAfterAtk
				{
					time = Time.time
				});
			}
			CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(((Component)(object)this).transform.position).Dispatch(entity);
			knockback.ApplyWithOrigin(((Component)(object)this).transform.position, entity);
			FxPlayNewNetworked(fxHit, entity);
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
			if (allEntity.TryGetData<Ad_ThrowSpearAfterAtk>(out var data))
			{
				allEntity.RemoveData(data);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}

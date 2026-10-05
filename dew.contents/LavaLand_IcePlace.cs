using Mirror;
using UnityEngine;

public class LavaLand_IcePlace : Actor
{
	public float applyElementalInterval;

	public float radius;

	private float _lastDamageTime;

	public override bool isDestroyedOnRoomChange => true;

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || !(Time.time - _lastDamageTime > applyElementalInterval))
		{
			return;
		}
		_lastDamageTime = Time.time;
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, ((Component)(object)this).transform.position, radius))
		{
			if (item.Control.isDashing || item.IsNullInactiveDeadOrKnockedOut())
			{
				continue;
			}
			if (item.Status.TryGetStatusEffect<Se_ArcticTerritory>(out var effect))
			{
				effect.ResetTimer();
				continue;
			}
			CreateStatusEffect(item, default, (Se_ArcticTerritory b) =>
			{
				b.duration *= 5f;
			});
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}

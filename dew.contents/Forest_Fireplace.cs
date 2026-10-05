using Mirror;
using UnityEngine;

public class Forest_Fireplace : Actor, IBanRoomNodesNearby
{
	public static Forest_Fireplace instance;

	public float damageMaxHealthRatio;

	public float damageInterval;

	public float damageProcCoeff;

	public float radius;

	public GameObject fxHit;

	private float _lastDamageTime;

	public override bool isDestroyedOnRoomChange => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		instance = this;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || !(Time.time - _lastDamageTime > damageInterval))
		{
			return;
		}
		_lastDamageTime = Time.time;
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, ((Component)(object)this).transform.position, radius))
		{
			if (item is Hero || item is Monster || item is Summon)
			{
				DefaultDamage(damageMaxHealthRatio * item.maxHealth, damageProcCoeff).SetElemental(ElementalType.Fire).SetOriginPosition(((Component)(object)this).transform.position).Dispatch(item);
				FxPlayNewNetworked(fxHit, item);
			}
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}

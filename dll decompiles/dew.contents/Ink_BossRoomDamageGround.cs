using System.Collections;
using Mirror;
using UnityEngine;

public class Ink_BossRoomDamageGround : Actor
{
	public float dmgMaxHealthRatio;

	public float radius;

	public float interval;

	public GameObject fxInstance;

	public GameObject fxHit;

	private float _currentTime;

	private bool _spawnGroundEnable;

	public void Spawn()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			_currentTime = 0f;
			_spawnGroundEnable = true;
			FxPlayNetworked(fxInstance, ((Component)(object)this).transform.position, ((Component)(object)this).transform.rotation);
			SingletonDewNetworkBehaviour<Room>.instance.onRoomClear.AddListener(() =>
			{
				((MonoBehaviour)(object)this).StartCoroutine(Routine());
			});
		}
		IEnumerator Routine()
		{
			FxStopNetworked(fxInstance);
			yield return new WaitForSeconds(3f);
			DestroyIfActive();
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (!((NetworkBehaviour)this).isServer || !_spawnGroundEnable || Time.time - _currentTime < interval)
		{
			return;
		}
		_currentTime = Time.time;
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, ((Component)(object)this).transform.position, radius))
		{
			if (item.owner.isHumanPlayer && !item.IsNullInactiveDeadOrKnockedOut())
			{
				float amount = item.Status.maxHealth * dmgMaxHealthRatio;
				CreateDamage(DamageData.SourceType.Pure, amount).SetOriginPosition(((Component)(object)this).transform.position).SetAttr(DamageAttribute.DamageOverTime).Dispatch(item);
				FxPlayNewNetworked(fxHit, item);
			}
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}

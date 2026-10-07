using Mirror;
using UnityEngine;

public class Ai_ArcticTerritory : AbilityInstance
{
	[HideInInspector]
	public float radius;

	public float collisionCheckInterval;

	public GameObject fxInstance;

	private float _currentTime;

	private float _elapsedTime;

	protected override void OnCreate()
	{
		base.OnCreate();
		_currentTime = 0f;
		_elapsedTime = 0f;
		fxInstance.transform.localScale = Vector3.one * radius;
		FxPlay(fxInstance, ((Component)(object)this).transform.position, null);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		FxStop(fxInstance);
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (!((NetworkBehaviour)this).isServer || Time.time - _currentTime < collisionCheckInterval)
		{
			return;
		}
		_currentTime = Time.time;
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, ((Component)(object)this).transform.position, radius))
		{
			if (item.owner.isHumanPlayer && !item.Control.isDashing && !item.IsNullInactiveDeadOrKnockedOut())
			{
				if (item.Status.TryGetStatusEffect<Se_ArcticTerritory>(out var effect))
				{
					effect.ResetTimer();
				}
				else
				{
					CreateStatusEffect<Se_ArcticTerritory>(item);
				}
			}
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}

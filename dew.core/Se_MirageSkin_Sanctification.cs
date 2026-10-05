using Mirror;
using UnityEngine;

public class Se_MirageSkin_Sanctification : MirageSkinEffect
{
	public float searchInterval = 0.45f;

	public float leashDist = 4.5f;

	private float _lastSearchTime;

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || Time.time - _lastSearchTime < searchInterval)
		{
			return;
		}
		_lastSearchTime = Time.time;
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, victim.agentPosition, leashDist, tvDefaultUsefulEffectTargets))
		{
			if (!item.Status.HasStatusEffect<Se_MirageSkin_Sanctification>() && !item.IsAnyBoss())
			{
				if (item.Status.TryGetStatusEffect<Se_MirageSkin_Sanctification_Protected>(out var effect))
				{
					effect.ResetTimer();
				}
				else if (item is Monster && children.Count <= 7)
				{
					CreateStatusEffect<Se_MirageSkin_Sanctification_Protected>(item, new CastInfo(victim));
				}
			}
		}
		handle.Return();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_lastSearchTime = 0f;
	}

	private void MirrorProcessed()
	{
	}
}

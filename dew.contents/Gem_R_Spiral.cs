using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Gem_R_Spiral : Gem
{
	public ScalingValue shootSpeed;

	public float maxShootSpeed;

	public float searchRadius;

	public AbilityTargetValidator targets;

	private float _lastCheckTime;

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || !isValid || owner.isKnockedOut || owner.Status.hasStealth)
		{
			return;
		}
		float num = 1f / Mathf.Min(GetValue(shootSpeed), maxShootSpeed);
		if (!(Time.time - _lastCheckTime < num))
		{
			_lastCheckTime = Time.time;
			List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, owner.agentPosition, searchRadius, targets, owner);
			if (list.Count > 0)
			{
				Entity target = list[Random.Range(0, list.Count)];
				CreateAbilityInstance<Ai_Gem_R_Spiral_Fireball>(owner.position, null, new CastInfo(owner, target));
				NotifyUse();
			}
			handle.Return();
		}
	}

	private void MirrorProcessed()
	{
	}
}

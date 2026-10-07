using System;
using UnityEngine;

public class Ai_Atk_YubarStardust : AttackProjectile
{
	[NonSerialized]
	public Entity chainTarget0;

	[NonSerialized]
	public Entity chainTarget1;

	[NonSerialized]
	public Entity chainTarget2;

	[NonSerialized]
	public float chainStrength;

	private float _baseStrength;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseStrength = strength;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		strength = _baseStrength;
		isMain = true;
		chainTarget0 = null;
		chainTarget1 = null;
		chainTarget2 = null;
		chainStrength = 0f;
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		if (!((UnityEngine.Object)(object)hit.entity != (UnityEngine.Object)(object)info.target))
		{
			TryChainTarget(chainTarget0);
			TryChainTarget(chainTarget1);
			TryChainTarget(chainTarget2);
		}
		void TryChainTarget(Entity target)
		{
			if (!target.IsNullInactiveDeadOrKnockedOut() && !((UnityEngine.Object)(object)target == (UnityEngine.Object)(object)info.target))
			{
				isMain = false;
				strength = chainStrength;
				OnEntity(new EntityHit
				{
					entity = target,
					point = target.Visual.GetCenterPosition()
				});
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}

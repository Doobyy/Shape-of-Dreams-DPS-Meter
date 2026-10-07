using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_M_FrostyCharge : Ai_GenericDodge
{
	public float radius = 2.5f;

	public float colCheckInterval = 0.1f;

	public GameObject fxHit;

	public ScalingValue damage;

	private HashSet<Entity> _hitEntities = new HashSet<Entity>();

	private float _lastCollisionCheckTime;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_hitEntities.Clear();
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		position = info.caster.position;
		if (!((NetworkBehaviour)this).isServer || Time.time - _lastCollisionCheckTime < colCheckInterval)
		{
			return;
		}
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, position, radius, tvDefaultHarmfulEffectTargets))
		{
			if (_hitEntities.Add(item))
			{
				Damage(damage).SetElemental(ElementalType.Cold).SetOriginPosition(info.caster.position).Dispatch(item);
			}
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}

using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_DarkCave_CaveBat_Dash : AbilityInstance
{
	public DewCollider range;

	public ScalingValue damage;

	public Dash dash;

	public GameObject hitEffect;

	private readonly List<Entity> _affected = new List<Entity>();

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_affected.Clear();
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			dash.ApplyByDirection(info.caster, info.forward, (DispByDestination d) =>
			{
				d.affectedByMovementSpeed = true;
				d.onCancel = DestroyIfActive;
				d.onFinish = DestroyIfActive;
			});
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		((Component)(object)this).transform.position = info.caster.position;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
		{
			if (!_affected.Contains(entity))
			{
				_affected.Add(entity);
				DefaultDamage(damage).Dispatch(entity);
				FxPlayNewNetworked(hitEffect, entity);
			}
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}

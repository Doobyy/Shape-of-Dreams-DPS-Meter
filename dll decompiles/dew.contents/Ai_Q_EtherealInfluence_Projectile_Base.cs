using Mirror;
using UnityEngine;

public class Ai_Q_EtherealInfluence_Projectile_Base : StandardProjectile, IEtherealPathDamage
{
	public ScalingValue damage;

	public float procCoefficient = 1f;

	protected Ai_Q_EtherealInfluence _parent;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_parent = FindFirstAncestorOfType<Ai_Q_EtherealInfluence>();
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if ((Object)(object)_parent != null && _parent.isActive)
		{
			_parent._projectilePos = position;
		}
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		if ((Object)(object)hit.entity == (Object)(object)info.caster)
		{
			Destroy();
		}
		else
		{
			Damage(damage, procCoefficient).SetElemental(ElementalType.Light).SetDirection(rotation).Dispatch(hit.entity);
		}
	}

	private void MirrorProcessed()
	{
	}
}

using Mirror;
using UnityEngine;

public class Se_E_Permafrost_Stun : StatusEffect
{
	public float stunDuration;

	private float _animationSpeed;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		Animator val = (((Object)(object)victim != null && (Object)(object)victim.Animation != null) ? victim.Animation.animator : null);
		if ((Object)(object)val != null)
		{
			((Behaviour)(object)val).enabled = false;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(stunDuration);
			DoStun();
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			ApplyElemental(ElementalType.Cold, victim);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if ((Object)(object)victim != null && (Object)(object)victim.Animation != null && (Object)(object)victim.Animation.animator != null)
		{
			((Behaviour)(object)victim.Animation.animator).enabled = true;
		}
	}

	private void MirrorProcessed()
	{
	}
}

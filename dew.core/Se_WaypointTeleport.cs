using System;
using Mirror;
using UnityEngine;

public class Se_WaypointTeleport : StatusEffect
{
	public DewAnimationClip channelAnim;

	public DewAnimationClip completeAnim;

	public float channelDuration = 1f;

	public float postTeleportDaze = 0.75f;

	public GameObject teleportEffect;

	private bool _didFail;

	private Channel _daze;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.Control.CancelOngoingChannels();
			victim.Control.Stop();
			victim.Control.Rotate(Vector3.back, immediately: false);
			if (channelAnim != null)
			{
				victim.Animation.PlayAbilityAnimation(channelAnim);
			}
			victim.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
			_daze = info.caster.Control.StartDaze(channelDuration);
			Channel daze = _daze;
			daze.onCancel = (Action)Delegate.Combine(daze.onCancel, (Action)(() =>
			{
				_didFail = true;
				Destroy();
			}));
			SetTimer(channelDuration);
			ShowOnScreenTimer();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (_daze != null && _daze.isAlive)
		{
			_daze.Cancel();
		}
		if (!((UnityEngine.Object)(object)victim != null) || !victim.isActive)
		{
			return;
		}
		victim.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		if (channelAnim != null)
		{
			victim.Animation.StopAbilityAnimation(channelAnim);
		}
		if (!_didFail)
		{
			if (completeAnim != null)
			{
				victim.Animation.PlayAbilityAnimation(completeAnim);
			}
			Teleport(victim, info.point);
			FxPlayNetworked(teleportEffect, victim);
			victim.Control.Stop();
			victim.Control.StartDaze(postTeleportDaze);
		}
		else if ((UnityEngine.Object)(object)victim.owner != null && victim.owner.isHumanPlayer)
		{
			victim.owner.TpcShowCenterMessage(CenterMessageType.Error, "InGame_Message_TeleportUnavailableInCombat");
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		if (isActive && !(obj.actor is ElementalStatusEffect) && obj.actor.FindFirstOfType<Entity>() is Monster)
		{
			_didFail = true;
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}

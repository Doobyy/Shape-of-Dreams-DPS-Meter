using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class St_D_ExplosionArtist : SkillTrigger
{
	public int largeExplosionRequiredFireStack = 2;

	public float largeExplosionCooldownPerTarget = 3f;

	public GameObject fxDash;

	private Dictionary<uint, float> _targetLastExplodeTimes = new Dictionary<uint, float>();

	protected override void OnEquip(Entity newOwner)
	{
		base.OnEquip(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			owner.Control.ClientEvent_OnDisplacementStarted += new Action<Displacement>(ClientEventOnDisplacementStarted);
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		}
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom obj)
	{
		_targetLastExplodeTimes.Clear();
	}

	protected override void OnUnequip(Entity formerOwner)
	{
		base.OnUnequip(formerOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			if ((bool)(UnityEngine.Object)(object)formerOwner)
			{
				formerOwner.Control.ClientEvent_OnDisplacementStarted -= new Action<Displacement>(ClientEventOnDisplacementStarted);
			}
			if ((bool)(UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance)
			{
				NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
			}
		}
	}

	private void ClientEventOnDisplacementStarted(Displacement obj)
	{
		if (obj.isDodging)
		{
			FxPlayNewNetworked(fxDash, owner);
			Displacement displacement = obj;
			displacement.onFinish = (Action)Delegate.Combine(displacement.onFinish, new Action(Explosion));
			Displacement displacement2 = obj;
			displacement2.onCancel = (Action)Delegate.Combine(displacement2.onCancel, new Action(Explosion));
		}
		void Explosion()
		{
			if (obj is DispByDestination dispByDestination)
			{
				CreateAbilityInstance<Ai_D_ExplosionArtist_DodgeExplosion>(dispByDestination.destination, null, new CastInfo(owner));
			}
		}
	}

	public void HandleHit(AbilityInstance self, Entity entity)
	{
		if (!_targetLastExplodeTimes.TryGetValue(((NetworkBehaviour)entity).netId, out var value) || !(Time.time - value < largeExplosionCooldownPerTarget))
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return null;
			if (entity.IsNullInactiveDeadOrKnockedOut() || entity.Status.fireStack >= largeExplosionRequiredFireStack)
			{
				_targetLastExplodeTimes[((NetworkBehaviour)entity).netId] = Time.time;
				self.CreateAbilityInstance<Ai_D_ExplosionArtist_LargeExplosion>(entity.position, null, new CastInfo(owner, entity));
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}

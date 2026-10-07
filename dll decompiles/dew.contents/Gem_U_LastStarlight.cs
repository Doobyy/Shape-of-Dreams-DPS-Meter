using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Gem_U_LastStarlight : Gem
{
	public float maxSpawnRadius;

	public GameObject fxCast;

	private ActorRef<Ai_Gem_U_LastStarlight> _instance;

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			newSkill.TriggerEvent_OnCastStart += new Action<EventInfoCast>(OnCastStart);
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)oldSkill != null)
			{
				oldSkill.TriggerEvent_OnCastStart -= new Action<EventInfoCast>(OnCastStart);
			}
			_instance = null;
		}
	}

	protected override void OnCastComplete(EventInfoCast info)
	{
		base.OnCastComplete(info);
		if (!_instance.IsNullOrInactive())
		{
			_instance.Get().parentActor = info.instance;
		}
	}

	private void OnCastStart(EventInfoCast obj)
	{
		if (!IsReady() || !isValid)
		{
			return;
		}
		NotifyUse();
		StartCooldown();
		FxPlayNetworked(fxCast, owner);
		Vector3? vector = null;
		Vector3 forwardDir = ((Component)(object)owner).transform.forward;
		switch (obj.trigger.configs[obj.configIndex].castMethod.type)
		{
		case CastMethodType.Cone:
			forwardDir = obj.info.forward;
			break;
		case CastMethodType.Arrow:
			forwardDir = obj.info.forward;
			break;
		case CastMethodType.Target:
			vector = obj.info.target.agentPosition;
			break;
		case CastMethodType.Point:
			vector = obj.info.point;
			break;
		}
		List<Entity> ents;
		if (!vector.HasValue)
		{
			ents = DewPhysics.OverlapCircleAllEntities(out var handle, owner.position, maxSpawnRadius, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
			{
				sortComparer = CollisionCheckSettings.DistanceFromCenter
			});
			if (TryGetTargetPoint(60f, out var p))
			{
				vector = p;
			}
			else if (TryGetTargetPoint(120f, out p))
			{
				vector = p;
			}
			else if (TryGetTargetPoint(360f, out p))
			{
				vector = p;
			}
			handle.Return();
		}
		if (!vector.HasValue)
		{
			vector = owner.agentPosition + forwardDir * maxSpawnRadius;
		}
		_instance = CreateAbilityInstanceWithSource<Ai_Gem_U_LastStarlight>(this, vector.Value, null, new CastInfo(owner, vector.Value));
		bool TryGetTargetPoint(float allowedAngle, out Vector3 reference)
		{
			float num = float.PositiveInfinity;
			foreach (Entity item in ents)
			{
				if (!item.IsNullInactiveDeadOrKnockedOut() && item is Monster)
				{
					Vector3 agentPosition = item.agentPosition;
					Vector3 vector2 = agentPosition - owner.agentPosition;
					if (!(Vector3.Angle(forwardDir, vector2) > allowedAngle / 2f) && !(vector2.sqrMagnitude > maxSpawnRadius * maxSpawnRadius))
					{
						Vector3 vector3 = Vector3.Project(vector2, forwardDir);
						Vector3 vector4 = owner.agentPosition + vector3 - agentPosition;
						if (!(num * num <= vector4.sqrMagnitude))
						{
							num = vector4.sqrMagnitude;
							reference = agentPosition;
							return true;
						}
					}
				}
			}
			reference = default;
			return false;
		}
	}

	private void MirrorProcessed()
	{
	}
}

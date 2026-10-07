using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Star_Bismuth_D_SkillHasteAndAutoCast : StarEffect
{
	public StarScalingValue bonusAmount;

	public float castMinIntervalSoloGame = 0.05f;

	public float castMinIntervalCoopGame = 0.2f;

	public Vector2 aiDetectDelay = new Vector2(0.25f, 0.75f);

	private int _index;

	private float _lastCastTime;

	public override Type heroType => typeof(Hero_Bismuth);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				abilityHasteFlat = GetValue(bonusAmount)
			});
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || hero.IsNullInactiveDeadOrKnockedOut() || !hero.isInCombat || hero.Control.ongoingChannels.Count > 0 || NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition || ManagerBase<CameraManager>.instance.isPlayingCutscene)
		{
			return;
		}
		float num = ((DewPlayer.allHumanPlayers.Count > 1) ? castMinIntervalCoopGame : castMinIntervalSoloGame);
		if (!(Time.time - _lastCastTime < num))
		{
			_index = (_index + 1) % 4;
			if ((_index != 0 || !Process(hero.Skill.Q)) && (_index != 1 || !Process(hero.Skill.W)) && (_index != 2 || !Process(hero.Skill.E)) && _index == 3)
			{
				Process(hero.Skill.R);
			}
		}
		bool Process(SkillTrigger s)
		{
			if (s.IsNullOrInactive())
			{
				return false;
			}
			if (!s.currentConfig.isActive)
			{
				return false;
			}
			if (!s.CanBeCast() || !s.CanBeReserved())
			{
				return false;
			}
			IReadOnlyList<ActionBase> queuedActions = hero.Control.queuedActions;
			for (int i = 0; i < queuedActions.Count; i++)
			{
				if (queuedActions[i] is ActionCast actionCast && (UnityEngine.Object)(object)actionCast.trigger == (UnityEngine.Object)(object)s)
				{
					return false;
				}
			}
			CastInfo castInfo;
			if (s.currentConfig.castMethod.type == CastMethodType.None)
			{
				castInfo = new CastInfo(hero);
			}
			else
			{
				Entity entity = null;
				ListReturnHandle<Entity> handle;
				foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, hero.position, s.currentConfig.effectiveRange, new CollisionCheckSettings
				{
					sortComparer = CollisionCheckSettings.DistanceFromCenter
				}))
				{
					if (s.currentConfig.targetValidator.Evaluate(hero, item) && !item.Visual.isSpawning)
					{
						float num2 = aiDetectDelay.Lerp((float)((NetworkBehaviour)item).netId * 0.25f);
						if (!(Time.time - item.creationTime < num2))
						{
							entity = item;
							break;
						}
					}
				}
				handle.Return();
				if ((UnityEngine.Object)(object)entity == null)
				{
					return false;
				}
				castInfo = s.GetPredictedCastInfoToTarget(entity, UnityEngine.Random.Range(0f, 0.5f));
			}
			hero.Control.Cast(s, s.currentConfigIndex, castInfo);
			_lastCastTime = Time.time;
			return true;
		}
	}

	private void MirrorProcessed()
	{
	}
}

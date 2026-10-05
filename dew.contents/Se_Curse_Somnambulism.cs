using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Curse_Somnambulism : CurseStatusEffect
{
	public float[] castInterval;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		List<SkillTrigger> list = new List<SkillTrigger>();
		while (true)
		{
			yield return new SI.WaitForSeconds(GetValue(castInterval) * UnityEngine.Random.Range(0.7f, 1.3f));
			if (NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition || ManagerBase<CameraManager>.instance.isPlayingCutscene || victim.Status.isInConversation || !(victim is Hero { isInCombat: not false } hero))
			{
				continue;
			}
			list.Clear();
			Check(hero.Skill.Q);
			Check(hero.Skill.W);
			Check(hero.Skill.E);
			Check(hero.Skill.R);
			Check(hero.Skill.Movement);
			if (list.Count <= 0)
			{
				continue;
			}
			list.Shuffle();
			SkillTrigger skillTrigger = list[0];
			CastInfo castInfo = new CastInfo(victim);
			switch (skillTrigger.currentConfig.castMethod.type)
			{
			case CastMethodType.Cone:
			case CastMethodType.Arrow:
				castInfo.angle = UnityEngine.Random.Range(0f, 360f);
				break;
			case CastMethodType.Target:
			{
				List<Entity> list2 = DewPhysics.OverlapCircleAllEntities(out var handle, victim.agentPosition, skillTrigger.currentConfig.castMethod._range, skillTrigger.currentConfig.targetValidator, victim);
				if (list2.Count == 0)
				{
					handle.Return();
					continue;
				}
				castInfo.target = list2[UnityEngine.Random.Range(0, list2.Count)];
				handle.Return();
				break;
			}
			case CastMethodType.Point:
				castInfo.point = Dew.GetPositionOnGround(victim.agentPosition + UnityEngine.Random.insideUnitCircle.ToXZ() * skillTrigger.currentConfig.castMethod._range);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case CastMethodType.None:
				break;
			}
			victim.Control.Cast(skillTrigger, castInfo, allowMoveToCast: false);
		}
		void Check(SkillTrigger s)
		{
			if (!s.IsNullOrInactive() && s.CanBeCast() && s.CanBeReserved())
			{
				list.Add(s);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}

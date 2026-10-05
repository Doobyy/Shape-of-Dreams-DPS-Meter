using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Star_Husk_F_DC_EndMovespeedToBonusAttackspeed : StarEffect
{
	private readonly List<(Se_R_Deception se, Action<Actor> handler)> _deceptionDestroyedSubs = new List<(Se_R_Deception, Action<Actor>)>();

	public override Type heroType => typeof(Hero_Husk);

	public override Type skillType => typeof(St_R_Deception);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			((Hero)victim).EntityEvent_OnCastCompleteBeforePrepare += new Action<EventInfoCast>(EntityEventOnCastCompleteBeforePrepare);
		}
	}

	private void EntityEventOnCastCompleteBeforePrepare(EventInfoCast obj)
	{
		AbilityInstance instance = obj.instance;
		Se_R_Deception se = instance as Se_R_Deception;
		if (se == null)
		{
			return;
		}
		Action<Actor> action = (Actor _) =>
		{
			float amount = se.GetValue(se.speedAmount);
			CreateStatusEffect(victim, new CastInfo(victim), (Se_Star_Husk_F_DC_EndMovespeedToBonusAttackspeed_Haste seHaste) =>
			{
				seHaste.hasteAmount = amount;
			});
		};
		se.ClientActorEvent_OnDestroyed += action;
		_deceptionDestroyedSubs.Add((se, action));
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		foreach (var (se_R_Deception, action) in _deceptionDestroyedSubs)
		{
			if ((UnityEngine.Object)(object)se_R_Deception != null)
			{
				se_R_Deception.ClientActorEvent_OnDestroyed -= action;
			}
		}
		_deceptionDestroyedSubs.Clear();
		if ((UnityEngine.Object)(object)(Hero)victim != null)
		{
			((Hero)victim).EntityEvent_OnCastCompleteBeforePrepare -= new Action<EventInfoCast>(EntityEventOnCastCompleteBeforePrepare);
		}
	}

	private void MirrorProcessed()
	{
	}
}

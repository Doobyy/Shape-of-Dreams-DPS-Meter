using System;
using Mirror;
using UnityEngine;

public class Se_Star_Husk_F_DC_EnhanceMaxStealthTime : StarEffect
{
	public float addedStealthTime = 0.25f;

	public override Type heroType => typeof(Hero_Husk);

	public override Type skillType => typeof(St_R_Deception);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.EntityEvent_OnCastCompleteBeforePrepare += new Action<EventInfoCast>(EntityEventOnCastCompleteBeforePrepare);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.EntityEvent_OnCastCompleteBeforePrepare -= new Action<EventInfoCast>(EntityEventOnCastCompleteBeforePrepare);
		}
	}

	private void EntityEventOnCastCompleteBeforePrepare(EventInfoCast obj)
	{
		if (obj.instance is Se_R_Deception se_R_Deception)
		{
			se_R_Deception.addedStealthTime += (float)(se_R_Deception.skillLevel - 1) * addedStealthTime;
		}
	}

	private void MirrorProcessed()
	{
	}
}

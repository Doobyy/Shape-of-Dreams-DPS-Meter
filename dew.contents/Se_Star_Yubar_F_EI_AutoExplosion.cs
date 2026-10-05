using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Star_Yubar_F_EI_AutoExplosion : StarEffect
{
	public int requiredLightStacks = 3;

	public float explosionDamageRatio = 0.5f;

	public override Type heroType => typeof(Hero_Yubar);

	public override Type skillType => typeof(St_Q_EtherealInfluence);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.EntityEvent_OnCastCompleteBeforePrepare += new Action<EventInfoCast>(EntityEventOnCastCompleteBeforePrepare);
		}
	}

	private void EntityEventOnCastCompleteBeforePrepare(EventInfoCast obj)
	{
		if (!(obj.instance is Ai_Q_EtherealInfluence ai_Q_EtherealInfluence))
		{
			return;
		}
		List<Entity> affected = new List<Entity>();
		ai_Q_EtherealInfluence.dealtDamageProcessor.Add(delegate(ref DamageData data, Actor from, Entity to)
		{
			if (from is IEtherealPathDamage && to.Status.lightStack >= requiredLightStacks && !affected.Contains(to))
			{
				affected.Add(to);
				Ai_Q_EtherealInfluence ai_Q_EtherealInfluence2 = from.FindFirstOfType<Ai_Q_EtherealInfluence>();
				if ((UnityEngine.Object)(object)ai_Q_EtherealInfluence2 != null)
				{
					ai_Q_EtherealInfluence2.Explode(explosionDamageRatio);
				}
			}
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (bool)(UnityEngine.Object)(object)victim)
		{
			victim.EntityEvent_OnCastCompleteBeforePrepare -= new Action<EventInfoCast>(EntityEventOnCastCompleteBeforePrepare);
		}
	}

	private void MirrorProcessed()
	{
	}
}

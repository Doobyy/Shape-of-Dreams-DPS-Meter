using System;
using Mirror;
using UnityEngine;

public class Se_Star_Yubar_F_SN_PullEnemy : StarEffect
{
	public float slowAmpMultiplier = 0.25f;

	public override Type heroType => typeof(Hero_Yubar);

	public override Type skillType => typeof(St_Q_SuperNova);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(OnBeforePrepare);
			victim.ActorEvent_OnAbilityInstanceCreated += new Action<EventInfoAbilityInstance>(OnCreated);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)victim == null))
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(OnBeforePrepare);
			victim.ActorEvent_OnAbilityInstanceCreated -= new Action<EventInfoAbilityInstance>(OnCreated);
		}
	}

	private void OnBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Se_Q_SuperNova_Slow se_Q_SuperNova_Slow)
		{
			se_Q_SuperNova_Slow.slowAmount *= 1f + slowAmpMultiplier;
		}
	}

	private void OnCreated(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_SuperNova ai_Q_SuperNova)
		{
			Vector3 point = ai_Q_SuperNova.info.point;
			CreateAbilityInstance<Ai_Star_Yubar_F_SN_Pull>(point, null, new CastInfo(hero, point));
		}
	}

	private void MirrorProcessed()
	{
	}
}

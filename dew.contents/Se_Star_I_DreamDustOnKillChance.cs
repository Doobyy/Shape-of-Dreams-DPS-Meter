using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Se_Star_I_DreamDustOnKillChance : StarEffect
{
	public int gainedAmount;

	public StarScalingValue gainChance;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ClientHeroEvent_OnKillOrAssist += new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)hero == null))
		{
			hero.ClientHeroEvent_OnKillOrAssist -= new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
		}
	}

	private void ClientHeroEventOnKillOrAssist(EventInfoKill obj)
	{
		Vector3 pos;
		int am;
		if (obj.victim is Monster && !(UnityEngine.Random.value > GetValue(gainChance)))
		{
			pos = obj.victim.position;
			am = gainedAmount;
			((MonoBehaviour)(object)NetworkedManagerBase<GameManager>.instance).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(0.7f);
			if (!NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition && !((UnityEngine.Object)(object)victim == null) && !((UnityEngine.Object)(object)victim.owner == null))
			{
				victim.owner.TpcShowWorldPopMessage(new WorldMessageSetting
				{
					rawText = DewLocalization.GetUIValue("Se_Star_I_DreamDustOnKillChance_BonusText"),
					color = new Color(0.6f, 0.9f, 1f, 1f),
					worldPos = pos
				});
				NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, am, pos, (Hero)victim);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}

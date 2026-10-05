using System;
using System.Collections;
using UnityEngine;

public class Star_Global_KillDreamDustBonus : DewStarItemOld
{
	public static readonly int GainedAmount = 2;

	public static readonly float[] GainChance = new float[5] { 0.01f, 0.02f, 0.03f, 0.04f, 0.05f };

	public override int maxLevel => 5;

	public override bool ShouldInitInGame()
	{
		return isServer;
	}

	public override void OnStartInGame()
	{
		base.OnStartInGame();
		hero.ClientHeroEvent_OnKillOrAssist += new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
	}

	private void ClientHeroEventOnKillOrAssist(EventInfoKill obj)
	{
		Vector3 pos;
		int am;
		if (obj.victim is Monster && !(UnityEngine.Random.value > GetDropChance()))
		{
			pos = obj.victim.position;
			am = GetDroppedAmount();
			((MonoBehaviour)(object)NetworkedManagerBase<GameManager>.instance).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(0.7f);
			if (!NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition && !((UnityEngine.Object)(object)hero == null) && !((UnityEngine.Object)(object)hero.owner == null))
			{
				hero.owner.TpcShowWorldPopMessage(new WorldMessageSetting
				{
					rawText = DewLocalization.GetUIValue("Star_Global_KillDreamDustBonus_BonusText"),
					color = new Color(0.6f, 0.9f, 1f, 1f),
					worldPos = pos
				});
				NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, am, pos, hero);
			}
		}
	}

	private float GetDropChance()
	{
		return GainChance.GetClamped(level - 1);
	}

	private int GetDroppedAmount()
	{
		return GainedAmount;
	}
}

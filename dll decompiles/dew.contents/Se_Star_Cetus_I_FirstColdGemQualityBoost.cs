using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Se_Star_Cetus_I_FirstColdGemQualityBoost : StarEffect
{
	public StarScalingValue bonusQuality;

	private bool _consumed;

	public override Type heroType => typeof(Hero_Cetus);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.Skill.ClientHeroEvent_OnGemEquip += new Action<Gem>(ClientHeroEventOnGemEquip);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)hero == null))
		{
			hero.Skill.ClientHeroEvent_OnGemEquip -= new Action<Gem>(ClientHeroEventOnGemEquip);
		}
	}

	private void ClientHeroEventOnGemEquip(Gem obj)
	{
		int upgradeAmount;
		if (!_consumed && obj.tags.HasFlag(DescriptionTags.Cold) && !(obj is IIgnoreFirstGemUpgrade))
		{
			_consumed = true;
			upgradeAmount = Mathf.RoundToInt(GetValue(bonusQuality) * 100f);
			Destroy();
			LockDestroy();
			((MonoBehaviour)(object)hero).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(0.5f);
			if (obj.IsNullOrInactive() || hero.IsNullInactiveDeadOrKnockedOut())
			{
				UnlockDestroy();
			}
			else
			{
				hero.CreateAbilityInstance(obj.position, null, new CastInfo(hero), (Ai_FirstBonusQualityUpgrader ai) =>
				{
					ai.target = obj;
					ai.upgradeAmount = upgradeAmount;
				});
				UnlockDestroy();
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}

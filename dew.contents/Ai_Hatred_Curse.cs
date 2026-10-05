using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Ai_Hatred_Curse : PunishmentInstance
{
	public GameObject effectOnTarget;

	protected override void OnCreate()
	{
		base.OnCreate();
		FxPlay(effectOnTarget, info.target);
		if (((NetworkBehaviour)this).isServer)
		{
			CurseStatusEffect curseStatusEffect = ChooseCurse(info.target);
			if (info.target.Status.TryGetStatusEffect(((object)curseStatusEffect).GetType(), out var effect))
			{
				effect.Destroy();
			}
			CreateStatusEffect(curseStatusEffect, info.target, new CastInfo(null, info.target), (CurseStatusEffect c) =>
			{
				c.currentStrength = HatredStrengthType.Potent;
				c.progressType = QuestProgressType.Kills;
				c.requiredAmount = 25;
			});
			Destroy();
		}
	}

	public CurseStatusEffect ChooseCurse(Entity target)
	{
		List<CurseStatusEffect> list = new List<CurseStatusEffect>();
		CurseStatusEffect[] array = (from c in DewResources.FindAllByTypeSubstring<CurseStatusEffect>("Se_Curse_", default(ResourceLoadSettings))
			where Dew.IsCurseIncludedInGame(((object)c).GetType().Name)
			select c).ToArray();
		foreach (CurseStatusEffect curseStatusEffect in array)
		{
			if (curseStatusEffect.IsViable(target))
			{
				list.Add(curseStatusEffect);
			}
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return Dew.SelectRandomWeightedInList(list, (CurseStatusEffect c) => c.chanceWeight, null);
	}

	private void MirrorProcessed()
	{
	}
}

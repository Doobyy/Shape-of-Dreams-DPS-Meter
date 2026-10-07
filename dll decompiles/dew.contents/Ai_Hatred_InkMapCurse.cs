using Mirror;
using UnityEngine;

public class Ai_Hatred_InkMapCurse : PunishmentInstance
{
	public GameObject effectOnTarget;

	protected override void OnCreate()
	{
		base.OnCreate();
		FxPlay(effectOnTarget, info.target);
		if (((NetworkBehaviour)this).isServer)
		{
			Se_Curse_DarkUrge byType = DewResources.GetByType<Se_Curse_DarkUrge>(default(ResourceLoadSettings));
			CurseStatusEffect curseStatusEffect = ((!byType.IsViable(info.target)) ? DewResources.GetByType<Ai_Hatred_Curse>(default(ResourceLoadSettings)).ChooseCurse(info.target) : byType);
			if (info.target.Status.TryGetStatusEffect(((object)curseStatusEffect).GetType(), out var effect))
			{
				effect.Destroy();
			}
			CreateStatusEffect(curseStatusEffect, info.target, new CastInfo(null, info.target));
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}

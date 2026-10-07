using System.Collections.Generic;
using UnityEngine;

public class Gem_C_Talc : Gem
{
	private struct Ad_Talc_Affected
	{
		public List<Entity> entities;
	}

	public ScalingValue addedDamage;

	public GameObject castEffect;

	public GameObject hitEffect;

	protected override void OnCastCompleteBeforePrepare(EventInfoCast info)
	{
		base.OnCastCompleteBeforePrepare(info);
		if (IsReady() && info.trigger.configs[info.configIndex].canConsumeCastBonus)
		{
			info.instance.dealtDamageProcessor.Add(DamageAmp, -1);
			info.instance.AddData(new Ad_Talc_Affected
			{
				entities = new List<Entity>()
			});
			NotifyUse();
			StartCooldown();
			FxPlayNewNetworked(castEffect, owner);
		}
	}

	private void DamageAmp(ref DamageData data, Actor actor, Entity target)
	{
		if (isValid && !data.IsAmountModifiedBy(this) && owner.CheckEnemyOrNeutral(target))
		{
			Ad_Talc_Affected data2;
			while (!actor.TryGetData<Ad_Talc_Affected>(out data2) && (Object)(object)actor.parentActor != null)
			{
				actor = actor.parentActor;
			}
			if (data2.entities != null && !data2.entities.Contains(target))
			{
				data2.entities.Add(target);
				data.SetAttr(DamageAttribute.IsCrit);
				data.AddFlatAmount(GetValue(addedDamage));
				data.SetAmountModifiedBy(this);
				FxPlayNewNetworked(hitEffect, target);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}

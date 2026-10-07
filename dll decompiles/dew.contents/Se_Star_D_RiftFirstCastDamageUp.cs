using System;
using Mirror;
using UnityEngine;

public class Se_Star_D_RiftFirstCastDamageUp : StarEffect
{
	public StarScalingValue firstSkillDamageAmp;

	[SaveVar(SaveVarFlags.Default)]
	private bool _isReady;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(OnRoomLoaded);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((bool)(UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance)
			{
				NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(OnRoomLoaded);
			}
			if (victim is Hero hero)
			{
				hero.EntityEvent_OnCastCompleteBeforePrepare -= new Action<EventInfoCast>(OnEntityEventOnCastCompleteBeforePrepare);
			}
		}
	}

	private void OnRoomLoaded(EventInfoLoadRoom loadInfo)
	{
		if (loadInfo.isTraveling && !_isReady)
		{
			showIcon = true;
			_isReady = true;
			((Hero)victim).EntityEvent_OnCastCompleteBeforePrepare += new Action<EventInfoCast>(OnEntityEventOnCastCompleteBeforePrepare);
		}
	}

	private void OnEntityEventOnCastCompleteBeforePrepare(EventInfoCast cast)
	{
		if (cast.trigger is SkillTrigger { skillType: not HeroSkillLocation.Movement })
		{
			cast.instance.dealtDamageProcessor.Add(Processer);
			showIcon = false;
			_isReady = false;
			((Hero)victim).EntityEvent_OnCastCompleteBeforePrepare -= new Action<EventInfoCast>(OnEntityEventOnCastCompleteBeforePrepare);
		}
	}

	private void Processer(ref DamageData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this))
		{
			data.SetAttr(DamageAttribute.IsCrit);
			data.ApplyAmplification(GetValue(firstSkillDamageAmp));
			data.SetAmountModifiedBy(this);
		}
	}

	private void MirrorProcessed()
	{
	}
}

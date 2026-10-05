using System;
using Mirror;
using UnityEngine;

public class Se_R_SanctuaryOfEl_Buff : StatusEffect
{
	public float duration;

	public ScalingValue hasteAmount;

	public bool doInvulnerable;

	public ScalingValue armorAmount;

	public float selfMultiplier;

	[NonSerialized]
	public bool disableUnstoppable;

	[NonSerialized]
	public bool disableArmor;

	private OnScreenTimerHandle _handle;

	private ScalingValue _baseHasteAmount;

	private ScalingValue _baseArmorAmount;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseHasteAmount = hasteAmount;
		_baseArmorAmount = armorAmount;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		hasteAmount = _baseHasteAmount;
		armorAmount = _baseArmorAmount;
		doInvulnerable = false;
		disableUnstoppable = false;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)victim).isOwned && (UnityEngine.Object)(object)info.caster != null && !((NetworkBehaviour)info.caster).isOwned)
		{
			Ai_R_SanctuaryOfEl_Ground ground = FindFirstAncestorOfType<Ai_R_SanctuaryOfEl_Ground>();
			_handle = ShowOnScreenTimerLocally(new OnScreenTimerHandle
			{
				fillAmountGetter = () => (!((UnityEngine.Object)(object)ground != null)) ? 0f : ground.fillAmount
			});
		}
		if (((NetworkBehaviour)this).isServer)
		{
			if (doInvulnerable)
			{
				DoInvulnerable();
			}
			if ((UnityEngine.Object)(object)victim == (UnityEngine.Object)(object)info.caster && !disableUnstoppable)
			{
				DoUnstoppable();
			}
			float num = GetValue(armorAmount) * (((UnityEngine.Object)(object)victim == (UnityEngine.Object)(object)info.caster) ? selfMultiplier : 1f);
			if (num > 0f && !disableArmor)
			{
				DoArmorBoost(num);
			}
			DoHaste(GetValue(hasteAmount) * (((UnityEngine.Object)(object)victim == (UnityEngine.Object)(object)info.caster) ? selfMultiplier : 1f));
			SetTimer(duration);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_handle != null)
		{
			HideOnScreenTimerLocally(_handle);
			_handle = null;
		}
	}

	private void MirrorProcessed()
	{
	}
}

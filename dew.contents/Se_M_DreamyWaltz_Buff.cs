using System;
using Mirror;

public class Se_M_DreamyWaltz_Buff : StatusEffect
{
	public float duration;

	public float shieldRatio;

	public float hasteAmount;

	[NonSerialized]
	public float strengthMultiplier = 1f;

	private float _baseDuration;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseDuration = duration;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		duration = _baseDuration;
		strengthMultiplier = 1f;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			if (!(victim is Summon))
			{
				ShowOnScreenTimer();
			}
			SetTimer(duration);
			ShowOnScreenTimer();
			int num = ((!(victim is Summon)) ? 1 : 2);
			DoShield(shieldRatio * victim.maxHealth * (float)num * strengthMultiplier);
			DoHaste(hasteAmount * (float)num * strengthMultiplier);
		}
	}

	private void MirrorProcessed()
	{
	}
}

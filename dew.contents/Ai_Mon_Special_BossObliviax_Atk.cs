using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossObliviax_Atk : DashAttackInstance
{
	public GameObject fxMainRight;

	public GameObject fxMainLeft;

	protected override void OnCreate()
	{
		if (firstEntity is Mon_Special_BossObliviax mon_Special_BossObliviax)
		{
			mon_Special_BossObliviax._chainAttackRequestTime = float.NegativeInfinity;
		}
		startEffect = ((DewAnimationClip.GetEntryIndex(info.animSelectValue, 2) == 0) ? fxMainRight : fxMainLeft);
		base.OnCreate();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && Random.value < NetworkedManagerBase<GameManager>.instance.GetSpecialSkillChanceMultiplier() && firstEntity is Mon_Special_BossObliviax mon_Special_BossObliviax)
		{
			mon_Special_BossObliviax._chainAttackRequestTime = Time.time;
		}
	}

	private void MirrorProcessed()
	{
	}
}

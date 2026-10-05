using System;
using Mirror;
using UnityEngine;

public class Se_Mon_DarkCave_BossSeeker_TunnelVision_PlayerLight : StatusEffect
{
	public GameObject targetEffectOnAltSkill;

	[NonSerialized]
	public bool isAltSkillEffect;

	protected override void OnPrepare()
	{
		base.OnPrepare();
		if (isAltSkillEffect)
		{
			startEffectVictim = targetEffectOnAltSkill;
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDestroy(parentActor);
		}
	}

	private void MirrorProcessed()
	{
	}
}

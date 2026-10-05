using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_DarkCave_BossSeeker_YellowDiagram_MainInstance : InstantDamageInstance
{
	public float distance;

	public GameObject fxSpawnAudio;

	public GameObject fxTelegraph;

	public GameObject fxCastEnd;

	public DewAnimationClip clip;

	[NonSerialized]
	public bool isFirstInstance;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		isFirstInstance = false;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			FxPlayNetworked(fxTelegraph, position, rotation);
			if (isFirstInstance)
			{
				FxPlayNetworked(fxCastEnd, info.caster);
				info.caster.Animation.PlayAbilityAnimation(clip);
			}
			DestroyOnDeath(info.caster);
		}
		yield return base.OnCreateSequenced();
	}

	protected override void OnAfterDelay()
	{
		base.OnAfterDelay();
		if (isFirstInstance)
		{
			FxPlayNetworked(fxSpawnAudio);
		}
		Vector3 vector = position + distance * info.forward;
		CreateAbilityInstance<Ai_Mon_DarkCave_BossSeeker_YellowDiagram_SubInstance>(vector, rotation, new CastInfo(info.caster, CastInfo.GetAngle(rotation)));
	}

	private void MirrorProcessed()
	{
	}
}

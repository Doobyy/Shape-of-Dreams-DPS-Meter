using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Mon_SnowMountain_BossSkoll : BossMonster
{
	public float allowSwipeTimeout = 1f;

	internal float _allowSwipeTime = float.NegativeInfinity;

	public DewAnimationClip cutSceneAnimationClip;

	public float cutSceneAnimationDelay;

	public GameObject cutSceneEffect;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity);
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		if (((NetworkBehaviour)this).isServer)
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			float seconds = Visual.spawnDuration + cutSceneAnimationDelay;
			yield return new WaitForSeconds(seconds);
			if (ManagerBase<CameraManager>.instance.isPlayingCutscene)
			{
				FxPlayNetworked(cutSceneEffect, this);
				Animation.PlayAbilityAnimation(cutSceneAnimationClip);
				float rawClipDuration = Animation.abilityAnimStatus.rawClipDuration;
				Control.StartDaze(rawClipDuration);
			}
		}
	}

	protected override bool PoolAIUpdate(ref EntityAIContext context, PoolAbilityInfo abilityInfo)
	{
		if ((UnityEngine.Object)(object)context.targetEnemy == null)
		{
			return false;
		}
		if (abilityInfo.triggerInstance is At_Mon_SnowMountain_BossSkoll_Swipe)
		{
			if (Time.time - _allowSwipeTime < allowSwipeTimeout && AI.Helper_CanBeCast<At_Mon_SnowMountain_BossSkoll_Swipe>() && AI.Helper_IsTargetInRange<At_Mon_SnowMountain_BossSkoll_Swipe>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_SnowMountain_BossSkoll_Swipe>();
				_allowSwipeTime = float.NegativeInfinity;
				AI.Helper_CastAbilityAuto<At_Mon_SnowMountain_BossSkoll_Swipe>();
				return true;
			}
			return false;
		}
		return base.PoolAIUpdate(ref context, abilityInfo);
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((UnityEngine.Object)(object)context.targetEnemy == null))
		{
			AI.Helper_ChaseTarget();
		}
	}

	[Server]
	public void AllowSwipe()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Mon_SnowMountain_BossSkoll::AllowSwipe()' called when server was not active");
		}
		else
		{
			_allowSwipeTime = Time.time;
		}
	}

	public override Type GetUniqueReward()
	{
		return typeof(Gem_U_GlacialCore);
	}

	private void MirrorProcessed()
	{
	}
}

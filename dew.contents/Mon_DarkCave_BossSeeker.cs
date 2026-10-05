using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Mon_DarkCave_BossSeeker : BossMonster, IPrewarmMonsterContributor
{
	public float healthReducedBeforeBlink = 0.1f;

	public float blinkBarrageHealthRatio = 1f / 3f;

	public float chaserHealthThreshold = 1.01f;

	public float hallucinationHealthThreshold = 0.66666f;

	public float fissureHealthThreshold = 0.66666f;

	public float tunnelVisionHealthThreshold = 0.66666f;

	private float _lastBlinkNormalizedHealth = 1f;

	public void ContributeMonsterPrewarm(Dictionary<Monster, int> counts, int instanceCount)
	{
		Se_Mon_DarkCave_BossSeeker_Hallucination byType = DewResources.GetByType<Se_Mon_DarkCave_BossSeeker_Hallucination>(default(ResourceLoadSettings));
		Mon_DarkCave_SeekerHallucination byType2 = DewResources.GetByType<Mon_DarkCave_SeekerHallucination>(default(ResourceLoadSettings));
		if (!((UnityEngine.Object)(object)byType == null) && !((UnityEngine.Object)(object)byType2 == null))
		{
			counts.TryGetValue(byType2, out var value);
			counts[byType2] = value + byType.clonesCount * 3 * instanceCount;
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_lastBlinkNormalizedHealth = 1f;
			CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity);
		}
	}

	protected override bool PoolAIUpdate(ref EntityAIContext context, PoolAbilityInfo abilityInfo)
	{
		if ((UnityEngine.Object)(object)context.targetEnemy == null)
		{
			return false;
		}
		if (Status.HasStatusEffect<Se_Mon_DarkCave_BossSeeker_Hallucination>())
		{
			AI.Helper_ChaseTarget();
			return true;
		}
		if (Status.HasStatusEffect<Se_Mon_DarkCave_BossSeeker_TunnelVision>())
		{
			return true;
		}
		if (_lastBlinkNormalizedHealth - normalizedHealth >= healthReducedBeforeBlink)
		{
			if (normalizedHealth < blinkBarrageHealthRatio)
			{
				if (AI.Helper_CanBeCast<At_Mon_DarkCave_BossSeeker_BlinkBarrage>())
				{
					AI.Helper_CastAbilityAuto<At_Mon_DarkCave_BossSeeker_BlinkBarrage>();
					return true;
				}
			}
			else
			{
				switch (currentPoolIndex)
				{
				case 0:
					if (AI.Helper_CanBeCast<At_Mon_DarkCave_BossSeeker_Blink>())
					{
						AI.Helper_CastAbilityAuto<At_Mon_DarkCave_BossSeeker_Blink>();
						return true;
					}
					break;
				case 1:
					if (AI.Helper_CanBeCast<At_Mon_DarkCave_BossSeeker_Substitution>())
					{
						AI.Helper_CastAbilityAuto<At_Mon_DarkCave_BossSeeker_Substitution>();
						return true;
					}
					break;
				}
			}
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
	public void UpdateBlinkHealth()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Mon_DarkCave_BossSeeker::UpdateBlinkHealth()' called when server was not active");
		}
		else
		{
			_lastBlinkNormalizedHealth = normalizedHealth;
		}
	}

	public override Type GetUniqueReward()
	{
		return typeof(Gem_U_SoulPrison);
	}

	private void MirrorProcessed()
	{
	}
}

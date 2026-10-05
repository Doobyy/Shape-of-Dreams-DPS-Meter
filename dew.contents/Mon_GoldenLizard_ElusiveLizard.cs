using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Mon_GoldenLizard_ElusiveLizard : Monster, IMonsterNoScaling
{
	public float moveChanceOnDamage = 0.7f;

	public float moveChanceOnNearbyWalking = 0.4f;

	public float moveMinInterval = 0.5f;

	public float proximityTickInterval = 0.5f;

	public float proximityRange = 4f;

	public float hitChanceFromSummon = 0.01f;

	public float hitChanceFromMeleeAtk = 0.03f;

	public float hitChanceFromRangedAtk = 0.015f;

	public float hitChanceFromDamageOverTime = 0.005f;

	public float hitChanceFromOthers = 0.03f;

	public float lacertaHitChanceMultiplier = 10f;

	public float vesperHitChanceMultiplier = 0.4f;

	public float goldMin = 0.7f;

	public float goldMax = 1.3f;

	public int statOrbCount = 10;

	public int upgradedChompLevel = 5;

	private float _lastMoveTime;

	private float _lastProximityCheckTime;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		takenDamageProcessor.Add(delegate(ref DamageData data, Actor from, Entity to)
		{
			if (!(from is Ai_R_Chomp) && !(from is Ai_U_BigChomp))
			{
				float num = ((!(from is Summon) && !from.IsDescendantOf<Summon>()) ? ((!(from is MeleeAttackInstance) && !from.IsDescendantOf<MeleeAttackInstance>()) ? ((from is AttackProjectile || from.IsDescendantOf<AttackProjectile>()) ? hitChanceFromRangedAtk : ((!data.HasAttr(DamageAttribute.DamageOverTime)) ? hitChanceFromOthers : hitChanceFromDamageOverTime)) : hitChanceFromMeleeAtk) : hitChanceFromSummon);
				if (from.IsDescendantOf<Hero_Vesper>())
				{
					num *= vesperHitChanceMultiplier;
				}
				if (from.IsDescendantOf<Hero_Lacerta>())
				{
					num *= lacertaHitChanceMultiplier;
				}
				num *= data.procCoefficient;
				if (Status.hasStun)
				{
					num *= 100f;
				}
				else if (Status.hasRoot)
				{
					num *= 5f;
				}
				if (Random.value > num)
				{
					RpcShowDodgeText();
					data.ApplyRawMultiplier(0f);
				}
				if (Random.value < moveChanceOnDamage)
				{
					MoveNow();
				}
			}
		});
	}

	[ClientRpc]
	private void RpcShowDodgeText()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Mon_GoldenLizard_ElusiveLizard::RpcShowDodgeText()", -1673102365, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || Time.time - _lastProximityCheckTime < proximityTickInterval)
		{
			return;
		}
		_lastProximityCheckTime = Time.time;
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, agentPosition, proximityRange))
		{
			if (!((Object)(object)item == (Object)(object)this) && !(item.Control.walkStrength < 0.4f) && Random.value < moveChanceOnNearbyWalking)
			{
				MoveNow();
			}
		}
		handle.Return();
	}

	protected override void OnDeath(EventInfoKill info)
	{
		base.OnDeath(info);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		Actor actor = info.actor;
		if (actor is Ai_R_Chomp || actor is Ai_U_BigChomp)
		{
			Hero hero = info.actor.FindFirstOfType<Hero>();
			for (int i = 0; i < statOrbCount; i++)
			{
				CreateAbilityInstance(agentPosition, null, new CastInfo(hero), (Ai_Gem_E_Predation_Pickup p) =>
				{
					p._targetHero = hero;
				});
			}
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		int amount = DewMath.RandomRoundToInt(NetworkedManagerBase<GameManager>.instance.GetSpecialRewardAmount_Gold() * Random.Range(goldMin, goldMax));
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			if (!allHero.IsNullInactiveDeadOrKnockedOut())
			{
				NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: true, isGivenByOtherPlayer: false, amount, agentPosition, allHero);
			}
		}
		IEnumerator Routine()
		{
			LockDestroy();
			Vector3 pos = agentPosition;
			SkillTrigger skillTrigger = info.actor.FindFirstOfType<SkillTrigger>();
			if (!skillTrigger.IsNullOrInactive())
			{
				skillTrigger.level += upgradedChompLevel;
			}
			yield return new WaitForSeconds(1.25f);
			foreach (DewPlayer allHumanPlayer in DewPlayer.allHumanPlayers)
			{
				if (!allHumanPlayer.hero.IsNullInactiveDeadOrKnockedOut())
				{
					Dew.CreateSkillTrigger<St_E_LizardlyBlessing>(Dew.GetGoodRewardPosition(pos + (allHumanPlayer.hero.agentPosition - pos).normalized * 3f), NetworkedManagerBase<LootManager>.instance.SelectSkillLevel(Rarity.Epic), allHumanPlayer);
				}
			}
			UnlockDestroy();
		}
	}

	public void MoveNow()
	{
		if (!(Time.time - _lastMoveTime < moveMinInterval))
		{
			_lastMoveTime = Time.time;
			Vector3 destination = ((!(section != null)) ? Dew.GetValidAgentDestination_Closest(agentPosition, Dew.GetPositionOnGround(agentPosition + Random.insideUnitSphere * 8f)) : section.GetGoodWanderPosition(agentPosition));
			Control.MoveToDestination(destination, immediately: false);
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcShowDodgeText()
	{
		InGameUIManager.instance.ShowWorldPopMessage(new WorldMessageSetting
		{
			color = Color.white,
			rawText = DewLocalization.GetUIValue("Mon_GoldenLizard_ElusiveLizard_Dodged"),
			worldPos = Visual.GetCenterPosition()
		});
	}

	protected static void InvokeUserCode_RpcShowDodgeText(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowDodgeText called on server.");
		}
		else
		{
			((Mon_GoldenLizard_ElusiveLizard)(object)obj).UserCode_RpcShowDodgeText();
		}
	}

	static Mon_GoldenLizard_ElusiveLizard()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Mon_GoldenLizard_ElusiveLizard), "System.Void Mon_GoldenLizard_ElusiveLizard::RpcShowDodgeText()", (RemoteCallDelegate)InvokeUserCode_RpcShowDodgeText);
	}
}

using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class PickupManager : NetworkedManagerBase<PickupManager>
{
	public int largeGoldOrbAmount = 100;

	public int mediumGoldOrbAmount = 40;

	public int smallGoldOrbAmount = 10;

	public int goldOrbMaxCount = 7;

	public int largeExpOrbAmount = 50;

	public int mediumExpOrbAmount = 10;

	public int smallExpOrbAmount = 3;

	public int expOrbMaxCount = 7;

	public int dreamDustMaxCount = 5;

	public int dreamDustMinAmount = 10;

	public bool isStardustDropDisabled;

	private Action<EventInfoKill> _cachedHandleDeathDrop;

	private static int _pendingOrbAmount;

	private static Hero _pendingTarget;

	private static bool _pendingIsKillGold;

	private static bool _pendingIsGivenByOtherPlayer;

	private static readonly Action<Pickup_BaseExpOrb> _applyPendingExpOrb = (Pickup_BaseExpOrb p) =>
	{
		p.amount = _pendingOrbAmount;
	};

	private static readonly Action<Pickup_BaseGoldOrb> _applyPendingGoldOrb = (Pickup_BaseGoldOrb p) =>
	{
		p.amount = _pendingOrbAmount;
		p.target = _pendingTarget;
		p.isKillGold = _pendingIsKillGold;
		p.isGivenByOtherPlayer = _pendingIsGivenByOtherPlayer;
		_pendingTarget = null;
	};

	private static readonly Action<Pickup_DreamDust> _applyPendingDreamDust = (Pickup_DreamDust p) =>
	{
		p.amount = _pendingOrbAmount;
		p.target = _pendingTarget;
		p.isGivenByOtherPlayer = _pendingIsGivenByOtherPlayer;
		_pendingTarget = null;
	};

	public const int kTargetDropOrbCount = 3;

	public override void OnStartServer()
	{
		base.OnStartServer();
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd += new Action<Entity>(HandleEntityAdd);
	}

	private void HandleEntityAdd(Entity obj)
	{
		if (obj is Monster)
		{
			obj.EntityEvent_OnDeath += new Action<EventInfoKill>(HandleDeathDrop);
		}
	}

	[Server]
	public void DropStarDust(int amount, Vector3 position)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PickupManager::DropStarDust(System.Int32,UnityEngine.Vector3)' called when server was not active");
			return;
		}
		position = Dew.GetGoodRewardPosition(position);
		amount = DewMath.RandomRoundToInt((float)amount * DewBuildProfile.current.stardustGainMultiplier);
		if (amount > 0)
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			int remainingAmount = amount;
			while (remainingAmount > 0 && !NetworkedManagerBase<ZoneManager>.instance.isInRoomTransition)
			{
				int b = Mathf.Max(UnityEngine.Random.Range(2, 4), amount / 6);
				b = Mathf.Min(remainingAmount, b);
				remainingAmount -= b;
				Spawn(b);
				yield return new WaitForSeconds(UnityEngine.Random.Range(0.05f, 0.15f));
			}
		}
		void Spawn(int val)
		{
			Vector3 end = position + (UnityEngine.Random.onUnitSphere * 2.5f).Flattened();
			end = Dew.GetValidAgentDestination_LinearSweep(position, end);
			Dew.CreateActor(end, null, null, (Shrine_Stardust stardust) =>
			{
				stardust.amount = val;
			});
		}
	}

	[Server]
	public void DropDreamDust(bool isGivenByOtherPlayer, int amount, Vector3 position, Hero target = null)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PickupManager::DropDreamDust(System.Boolean,System.Int32,UnityEngine.Vector3,Hero)' called when server was not active");
			return;
		}
		int num = Mathf.Max(amount / dreamDustMaxCount, dreamDustMinAmount);
		while (amount > 0)
		{
			if (amount >= num)
			{
				Spawn(num);
				amount -= num;
			}
			else
			{
				Spawn(amount);
				amount = 0;
			}
		}
		void Spawn(int val)
		{
			_pendingOrbAmount = val;
			_pendingTarget = target;
			_pendingIsGivenByOtherPlayer = isGivenByOtherPlayer;
			NetworkedManagerBase<ActorManager>.instance.serverActor.CreatePickupInstance(position, null, default, _applyPendingDreamDust);
		}
	}

	private static bool IsUnderPerfPressure()
	{
		return GraphicsManager.WasLowFpsInLast5Seconds();
	}

	[Server]
	public void DropGold(bool isKillGold, bool isGivenByOtherPlayer, int amount, Vector3 position, Hero target = null)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PickupManager::DropGold(System.Boolean,System.Boolean,System.Int32,UnityEngine.Vector3,Hero)' called when server was not active");
		}
		else if (amount > 0)
		{
			int num = (IsUnderPerfPressure() ? 1 : 3);
			if (num <= 1)
			{
				SpawnGoldOrbForAmount(amount, isKillGold, isGivenByOtherPlayer, position, target);
			}
			else
			{
				((MonoBehaviour)(object)this).StartCoroutine(DropGoldRoutine(num, isKillGold, isGivenByOtherPlayer, amount, position, target));
			}
		}
	}

	private IEnumerator DropGoldRoutine(int orbCount, bool isKillGold, bool isGivenByOtherPlayer, int amount, Vector3 position, Hero target)
	{
		int perOrb = amount / orbCount;
		int remainder = amount - perOrb * orbCount;
		for (int i = 0; i < orbCount; i++)
		{
			int num = perOrb + ((i == 0) ? remainder : 0);
			if (num > 0)
			{
				SpawnGoldOrbForAmount(num, isKillGold, isGivenByOtherPlayer, position, target);
			}
			if (i < orbCount - 1)
			{
				yield return null;
			}
		}
	}

	private void SpawnGoldOrbForAmount(int orbAmount, bool isKillGold, bool isGivenByOtherPlayer, Vector3 position, Hero target)
	{
		if (orbAmount >= largeGoldOrbAmount)
		{
			_pendingOrbAmount = orbAmount;
			_pendingTarget = target;
			_pendingIsKillGold = isKillGold;
			_pendingIsGivenByOtherPlayer = isGivenByOtherPlayer;
			NetworkedManagerBase<ActorManager>.instance.serverActor.CreatePickupInstance(position, (Quaternion?)null, default(CastInfo), (Action<Pickup_LargeGoldOrb>)_applyPendingGoldOrb);
		}
		else if (orbAmount >= mediumGoldOrbAmount)
		{
			_pendingOrbAmount = orbAmount;
			_pendingTarget = target;
			_pendingIsKillGold = isKillGold;
			_pendingIsGivenByOtherPlayer = isGivenByOtherPlayer;
			NetworkedManagerBase<ActorManager>.instance.serverActor.CreatePickupInstance(position, (Quaternion?)null, default(CastInfo), (Action<Pickup_MediumGoldOrb>)_applyPendingGoldOrb);
		}
		else
		{
			_pendingOrbAmount = orbAmount;
			_pendingTarget = target;
			_pendingIsKillGold = isKillGold;
			_pendingIsGivenByOtherPlayer = isGivenByOtherPlayer;
			NetworkedManagerBase<ActorManager>.instance.serverActor.CreatePickupInstance(position, (Quaternion?)null, default(CastInfo), (Action<Pickup_SmallGoldOrb>)_applyPendingGoldOrb);
		}
	}

	[Server]
	public void DropExp(int amount, Vector3 position)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PickupManager::DropExp(System.Int32,UnityEngine.Vector3)' called when server was not active");
		}
		else if (amount > 0)
		{
			int num = (IsUnderPerfPressure() ? 1 : 3);
			if (num <= 1)
			{
				SpawnExpOrbForAmount(amount, position);
			}
			else
			{
				((MonoBehaviour)(object)this).StartCoroutine(DropExpRoutine(num, amount, position));
			}
		}
	}

	private IEnumerator DropExpRoutine(int orbCount, int amount, Vector3 position)
	{
		int perOrb = amount / orbCount;
		int remainder = amount - perOrb * orbCount;
		for (int i = 0; i < orbCount; i++)
		{
			int num = perOrb + ((i == 0) ? remainder : 0);
			if (num > 0)
			{
				SpawnExpOrbForAmount(num, position);
			}
			if (i < orbCount - 1)
			{
				yield return null;
			}
		}
	}

	private void SpawnExpOrbForAmount(int orbAmount, Vector3 position)
	{
		if (orbAmount >= largeExpOrbAmount)
		{
			_pendingOrbAmount = orbAmount;
			NetworkedManagerBase<ActorManager>.instance.serverActor.CreatePickupInstance(position, (Quaternion?)null, default(CastInfo), (Action<Pickup_LargeExpOrb>)_applyPendingExpOrb);
		}
		else if (orbAmount >= mediumExpOrbAmount)
		{
			_pendingOrbAmount = orbAmount;
			NetworkedManagerBase<ActorManager>.instance.serverActor.CreatePickupInstance(position, (Quaternion?)null, default(CastInfo), (Action<Pickup_MediumExpOrb>)_applyPendingExpOrb);
		}
		else
		{
			_pendingOrbAmount = orbAmount;
			NetworkedManagerBase<ActorManager>.instance.serverActor.CreatePickupInstance(position, (Quaternion?)null, default(CastInfo), (Action<Pickup_SmallExpOrb>)_applyPendingExpOrb);
		}
	}

	private void HandleDeathDrop(EventInfoKill obj)
	{
		if ((obj.victim.Status.TryGetStatusEffect<Se_HunterBuff>(out var effect) && !effect.enableGoldAndExpDrops) || obj.victim is Monster { disableLoot: not false })
		{
			return;
		}
		DewGameplayExperienceSettings ges = NetworkedManagerBase<GameManager>.instance.ges;
		bool flag = IsUnderPerfPressure();
		int killGoldAmount = NetworkedManagerBase<GameManager>.instance.GetKillGoldAmount(obj.victim);
		if (flag)
		{
			Pickup_BaseGoldOrb.GrantGold(killGoldAmount, null, isKillGold: true, isGivenByOtherPlayer: false);
		}
		else
		{
			DropGold(isKillGold: true, isGivenByOtherPlayer: false, killGoldAmount, obj.victim.position);
		}
		for (int i = 0; i < DewPlayer.gamePlayers.Count; i++)
		{
			Hero hero = DewPlayer.gamePlayers[i].hero;
			if (!((UnityEngine.Object)(object)hero == null) && hero.level < hero.maxLevel)
			{
				int num = (int)NetworkedManagerBase<GameManager>.instance.GetExpDropFromEntity(obj.victim);
				if (flag)
				{
					Pickup_BaseExpOrb.GrantExp(num);
				}
				else
				{
					DropExp(num, obj.victim.position);
				}
				break;
			}
		}
		if (!(obj.victim is Monster monster2))
		{
			return;
		}
		float num2 = 0f;
		int num3 = 0;
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			if (!gamePlayer.hero.IsNullOrInactive() && !gamePlayer.hero.isKnockedOut)
			{
				num2 += gamePlayer.hero.currentHealth / gamePlayer.hero.maxHealth;
				num3++;
			}
		}
		float num4 = Mathf.Lerp(t: (num3 != 0) ? (num2 / (float)num3) : 1f, a: ges.regenOrbDropChanceOnLowHealth.Get(monster2.type), b: ges.regenOrbDropChanceOnMaxHealth.Get(monster2.type)) * NetworkedManagerBase<GameManager>.instance.difficulty.regenOrbChanceMultiplier;
		Hero hero2 = obj.actor.FindFirstOfType<Hero>();
		if ((UnityEngine.Object)(object)hero2 != null && (UnityEngine.Object)(object)hero2.owner != null && hero2.owner.isHumanPlayer)
		{
			num4 *= hero2.owner.potionDropChanceMultiplier;
		}
		if (UnityEngine.Random.value < num4)
		{
			NetworkedManagerBase<ActorManager>.instance.serverActor.CreatePickupInstance<Pickup_RegenOrb>(Dew.GetGoodRewardPosition(monster2.position), null, default);
		}
		if (!isStardustDropDisabled && UnityEngine.Random.value < ges.stardustDeathDropChance.Get(monster2.type))
		{
			int amount = UnityEngine.Random.Range(ges.stardustDeathDropAmount.x, ges.stardustDeathDropAmount.y + 1);
			DropStarDust(amount, obj.victim.position);
		}
	}

	private void MirrorProcessed()
	{
	}
}

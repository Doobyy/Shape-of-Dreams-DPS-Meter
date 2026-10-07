using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Shrine_HiddenStash : Shrine
{
	public GameObject fxJackPot;

	public int jackPotScore;

	private Dictionary<DewPlayer, int> _playerScore = new Dictionary<DewPlayer, int>();

	protected override bool OnUse(Entity entity)
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		return true;
		void DropGem()
		{
			foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
			{
				if (!gamePlayer.hero.IsNullInactiveDeadOrKnockedOut())
				{
					Rarity rarity = NetworkedManagerBase<LootManager>.instance.SelectGemRarity(isHigh: true);
					Vector3 goodRewardPosition = Dew.GetGoodRewardPosition(P_0.pivot);
					NetworkedManagerBase<LootManager>.instance.SelectGemAndQuality(rarity, out var gem, out var quality);
					Dew.CreateGem(gem, goodRewardPosition, quality, gamePlayer);
					if (!_playerScore.ContainsKey(gamePlayer))
					{
						_playerScore.Add(gamePlayer, (int)rarity);
					}
					else
					{
						_playerScore[gamePlayer] += (int)rarity;
					}
				}
			}
		}
		void DropSkill()
		{
			foreach (DewPlayer gamePlayer2 in DewPlayer.gamePlayers)
			{
				if (!gamePlayer2.hero.IsNullInactiveDeadOrKnockedOut())
				{
					Rarity rarity = NetworkedManagerBase<LootManager>.instance.SelectSkillRarity(isHigh: true);
					Vector3 goodRewardPosition = Dew.GetGoodRewardPosition(P_0.pivot);
					NetworkedManagerBase<LootManager>.instance.SelectSkillAndLevel(rarity, out var skill, out var level);
					Dew.CreateSkillTrigger(skill, goodRewardPosition, level, gamePlayer2);
					if (!_playerScore.ContainsKey(gamePlayer2))
					{
						_playerScore.Add(gamePlayer2, (int)rarity);
					}
					else
					{
						_playerScore[gamePlayer2] += (int)rarity;
					}
				}
			}
		}
		IEnumerator Routine()
		{
			Vector3 pivot = GetRandomSpawnPosition(entity.position);
			yield return new WaitForSeconds(0.5f);
			if (this.IsNullOrInactive() || NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
			{
				yield break;
			}
			switch (Random.Range(0, 6))
			{
			case 0:
				DropSkill();
				DropSkill();
				DropSkill();
				break;
			case 1:
				DropSkill();
				DropGem();
				DropGem();
				break;
			case 2:
				DropGem();
				DropGem();
				DropGem();
				break;
			case 3:
			{
				int amount2 = DewMath.RandomRoundToInt(NetworkedManagerBase<GameManager>.instance.GetSpecialRewardAmount_Gold() * Random.Range(0.8f, 1.5f)) * DewPlayer.gamePlayers.Count;
				NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, amount2, position);
				DropGem();
				break;
			}
			case 4:
			{
				int amount = DewMath.RandomRoundToInt(NetworkedManagerBase<GameManager>.instance.GetSpecialRewardAmount_Gold() * Random.Range(0.8f, 1.5f));
				NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, amount, position);
				DropGem();
				break;
			}
			default:
				foreach (DewPlayer gamePlayer3 in DewPlayer.gamePlayers)
				{
					if (!gamePlayer3.hero.IsNullInactiveDeadOrKnockedOut())
					{
						Vector3 goodRewardPosition = Dew.GetGoodRewardPosition(pivot);
						NetworkedManagerBase<LootManager>.instance.SelectSkillAndLevel(Rarity.Rare, out var _, out var level);
						level += 5;
						Dew.CreateSkillTrigger<St_C_Sneeze>(goodRewardPosition, level, gamePlayer3);
					}
				}
				break;
			}
			foreach (DewPlayer gamePlayer4 in DewPlayer.gamePlayers)
			{
				if (!gamePlayer4.hero.IsNullInactiveDeadOrKnockedOut() && _playerScore.ContainsKey(gamePlayer4) && _playerScore[gamePlayer4] >= jackPotScore)
				{
					RpcPlayJackPotEffect(gamePlayer4);
				}
			}
		}
	}

	[ClientRpc]
	private void RpcPlayJackPotEffect(DewPlayer h)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)h);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Shrine_HiddenStash::RpcPlayJackPotEffect(DewPlayer)", 29922873, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcPlayJackPotEffect__DewPlayer(DewPlayer h)
	{
		if (!((Object)(object)h != (Object)(object)DewPlayer.local))
		{
			FxPlayNew(fxJackPot, ((Component)(object)this).transform.position, null);
		}
	}

	protected static void InvokeUserCode_RpcPlayJackPotEffect__DewPlayer(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayJackPotEffect called on server.");
		}
		else
		{
			((Shrine_HiddenStash)(object)obj).UserCode_RpcPlayJackPotEffect__DewPlayer(NetworkReaderExtensions.ReadNetworkBehaviour<DewPlayer>(reader));
		}
	}

	static Shrine_HiddenStash()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_HiddenStash), "System.Void Shrine_HiddenStash::RpcPlayJackPotEffect(DewPlayer)", (RemoteCallDelegate)InvokeUserCode_RpcPlayJackPotEffect__DewPlayer);
	}
}

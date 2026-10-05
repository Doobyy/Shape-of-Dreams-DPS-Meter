using System;
using System.Linq;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Se_Star_Husk_I_ShopIntimidation : StarEffect
{
	public GameObject fxPrepareHusk;

	public GameObject fxActivateHusk;

	public GameObject fxActivateShop;

	public float chance = 0.5f;

	public StarScalingValue reductionOnSuccess;

	public StarScalingValue ampOnFailure;

	private float _lastCheckTime;

	public override Type heroType => typeof(Hero_Husk);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd += new Action<Actor>(OnActorAdd);
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			OnActorAdd(allActor);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || Time.time - _lastCheckTime < 1f)
		{
			return;
		}
		_lastCheckTime = Time.time;
		if (hero.IsNullInactiveDeadOrKnockedOut() || hero.isInCombat || hero.Control.ongoingChannels.Count > 0 || hero.Control.isDisplacing)
		{
			return;
		}
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, hero.agentPosition, 4f))
		{
			if (item is PropEnt_Merchant_Jonas target && !item.persistentData.ContainsData("Se_Star_Husk_I_ShopIntimidation", "result", player.guid))
			{
				Intimidate(target);
				break;
			}
		}
		handle.Return();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd -= new Action<Actor>(OnActorAdd);
		}
	}

	private void OnActorAdd(Actor obj)
	{
		PropEnt_Merchant_Jonas jonas = obj as PropEnt_Merchant_Jonas;
		if (jonas == null)
		{
			return;
		}
		jonas.onMerchandisePopulated += (Action<DewPlayer>)((DewPlayer p) =>
		{
			if (!this.IsNullOrInactive() && !((UnityEngine.Object)(object)p != (UnityEngine.Object)(object)player))
			{
				UpdatePrice(jonas);
			}
		});
	}

	private void Intimidate(PropEnt_Merchant_Jonas target)
	{
		FxPlayNetworked(fxPrepareHusk, hero);
		hero.Control.RotateTowards(target, immediately: false, 1f);
		hero.Control.StartChannel(new Channel
		{
			duration = 1.25f,
			blockedActions = Channel.BlockedAction.Everything,
			onComplete = () =>
			{
				if (!target.IsNullInactiveDeadOrKnockedOut() && !hero.IsNullInactiveDeadOrKnockedOut())
				{
					FxPlayNetworked(fxActivateHusk, hero);
					FxPlayNetworked(fxActivateHusk, target);
					bool flag = UnityEngine.Random.value < chance;
					target.persistentData.SetData("Se_Star_Husk_I_ShopIntimidation", "result", player.guid, flag);
					UpdatePrice(target);
					RpcShowResult(flag, UnityEngine.Random.value < 0.1f, target);
				}
			}
		});
	}

	[ClientRpc]
	private void RpcShowResult(bool isSuccess, bool isCritical, Entity target)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, isSuccess);
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, isCritical);
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)target);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Se_Star_Husk_I_ShopIntimidation::RpcShowResult(System.Boolean,System.Boolean,Entity)", 1330871572, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void UpdatePrice(PropEnt_Merchant_Jonas target)
	{
		MerchandiseData[] source = default;
		if (target.persistentData.TryGetData<bool>("Se_Star_Husk_I_ShopIntimidation", "result", player.guid, out var value) && ((SyncIDictionary<string, MerchandiseData[]>)(object)target.merchandises).TryGetValue(player.guid, ref source))
		{
			MerchandiseData[] array = source.ToArray();
			float num = (value ? (1f - GetValue(reductionOnSuccess)) : (1f + GetValue(ampOnFailure)));
			for (int i = 0; i < array.Length; i++)
			{
				array[i].price = (Cost)(array[i].price * num);
			}
			((SyncIDictionary<string, MerchandiseData[]>)(object)target.merchandises)[player.guid] = array;
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcShowResult__Boolean__Boolean__Entity(bool isSuccess, bool isCritical, Entity target)
	{
		string key;
		if (isCritical)
		{
			key = (isSuccess ? "SkillCheck_CriticalSuccess" : "SkillCheck_CriticalFailure");
		}
		else
		{
			key = (isSuccess ? "SkillCheck_Success" : "SkillCheck_Failure");
		}
		InGameUIManager.instance.ShowWorldPopMessage(new WorldMessageSetting
		{
			rawText = DewLocalization.GetUIValue(key),
			worldPos = target.Visual.GetCenterPosition(),
			color = ((!isSuccess) ? new Color(1f, 0.7f, 0.7f) : Color.white)
		});
	}

	protected static void InvokeUserCode_RpcShowResult__Boolean__Boolean__Entity(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowResult called on server.");
		}
		else
		{
			((Se_Star_Husk_I_ShopIntimidation)(object)obj).UserCode_RpcShowResult__Boolean__Boolean__Entity(NetworkReaderExtensions.ReadBool(reader), NetworkReaderExtensions.ReadBool(reader), NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader));
		}
	}

	static Se_Star_Husk_I_ShopIntimidation()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Se_Star_Husk_I_ShopIntimidation), "System.Void Se_Star_Husk_I_ShopIntimidation::RpcShowResult(System.Boolean,System.Boolean,Entity)", (RemoteCallDelegate)InvokeUserCode_RpcShowResult__Boolean__Boolean__Entity);
	}
}

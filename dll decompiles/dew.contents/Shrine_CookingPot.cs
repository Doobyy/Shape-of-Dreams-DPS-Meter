using System;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Shrine_CookingPot : Shrine
{
	public static Shrine_CookingPot softInstance;

	public GameObject fxApply;

	[NonSerialized]
	public PropEnt_Merchant_Jonas potOwnJonas;

	private const string OfferCategory = "Shrine_CookingPot";

	private const string OfferKey = "offer";

	public static Shrine_CookingPot instance => Dew.Helper_GetInstanceOfActor(ref softInstance);

	public override Cost? GetCost(Entity activator)
	{
		return null;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (!(UnityEngine.Object)(object)potOwnJonas)
		{
			Destroy();
			return;
		}
		potOwnJonas.ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
		{
			MakeUnavailable();
		});
	}

	public override bool CanInteract(Entity entity)
	{
		if (base.CanInteract(entity) && entity is Hero hero)
		{
			return GetIngredientCount(hero) > 0;
		}
		return false;
	}

	protected override bool OnUse(Entity entity)
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			return false;
		}
		if (!(entity is Hero hero) || (UnityEngine.Object)(object)hero.owner == null)
		{
			return false;
		}
		int ingredientCount = GetIngredientCount(hero);
		if (ingredientCount <= 0)
		{
			return false;
		}
		if (!persistentData.TryGetData<int>("Shrine_CookingPot", "offer", hero.owner.guid, out var value))
		{
			int[] array = new int[6] { 0, 1, 2, 3, 4, 5 };
			array.Shuffle();
			value = array[0] * 100 + array[1];
			persistentData.SetData("Shrine_CookingPot", "offer", hero.owner.guid, value);
		}
		TargetOpenCook((NetworkConnection)(object)(NetworkConnectionToClient)hero.owner, value / 100, value % 100, ingredientCount);
		return true;
	}

	[TargetRpc]
	private void TargetOpenCook(NetworkConnection conn, int d0, int d1, int n)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, d0);
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, d1);
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, n);
		((NetworkBehaviour)this).SendTargetRPCInternal(conn, "System.Void Shrine_CookingPot::TargetOpenCook(Mirror.NetworkConnection,System.Int32,System.Int32,System.Int32)", -1481223395, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[Command(requiresAuthority = false)]
	private void CmdChooseDish(Hero hero, int dish, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)hero);
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, dish);
		((NetworkBehaviour)this).SendCommandInternal("System.Void Shrine_CookingPot::CmdChooseDish(Hero,System.Int32,Mirror.NetworkConnectionToClient)", 1359564107, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	private int GetIngredientCount(Hero hero)
	{
		if (!hero.Status.TryGetStatusEffect<Se_Gem_L_Culinary_Stack>(out var effect))
		{
			return 0;
		}
		return effect.stack;
	}

	private void ConsumeIngredients(Hero hero)
	{
		if (hero.Status.TryGetStatusEffect<Se_Gem_L_Culinary_Stack>(out var effect))
		{
			effect.SetStack(0);
		}
	}

	public static int GetAmount(int dish, int n)
	{
		return dish switch
		{
			0 => Mathf.Max(1, Mathf.CeilToInt(0.5f * (float)n)), 
			1 => Mathf.Max(1, Mathf.CeilToInt(0.5f * (float)n)), 
			2 => Mathf.Max(1, Mathf.CeilToInt(0.5f * (float)n)), 
			3 => Mathf.Max(1, Mathf.CeilToInt(1f * (float)n)), 
			4 => Mathf.Max(1, Mathf.CeilToInt(2f * (float)n)), 
			5 => Mathf.Max(1, Mathf.CeilToInt(3f * (float)n)), 
			_ => 1, 
		};
	}

	private string NameKey(int dish)
	{
		return dish switch
		{
			0 => "Shrine_CookingPot_OppressorTailSashimi", 
			1 => "Shrine_CookingPot_InkRiverLatte", 
			2 => "Shrine_CookingPot_CosmicDustSoup", 
			3 => "Shrine_CookingPot_SpicySpiderStirFry", 
			4 => "Shrine_CookingPot_LeafPuppySalad", 
			5 => "Shrine_CookingPot_GoldBarFry", 
			_ => "", 
		};
	}

	private string GetLabel(int dish, int n)
	{
		int amount = GetAmount(dish, n);
		string uIValue = DewLocalization.GetUIValue(NameKey(dish));
		return dish switch
		{
			0 => $"{uIValue} (<sprite=2> +{amount})", 
			1 => $"{uIValue} (<sprite=1> +{amount})", 
			2 => $"{uIValue} (<sprite=7> +{amount})", 
			3 => $"{uIValue} (<sprite=10> +{amount}%)", 
			4 => $"{uIValue} (<sprite=3> +{amount})", 
			5 => uIValue + " (+" + string.Format(DewLocalization.GetUIValue("Currency_Template_Gold"), amount) + ")", 
			_ => uIValue, 
		};
	}

	private void Apply(int dish, Hero hero, int n)
	{
		int amount = GetAmount(dish, n);
		FxPlay(fxApply);
		Se_Culinary_StatReward effect;
		if (dish == 5)
		{
			NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, amount, hero.position, hero);
		}
		else if (hero.Status.TryGetStatusEffect<Se_Culinary_StatReward>(out effect))
		{
			SetBonus(effect);
		}
		else
		{
			hero.CreateStatusEffect<Se_Culinary_StatReward>(hero, new CastInfo(hero), SetBonus);
		}
		void SetBonus(Se_Culinary_StatReward reward)
		{
			switch (dish)
			{
			case 0:
				reward.bonus.attackDamageFlat += amount;
				break;
			case 1:
				reward.bonus.abilityPowerFlat += amount;
				break;
			case 2:
				reward.bonus.armorFlat += amount;
				break;
			case 3:
				reward.bonus.critChanceFlat += (float)amount * 0.01f;
				break;
			case 4:
				reward.bonus.maxHealthFlat += amount;
				break;
			}
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_TargetOpenCook__NetworkConnection__Int32__Int32__Int32(NetworkConnection conn, int d0, int d1, int n)
	{
		Hero hero = (((UnityEngine.Object)(object)DewPlayer.local != null) ? DewPlayer.local.hero : null);
		if (hero.IsNullInactiveDeadOrKnockedOut())
		{
			return;
		}
		ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
		{
			owner = (UnityEngine.Object)(object)this,
			validator = () => isAvailable,
			rawContent = string.Format(DewLocalization.GetUIValue("Shrine_CookingPot_Prompt"), n),
			buttons = (DewMessageSettings.ButtonType.Cancel | DewMessageSettings.ButtonType.Custom0 | DewMessageSettings.ButtonType.Custom1),
			customButtonTexts = new string[2]
			{
				GetLabel(d0, n),
				GetLabel(d1, n)
			},
			onClose = (DewMessageSettings.ButtonType btn) =>
			{
				FxStop(useEffectLocal);
				int num = -1;
				switch (btn)
				{
				case DewMessageSettings.ButtonType.Custom0:
					num = d0;
					break;
				case DewMessageSettings.ButtonType.Custom1:
					num = d1;
					break;
				}
				if (num >= 0)
				{
					CmdChooseDish(hero, num);
				}
			},
			verticalButtons = true
		});
	}

	protected static void InvokeUserCode_TargetOpenCook__NetworkConnection__Int32__Int32__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetOpenCook called on server.");
		}
		else
		{
			((Shrine_CookingPot)(object)obj).UserCode_TargetOpenCook__NetworkConnection__Int32__Int32__Int32(NetworkClient.connection, NetworkReaderExtensions.ReadInt(reader), NetworkReaderExtensions.ReadInt(reader), NetworkReaderExtensions.ReadInt(reader));
		}
	}

	protected void UserCode_CmdChooseDish__Hero__Int32__NetworkConnectionToClient(Hero hero, int dish, NetworkConnectionToClient sender)
	{
		if (!((UnityEngine.Object)(object)hero == null) && !((UnityEngine.Object)(object)hero.owner == null) && persistentData.TryGetData<int>("Shrine_CookingPot", "offer", hero.owner.guid, out var value) && (dish == value / 100 || dish == value % 100))
		{
			int ingredientCount = GetIngredientCount(hero);
			if (ingredientCount > 0)
			{
				Apply(dish, hero, ingredientCount);
				ConsumeIngredients(hero);
			}
		}
	}

	protected static void InvokeUserCode_CmdChooseDish__Hero__Int32__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChooseDish called on client.");
		}
		else
		{
			((Shrine_CookingPot)(object)obj).UserCode_CmdChooseDish__Hero__Int32__NetworkConnectionToClient(NetworkReaderExtensions.ReadNetworkBehaviour<Hero>(reader), NetworkReaderExtensions.ReadInt(reader), senderConnection);
		}
	}

	static Shrine_CookingPot()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterCommand(typeof(Shrine_CookingPot), "System.Void Shrine_CookingPot::CmdChooseDish(Hero,System.Int32,Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdChooseDish__Hero__Int32__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_CookingPot), "System.Void Shrine_CookingPot::TargetOpenCook(Mirror.NetworkConnection,System.Int32,System.Int32,System.Int32)", (RemoteCallDelegate)InvokeUserCode_TargetOpenCook__NetworkConnection__Int32__Int32__Int32);
	}
}

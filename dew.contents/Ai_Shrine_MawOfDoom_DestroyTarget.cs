using System;
using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Shrine_MawOfDoom_DestroyTarget : AbilityInstance
{
	public float destroyDelay = 1f;

	public GameObject fxDestroyed;

	[NonSerialized]
	[SyncVar]
	public Actor target;

	[NonSerialized]
	public SafeAction customAction;

	private bool _isSkillTrigger;

	private Ge_CallOfTheRavenous _ge;

	protected NetworkBehaviourSyncVar ___targetNetId;

	public Actor Networktarget
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<Actor>(___targetNetId, ref target);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<Actor>(value, ref target, 64uL, (Action<Actor, Actor>)null, ref ___targetNetId);
		}
	}

	protected override IEnumerator OnCreateSequenced()
	{
		string[] args = null;
		int level = 0;
		string cachedGuid = null;
		if (Networktarget is Gem gem)
		{
			args = new string[2]
			{
				ChatManager.GetColoredDescribedPlayerName(gem.owner.owner),
				ChatManager.GetColoredGemName(((object)gem).GetType().Name, gem.quality)
			};
			level = gem.quality;
		}
		else if (Networktarget is SkillTrigger skillTrigger)
		{
			args = new string[2]
			{
				ChatManager.GetColoredDescribedPlayerName(skillTrigger.owner.owner),
				ChatManager.GetColoredSkillName(((object)skillTrigger).GetType().Name, skillTrigger.level)
			};
			level = skillTrigger.level;
			cachedGuid = skillTrigger.owner.owner.guid;
			_isSkillTrigger = true;
		}
		NetworkedManagerBase<ChatManager>.instance.ShowMessageLocally(new ChatManager.Message
		{
			type = ChatManager.MessageType.Notice,
			content = "Shrine_MawOfDoom_DestroyedMessage",
			args = args,
			itemType = ((object)Networktarget).GetType().Name,
			itemLevel = level
		});
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDestroy(Networktarget);
		yield return new SI.WaitForSeconds(destroyDelay);
		FxPlayNetworked(fxDestroyed);
		if (customAction != null && customAction.Count > 0)
		{
			try
			{
				customAction?.Invoke();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		else
		{
			if (_isSkillTrigger)
			{
				_ge = Dew.FindActorOfType<Ge_CallOfTheRavenous>();
				if ((UnityEngine.Object)(object)_ge == null)
				{
					_ge = Dew.CreateActor<Ge_CallOfTheRavenous>();
				}
				if (Networktarget is SkillTrigger skillTrigger2 && !_ge.didSpawnQuest)
				{
					SkillTrigger byShortTypeName = DewResources.GetByShortTypeName<SkillTrigger>(((object)skillTrigger2).GetType().Name, ResourceLoadSettings.Light);
					if (skillTrigger2.rarity == Rarity.Identity)
					{
						_ge.AddDestroyedSkill(cachedGuid, byShortTypeName, isIdentityDestroyed: true, level);
					}
					else
					{
						_ge.AddDestroyedSkill(cachedGuid, byShortTypeName, isIdentityDestroyed: false, level);
					}
					Ge_CallOfTheRavenous ge = _ge;
					ge.NetworkdestroyedSkillCount = ge.destroyedSkillCount + 1;
				}
			}
			Networktarget.Destroy();
		}
		DestroyIfActive();
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (Networktarget.IsNullOrInactive())
		{
			return;
		}
		Entity owner;
		if (Networktarget is Gem gem)
		{
			owner = gem.owner;
		}
		else
		{
			if (!(Networktarget is SkillTrigger skillTrigger))
			{
				return;
			}
			owner = skillTrigger.owner;
		}
		if ((UnityEngine.Object)(object)owner != null)
		{
			position = owner.Visual.GetCenterPosition();
		}
		else
		{
			position = Networktarget.position;
		}
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Networktarget);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Networktarget);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Actor>(ref target, (Action<Actor, Actor>)null, reader, ref ___targetNetId);
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Actor>(ref target, (Action<Actor, Actor>)null, reader, ref ___targetNetId);
		}
	}
}

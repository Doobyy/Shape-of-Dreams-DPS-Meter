using Mirror;
using UnityEngine;

public class Shrine_Anitya : Shrine, ICustomInteractable
{
	private Ge_Shrine_Anitya _ge;

	public string nameRawText => DewLocalization.GetUIValue(((object)this).GetType().Name + "_Name");

	public string interactActionRawText => DewLocalization.GetUIValue("InGame_Interact_Pray");

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_ge = Dew.FindActorOfType<Ge_Shrine_Anitya>();
			if ((Object)(object)_ge == null)
			{
				_ge = Dew.CreateActor<Ge_Shrine_Anitya>();
			}
			if (_ge.count >= _ge.countForAction)
			{
				Destroy();
			}
		}
	}

	protected override bool OnUse(Entity entity)
	{
		if (_ge.IsNullOrInactive())
		{
			return true;
		}
		if (_ge.count > _ge.countForAction)
		{
			return true;
		}
		Ge_Shrine_Anitya ge = _ge;
		ge.Networkcount = ge.count + 1;
		if (_ge.count >= _ge.countForAction)
		{
			NetworkedManagerBase<ChatManager>.instance.BroadcastMessage(new ChatManager.Message
			{
				type = ChatManager.MessageType.Notice,
				content = "Chat_Notice_InkHiddenChallengeActivated"
			});
		}
		return true;
	}

	private void MirrorProcessed()
	{
	}
}

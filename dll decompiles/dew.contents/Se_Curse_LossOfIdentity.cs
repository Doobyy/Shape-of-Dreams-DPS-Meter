using Mirror;
using UnityEngine;

public class Se_Curse_LossOfIdentity : CurseStatusEffect
{
	[SaveVar(SaveVarFlags.Default)]
	private DewPersistence.GeneralData _serializedIdentity;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			Unequip();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			Equip();
		}
	}

	[Server]
	private void Unequip()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Se_Curse_LossOfIdentity::Unequip()' called when server was not active");
		}
		else if (!victim.IsNullOrInactive() && victim is Hero hero && !((Object)(object)hero.Skill.Identity == null))
		{
			_serializedIdentity = new DewPersistence.GeneralData().Capture(hero.Skill.Identity);
			hero.Skill.Identity.Destroy();
		}
	}

	[Server]
	private void Equip()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Se_Curse_LossOfIdentity::Equip()' called when server was not active");
		}
		else if (!victim.IsNullOrInactive() && victim is Hero hero && _serializedIdentity != null)
		{
			SkillTrigger skillTrigger = DewPersistence.CreateActor<SkillTrigger>(_serializedIdentity);
			if (!((Object)(object)skillTrigger == null))
			{
				hero.Skill.EquipSkill(HeroSkillLocation.Identity, skillTrigger, ignoreCanReplace: true);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}

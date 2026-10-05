using Mirror;
using UnityEngine;

public class Gem_L_Culinary : Gem
{
	public ScalingValue dropChance;

	public float killGraceTime = 3f;

	private KillTracker _tracker;

	public float DropChance => 1f - 1f / (1f + Mathf.Max(0f, GetValue(dropChance)));

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			_tracker = newSkill.TrackKills(killGraceTime, OnKill);
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			_tracker?.Stop();
			_tracker = null;
		}
	}

	private void OnKill(EventInfoKill obj)
	{
		if (isValid && !((Object)(object)obj.victim == null) && owner.CheckEnemyOrNeutral(obj.victim) && !(Random.value > DropChance))
		{
			CreateAbilityInstance<Ai_Culinary_Pickup>(obj.victim.position, null, new CastInfo(owner));
			NotifyUse();
		}
	}

	private void MirrorProcessed()
	{
	}
}

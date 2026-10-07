using Mirror;
using UnityEngine;

public class Gem_R_Slippery : Gem
{
	public ScalingValue dmgPerMoveSpeedRatio;

	public GameObject fxStart;

	private float CurrentDamageAmp => Mathf.Max(0f, owner.Status.movementSpeedMultiplier - 1f) * GetValue(dmgPerMoveSpeedRatio);

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			FxPlayNetworked(fxStart, newOwner);
			if (newOwner.Status.TryGetStatusEffect<Se_Gem_R_Slippery>(out var effect))
			{
				effect.Destroy();
			}
			CreateStatusEffect<Se_Gem_R_Slippery>(newOwner, new CastInfo(newOwner));
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer && !((Object)(object)oldOwner == null) && oldOwner.Status.TryGetStatusEffect<Se_Gem_R_Slippery>(out var effect))
		{
			effect.Destroy();
		}
	}

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		newSkill.dealtDamageProcessor.Add(AmplifyDamage);
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			if ((Object)(object)oldSkill != null)
			{
				oldSkill.dealtDamageProcessor.Remove(AmplifyDamage);
			}
			FxStopNetworked(fxStart);
		}
	}

	private void AmplifyDamage(ref DamageData data, Actor actor, Entity target)
	{
		if (owner.CheckEnemyOrNeutral(target) && !data.IsAmountModifiedBy(this))
		{
			data.ApplyAmplification(CurrentDamageAmp);
			data.SetAmountModifiedBy(this);
			NotifyUse();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxStart);
		}
	}

	private void MirrorProcessed()
	{
	}
}

using Mirror;
using UnityEngine;

public class Gem_C_Guidance : Gem
{
	public ScalingValue healAmp;

	public float shieldRatio;

	public float shieldDuration = 3f;

	private ActorRef<Se_GenericShield_Stacking> _shield;

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			newSkill.dealtHealProcessor.Add(HealAmp);
			_shield = CreateStatusEffect(owner, new CastInfo(owner), (Se_GenericShield_Stacking s) =>
			{
				s.timeout = shieldDuration;
			});
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			if ((Object)(object)oldSkill != null)
			{
				oldSkill.dealtHealProcessor.Remove(HealAmp);
			}
			if (!_shield.IsNullOrInactive())
			{
				_shield.Get().Destroy();
				_shield = null;
			}
		}
	}

	private void HealAmp(ref HealData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this))
		{
			data.ApplyAmplification(GetValue(healAmp));
			data.SetAmountModifiedBy(this);
			NotifyUse();
		}
	}

	protected override void OnDoHeal(EventInfoHeal obj)
	{
		base.OnDoHeal(obj);
		if (!obj.chain.DidReact(this) && !(obj.discardedAmount <= 0f) && !_shield.IsNullOrInactive())
		{
			_shield.Get().AddAmount(obj.discardedAmount * shieldRatio, obj.chain.New(this));
			NotifyUse();
		}
	}

	private void MirrorProcessed()
	{
	}
}

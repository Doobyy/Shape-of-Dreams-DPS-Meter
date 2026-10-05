using System;
using Mirror;
using UnityEngine;

public class Se_Star_Mist_F_FL_DecreasedCooldownAndCharge : StarEffect
{
	public float cooldownReduction;

	public int reducedCharges;

	public override Type heroType => typeof(Hero_Mist);

	public override Type skillType => typeof(St_Q_Fleche);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)skill == null))
		{
			DoSkillBonusAll(new SkillBonus
			{
				addedCharge = -reducedCharges,
				cooldownOffset = 0f - cooldownReduction
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}

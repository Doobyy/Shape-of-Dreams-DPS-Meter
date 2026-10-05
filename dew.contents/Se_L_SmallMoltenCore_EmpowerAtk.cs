using System;
using Mirror;

public class Se_L_SmallMoltenCore_EmpowerAtk : StatusEffect
{
	[NonSerialized]
	public Sum_L_SmallMoltenCore_Dragon dragon;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && !dragon.IsNullOrInactive())
		{
			DestroyOnDeath(info.caster);
			DestroyOnDeath(dragon);
			ShowOnScreenTimer();
			SetTimer(dragon.empowerDuration);
		}
	}

	private void MirrorProcessed()
	{
	}
}

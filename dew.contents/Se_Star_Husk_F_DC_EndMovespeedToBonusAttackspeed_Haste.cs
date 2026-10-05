using System;
using Mirror;

public class Se_Star_Husk_F_DC_EndMovespeedToBonusAttackspeed_Haste : StatusEffect
{
	public float hasteDuration = 3f;

	[NonSerialized]
	public float hasteAmount;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoHaste(hasteAmount);
			SetTimer(hasteDuration);
			ShowOnScreenTimer("St_R_Deception");
		}
	}

	private void MirrorProcessed()
	{
	}
}

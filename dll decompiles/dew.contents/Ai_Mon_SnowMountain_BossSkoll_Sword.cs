using UnityEngine;

public class Ai_Mon_SnowMountain_BossSkoll_Sword : InstantDamageInstance
{
	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (info.point != default(Vector3))
		{
			((Component)(object)this).transform.position = info.point;
		}
	}

	private void MirrorProcessed()
	{
	}
}

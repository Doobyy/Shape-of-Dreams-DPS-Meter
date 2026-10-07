using UnityEngine;

public class Ai_Mon_SnowMountain_VikingWarrior_Sword : InstantDamageInstance
{
	public GameObject fxMain;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (info.point != default(Vector3))
		{
			((Component)(object)this).transform.position = info.point;
		}
		FxPlay(fxMain, info.point, null);
	}

	private void MirrorProcessed()
	{
	}
}

using UnityEngine;

public class Ai_Mon_SnowMountain_VikingWarrior_Shield : StandardProjectile
{
	public DewCollider range;

	public ScalingValue dmgFactor;

	public GameObject fxTelegraph;

	public GameObject fxHit;

	public GameObject fxFinishAtk;

	public override bool reuseInRoom
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	protected override void OnPrepare()
	{
	}

	protected override void OnCreate()
	{
	}

	protected override void OnComplete()
	{
	}

	private void MirrorProcessed()
	{
	}
}

using UnityEngine;

public class Ai_Nor_MyOwnLittleNebula_Missile : StandardProjectile
{
	public ScalingValue dmgFactor;

	public float deviateMag;

	private Vector3 _deviateVector;

	public override bool reuseInRoom
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	protected override void OnCreate()
	{
	}

	protected override Vector3 PositionSolver(float dt)
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	protected override void OnEntity(EntityHit hit)
	{
	}

	private void MirrorProcessed()
	{
	}
}

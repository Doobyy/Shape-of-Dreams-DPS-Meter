using UnityEngine;

public class PropEnt_Sky_Orca : PropEntity
{
	public Vector3 circularPoint;

	public float speed;

	public float radius;

	public override void OnStartServer()
	{
		base.OnStartServer();
		CreateBasicEffect(this, new UntargetableEffect(), float.PositiveInfinity);
		CreateBasicEffect(this, new InvisibleEffect
		{
			ignoreReveal = true
		}, float.PositiveInfinity);
		CreateBasicEffect(this, new InvulnerableEffect(), float.PositiveInfinity);
		CreateBasicEffect(this, new UncollidableEffect(), float.PositiveInfinity);
		Control.baseAgentSpeed = speed;
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		float f = speed * context.deltaTime;
		Vector3 positionOnGround = Dew.GetPositionOnGround(circularPoint);
		float x = positionOnGround.x + radius * Mathf.Cos(f);
		float z = positionOnGround.z + radius * Mathf.Sin(f);
		Vector3 destination = new Vector3(x, positionOnGround.y, z);
		Control.MoveToDestination(destination, immediately: false);
	}

	private void MirrorProcessed()
	{
	}
}

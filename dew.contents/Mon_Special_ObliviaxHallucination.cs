using Mirror;
using UnityEngine;

public class Mon_Special_ObliviaxHallucination : Monster
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity);
			CreateBasicEffect(this, new UntargetableEffect(), float.PositiveInfinity);
			CreateBasicEffect(this, new InvisibleEffect
			{
				ignoreReveal = true
			}, float.PositiveInfinity);
			CreateBasicEffect(this, new InvulnerableEffect(), float.PositiveInfinity);
			CreateBasicEffect(this, new UncollidableEffect(), float.PositiveInfinity);
		}
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		_ = (Object)(object)context.targetEnemy == null;
	}

	private void MirrorProcessed()
	{
	}
}

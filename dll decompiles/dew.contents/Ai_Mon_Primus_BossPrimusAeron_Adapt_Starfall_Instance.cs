using Mirror;
using UnityEngine;

[RequireComponent(typeof(GenericTransformSync))]
public class Ai_Mon_Primus_BossPrimusAeron_Adapt_Starfall_Instance : InstantDamageInstance
{
	public float speed = 3f;

	public override bool reuseInRoom => true;

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !info.target.IsNullInactiveDeadOrKnockedOut())
		{
			Vector3 current = position;
			current = Vector3.MoveTowards(current, info.target.GetAIAgentPosition(info.caster), dt * speed);
			position = current;
		}
	}

	private void MirrorProcessed()
	{
	}
}

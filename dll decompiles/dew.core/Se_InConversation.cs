using Mirror;
using UnityEngine;

public class Se_InConversation : StatusEffect
{
	private bool _previousAIState;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoUntargetable();
			DoUncollidable();
			DoInvisible(ignoreReveal: true);
			DoDeathInterrupt((EventInfoKill _) =>
			{
				victim.Status.SetHealth(1f);
			}, -10000);
			victim.Control.CancelOngoingChannels();
			victim.Control.Stop();
			_previousAIState = victim.AI.disableAI;
			victim.AI.disableAI = true;
			victim.Status.isInConversation = true;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)victim != null)
		{
			victim.AI.disableAI = _previousAIState;
			victim.Status.isInConversation = false;
		}
	}

	private void MirrorProcessed()
	{
	}
}

using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossMaw_Cataclysm_Meteor : InstantDamageInstance
{
	public Transform rotationTransform;

	public Transform fallTransform;

	public GameObject flyEffect;

	public GameObject impactEffect;

	public float fallSpeed;

	protected override void OnCreate()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			StartSequence(Sequence());
		}
		base.OnCreate();
		IEnumerator Sequence()
		{
			FxPlayNetworked(flyEffect);
			yield return new SI.WaitForSeconds(damageDelay);
			FxPlayNetworked(impactEffect);
			FxStopNetworked(flyEffect);
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		fallTransform.position += fallTransform.forward * (fallSpeed * Time.deltaTime);
	}

	private void MirrorProcessed()
	{
	}
}

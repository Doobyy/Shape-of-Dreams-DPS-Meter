using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_RoomMod_InkStrikeWarning_Artillery : AbilityInstance
{
	public float delay;

	public float range;

	public GameObject fxTelegraph;

	public GameObject fxInstance;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		FxPlayNetworked(fxTelegraph, position, Quaternion.identity);
		yield return new SI.WaitForSeconds(delay);
		FxStopNetworked(fxTelegraph);
		FxPlayNetworked(fxInstance, position, Quaternion.identity);
		yield return new SI.WaitForSeconds(0.1f);
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, position, range);
		for (int i = 0; i < list.Count; i++)
		{
			Entity entity = list[i];
			if (!entity.Status.hasCrowdControlImmunity)
			{
				entity.Visual.KnockUp(KnockUpStrength.Normal, isFriendly: false);
				if (entity.Status.TryGetStatusEffect<Se_RoomMod_InkStrikeWarning_Slow>(out var effect))
				{
					effect.ResetTimer();
				}
				else
				{
					CreateStatusEffect<Se_RoomMod_InkStrikeWarning_Slow>(entity, new CastInfo(entity));
				}
			}
		}
		handle.Return();
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}

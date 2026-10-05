using Mirror;
using UnityEngine;

public class Se_RoomMod_WarpingField_Unstable : StatusEffect
{
	public Vector2 distanceRange;

	public float chancePerTick = 0.005f;

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !(Random.value > chancePerTick) && !victim.Control.isDisplacing && !victim.Status.hasCrowdControlImmunity && (!((Object)(object)victim.owner != null) || !victim.owner.isSamplingCastInfo) && !SingletonDewNetworkBehaviour<Room>.instance.didClearRoom && !NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
		{
			Vector3 end = victim.agentPosition + Random.onUnitSphere.Flattened().normalized * Random.Range(distanceRange.x, distanceRange.y);
			end = Dew.GetValidAgentDestination_Closest(victim.agentPosition, end);
			CreateStatusEffect<Se_RoomMod_WarpingField_Warp>(victim, new CastInfo(victim, end));
		}
	}

	private void MirrorProcessed()
	{
	}
}

using UnityEngine;

[DewResourceLink(ResourceLinkBy.None)]
public class Trap_ArrowShooter : Actor, IActivatableTrap, IBanRoomNodesNearby, IBanCampsNearby
{
	public void ActivateTrap()
	{
		CreateAbilityInstance<Ai_Trap_ArrowShooter_Arrow>(((Component)(object)this).transform.position, Quaternion.identity, new CastInfo(null, CastInfo.GetAngle(((Component)(object)this).transform.rotation)));
	}

	private void MirrorProcessed()
	{
	}
}

using UnityEngine;

[DewResourceLink(ResourceLinkBy.None)]
public class Trap_Lightning : Actor, IActivatableTrap, IBanRoomNodesNearby, IBanCampsNearby
{
	public void ActivateTrap()
	{
		CreateAbilityInstance<Ai_Trap_Lightning>(((Component)(object)this).transform.position, null, default);
	}

	private void MirrorProcessed()
	{
	}
}

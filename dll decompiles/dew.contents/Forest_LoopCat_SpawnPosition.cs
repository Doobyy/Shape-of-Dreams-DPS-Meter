using UnityEngine;

public class Forest_LoopCat_SpawnPosition : MonoBehaviour, IPlayerPathablePoint, IBanRoomNodesNearby
{
	Vector3 IPlayerPathablePoint.pathablePosition => transform.position;
}

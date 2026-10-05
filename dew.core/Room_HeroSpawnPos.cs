using UnityEngine;

public class Room_HeroSpawnPos : MonoBehaviour, IPlayerPathablePoint, IBanRoomNodesNearby
{
	Vector3 IPlayerPathablePoint.pathablePosition => transform.position;
}

using UnityEngine;

public class Room_PlayerPathablePoint : MonoBehaviour, IPlayerPathablePoint
{
	public Vector3 pathablePosition => transform.position;
}

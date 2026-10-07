using UnityEngine;

public class Room_BossArena : SingletonBehaviour<Room_BossArena>
{
	public float radius = 10f;

	public Vector3 center => transform.position;

	public Vector3 GetRandomPosition()
	{
		return Dew.GetPositionOnGround(transform.position + Random.insideUnitCircle.ToXZ() * radius);
	}

	public Vector3 GetRandomPathablePosition()
	{
		return Dew.GetValidAgentDestination_Closest(center, GetRandomPosition());
	}
}

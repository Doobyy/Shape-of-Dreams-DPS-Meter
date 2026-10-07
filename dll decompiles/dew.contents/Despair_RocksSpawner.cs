using UnityEngine;

public class Despair_RocksSpawner : MonoBehaviour
{
	public Vector3 boundSize;

	public GameObject[] templates;

	public int rocksCount;

	[Tooltip("Will use X component of scale configuration")]
	public bool uniformScale;

	public Vector3 scaleMin;

	public Vector3 scaleMax;

	public float collisionCheckRadius;
}

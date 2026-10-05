using UnityEngine;

public class DarkCave_RandomOpenBarrier : DewNetworkBehaviour
{
	public float openChance;

	private Room_Barrier _obstacle;

	protected override void Awake()
	{
		base.Awake();
		_obstacle = ((Component)(object)this).GetComponent<Room_Barrier>();
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		if (Random.value < openChance)
		{
			openSecretPlace();
		}
	}

	protected void openSecretPlace()
	{
		_obstacle.Open();
	}

	private void MirrorProcessed()
	{
	}
}

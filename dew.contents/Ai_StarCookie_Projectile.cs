using Mirror;
using UnityEngine;

public class Ai_StarCookie_Projectile : StandardProjectile
{
	public float startHeight;

	public Vector2 duration;

	public override bool reuseInRoom => true;

	protected override void OnPrepare()
	{
		base.OnPrepare();
		targetPosition = ((Component)(object)this).transform.position + Vector3.up * customEndHeight;
		float num = customEndHeight;
		SetCustomStartPosition(((Component)(object)this).transform.position + Vector3.up * startHeight);
		initialSpeed = (startHeight - num) / Random.Range(duration.x, duration.y);
		_acceleration = 0f;
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		if (((NetworkBehaviour)this).isServer)
		{
			Dew.CreateActor<Shrine_StarCookie>(targetPosition, null);
		}
	}

	private void MirrorProcessed()
	{
	}
}

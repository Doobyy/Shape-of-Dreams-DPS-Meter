using UnityEngine;

public class Ai_R_Frostbite_Freeze_Projectile : StandardProjectile
{
	public ScalingValue freezeDamage;

	public float stunDuration;

	public GameObject entityAttachEffect;

	public override bool reuseInRoom
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	protected override void OnEntity(EntityHit hit)
	{
	}

	private void MirrorProcessed()
	{
	}
}

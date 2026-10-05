using Mirror;
using UnityEngine;

public class Ai_Mon_Sky_StarSeed_SelfDestruct : AbilityInstance
{
	public bool killCaster;

	public int projCount;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			int num = Random.Range(0, 360);
			for (int i = 0; i < projCount; i++)
			{
				CreateAbilityInstance<Ai_Mon_Sky_StarSeed_SelfDestruct_Projectile>(info.caster.position, null, new CastInfo(info.caster, num));
				num += 360 / projCount;
			}
			if (killCaster)
			{
				info.caster.Kill();
			}
			else
			{
				info.caster.Destroy();
			}
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}

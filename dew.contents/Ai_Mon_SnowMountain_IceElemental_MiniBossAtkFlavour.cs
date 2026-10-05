using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_SnowMountain_IceElemental_MiniBossAtkFlavour : AbilityInstance
{
	public int subExplosionCount;

	public float startSubExplosionDistance;

	public float perExplosionDistance;

	public float perExplosionInterval;

	public float initDelay;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		Vector3 pullDir = -info.forward.Flattened().normalized;
		yield return new SI.WaitForSeconds(initDelay);
		for (int i = 0; i < subExplosionCount; i++)
		{
			Vector3 positionOnGround = Dew.GetPositionOnGround(((Component)(object)this).transform.position + info.forward * (startSubExplosionDistance + perExplosionDistance * (float)i));
			CreateAbilityInstance(positionOnGround, null, new CastInfo(info.caster), (Ai_Mon_SnowMountain_IceElemental_IceBreaker_SubExplosion s) =>
			{
				s.pullDirection = pullDir;
			});
			yield return new SI.WaitForSeconds(perExplosionInterval);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}

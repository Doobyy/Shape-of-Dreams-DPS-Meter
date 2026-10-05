using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Despair_WretchedArtillery_BarrageAtk : AbilityInstance
{
	public GameObject perWaveEffect;

	public int waves;

	public int shootCountPerWave;

	public float waveInterval;

	public float radius;

	public Vector2 landTime;

	public float minRangeFromSelf;

	public float minRangeOfCenterFromSelf;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		Vector3 center = info.point;
		for (int i = 0; i < waves; i++)
		{
			FxPlayNetworked(perWaveEffect, info.caster);
			Vector3 vector = info.caster.position - center;
			if (vector.sqrMagnitude < minRangeOfCenterFromSelf * minRangeOfCenterFromSelf)
			{
				vector = vector.normalized * minRangeOfCenterFromSelf;
				center = info.caster.position + vector;
			}
			for (int j = 0; j < shootCountPerWave; j++)
			{
				Vector3 pos = info.point + Random.onUnitSphere.Flattened().normalized * (Random.value * radius);
				Vector3 vector2 = (pos - info.caster.position).Flattened();
				if (vector2.sqrMagnitude < minRangeFromSelf * minRangeFromSelf)
				{
					vector2 = vector2.normalized * minRangeFromSelf;
					pos = info.caster.position + vector2;
				}
				pos = Dew.GetPositionOnGround(pos);
				bool first = i == 0 && j == 0;
				CreateAbilityInstance(info.caster.position, null, new CastInfo(info.caster, pos), (Ai_Mon_Despair_WretchedArtillery_BarrageAtk_Missile a) =>
				{
					a.initialSpeed = Vector3.Distance(info.caster.position, pos) / Random.Range(landTime.x, landTime.y);
					a.targetSpeed = a.initialSpeed;
					a.acceleration = 0f;
					a.isFirstMissile = first;
				});
			}
			yield return new SI.WaitForSeconds(waveInterval);
		}
		Destroy();
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if ((Object)(object)info.caster != null)
		{
			position = info.caster.position;
			rotation = info.caster.rotation;
		}
	}

	private void MirrorProcessed()
	{
	}
}

using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Despair_BossAzurak_AltAtk : AbilityInstance
{
	public float knockUpStrength;

	public ScalingValue dmgFactor;

	public Knockback knockback;

	public DewCollider range;

	public GameObject fxMain;

	[Space(15f)]
	public int spawnCountPerDir;

	public int dirCount;

	public float distanceStep;

	public float angleStep;

	public float spawnInterval;

	public float startDelay;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		FxPlayNetworked(fxMain, info.point, null);
		range.transform.position = info.point;
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
		{
			CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(info.point).Dispatch(entity);
			entity.Visual.KnockUp(knockUpStrength, isFriendly: false);
			knockback.ApplyWithOrigin(info.point, entity);
		}
		handle.Return();
		yield return new SI.WaitForSeconds(startDelay);
		int num = UnityEngine.Random.Range(0, 360);
		float num2 = 360f / (float)dirCount;
		for (int i = 0; i < dirCount; i++)
		{
			float baseAngle = (float)num + (float)i * num2;
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
			IEnumerator Routine()
			{
				for (int dirIndex = 0; dirIndex < spawnCountPerDir; dirIndex++)
				{
					float num3 = (float)(dirIndex + 1) * distanceStep;
					float f = (baseAngle + (float)dirIndex * angleStep) * ((float)Math.PI / 180f);
					float x = Mathf.Cos(f) * num3;
					float z = Mathf.Sin(f) * num3;
					Vector3 point = info.point + new Vector3(x, 0f, z);
					CreateAbilityInstance<Ai_Mon_Despair_BossAzurak_AltAtk_Instance>(point, null, new CastInfo(info.caster, point));
					yield return new SI.WaitForSeconds(spawnInterval);
				}
			}
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}

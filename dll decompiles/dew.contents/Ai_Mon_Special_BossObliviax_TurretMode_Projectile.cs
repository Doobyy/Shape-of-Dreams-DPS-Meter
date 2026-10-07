using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ai_Mon_Special_BossObliviax_TurretMode_Projectile : StandardProjectile
{
	public float startHeight;

	public float startFrontDis;

	public float startPosDeviation;

	public float secondAtkDelay;

	public DewCollider range;

	public ScalingValue damage;

	public Knockback Knockback;

	public GameObject fxExplode;

	public GameObject fxHit;

	public GameObject fxTelegrpah;

	public GameObject fxGround;

	private Vector3 _startPos;

	protected override void OnPrepare()
	{
		base.OnPrepare();
		Vector2 insideUnitCircle = Random.insideUnitCircle;
		Vector3 vector = new Vector3(0f, insideUnitCircle.x, insideUnitCircle.y);
		_startPos = info.caster.position + Vector3.up * startHeight + info.forward * startFrontDis + vector * startPosDeviation;
		SetCustomStartPosition(_startPos);
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			Vector3 normalized = (info.point - _startPos).normalized;
			FxPlayNetworked(fxTelegrpah, info.point, null);
			FxPlayNetworked(fxGround, info.point, Quaternion.LookRotation(normalized));
			yield return new WaitForSeconds(secondAtkDelay);
			FxPlayNetworked(fxExplode, info.point, null);
			List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity = entities[i];
				Damage(damage).SetOriginPosition(position).Dispatch(entity);
				FxPlayNewNetworked(fxHit, entity);
			}
			handle.Return();
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}

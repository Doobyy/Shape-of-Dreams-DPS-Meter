using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_LavaLand_FireElemental_ExplosionSub : AbilityInstance
{
	public DewCollider range;

	public float intervalDelay = 0.5f;

	public GameObject hitEffect;

	public ScalingValue dmgFactor;

	public float existTime;

	public float slowStrength;

	public float slowDuration = 3f;

	private float _lastDamagedTime;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		Vector3 pos = ((Component)(object)this).transform.position;
		for (int i = -2; i <= 2; i++)
		{
			for (int j = -2; j <= 2; j++)
			{
				CheckOffsetGround(new Vector3((float)i * 1.5f, 0f, (float)j * 1.5f), ref pos);
			}
		}
		((Component)(object)this).transform.position = pos;
		RaycastHit val = default;
		if (((NetworkBehaviour)this).isServer && Physics.Raycast(new Ray(((Component)(object)this).transform.position, Vector3.down), ref val, 3f, LayerMask.GetMask("Ground")) && ((Component)(object)val.collider).gameObject.TryGetComponent<LavaLand_Lava>(out var component) && ((Behaviour)(object)component).isActiveAndEnabled)
		{
			FxStopNetworked(startEffect);
			Destroy();
		}
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_lastDamagedTime = Time.time;
		}
	}

	private void CheckOffsetGround(Vector3 offset, ref Vector3 pos)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		Vector3 positionOnGround = Dew.GetPositionOnGround(pos + offset);
		Debug.DrawLine(positionOnGround, positionOnGround + Vector3.up * 3f, Color.yellow, 1f);
		if (!(positionOnGround.y < pos.y) && positionOnGround.y - pos.y < 2f && (int)Dew.GetNavMeshPathStatus(pos, positionOnGround) == 0)
		{
			pos.y = positionOnGround.y + 0.1f;
		}
	}

	protected override IEnumerator OnCreateSequenced()
	{
		yield return base.OnCreateSequenced();
		if (((NetworkBehaviour)this).isServer)
		{
			yield return new SI.WaitForSeconds(existTime);
			Destroy();
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !(Time.time - _lastDamagedTime < intervalDelay))
		{
			_lastDamagedTime = Time.time;
			List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity e = entities[i];
				OnHit(e);
			}
			handle.Return();
		}
	}

	private void OnHit(Entity e)
	{
		FxPlayNewNetworked(hitEffect, e);
		CreateDamage(DamageData.SourceType.Default, dmgFactor).SetElemental(ElementalType.Fire).SetAttr(DamageAttribute.DamageOverTime).Dispatch(e);
		CreateBasicEffect(e, new SlowEffect
		{
			strength = slowStrength
		}, slowDuration);
	}

	private void MirrorProcessed()
	{
	}
}

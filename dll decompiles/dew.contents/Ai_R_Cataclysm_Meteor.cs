using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_R_Cataclysm_Meteor : InstantDamageInstance
{
	public Transform rotationTransform;

	public Transform fallTransform;

	public GameObject flyEffect;

	public GameObject impactEffect;

	public float fallSpeed;

	public float stunDuration;

	[NonSerialized]
	public bool spawnBurnInstance;

	private Vector3 _baseFallLocalPosition;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseFallLocalPosition = fallTransform.localPosition;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		spawnBurnInstance = false;
	}

	protected override void OnCreate()
	{
		fallTransform.localPosition = _baseFallLocalPosition;
		((Component)(object)this).transform.position = Dew.GetPositionOnGround(((Component)(object)this).transform.position);
		rotationTransform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360), 0f);
		if (((NetworkBehaviour)this).isServer)
		{
			StartSequence(Sequence());
		}
		base.OnCreate();
		IEnumerator Sequence()
		{
			FxPlayNetworked(flyEffect);
			yield return new SI.WaitForSeconds(damageDelay);
			FxPlayNetworked(impactEffect);
			FxStopNetworked(flyEffect);
			if (spawnBurnInstance)
			{
				CreateAbilityInstance<Ai_R_Cataclysm_Meteor_Burn>(((Component)(object)this).transform.position, null, new CastInfo(info.caster));
			}
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		fallTransform.position += fallTransform.forward * (fallSpeed * Time.deltaTime);
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		CreateBasicEffect(entity, new StunEffect(), stunDuration, "cataclysm_stun");
	}

	private void MirrorProcessed()
	{
	}
}

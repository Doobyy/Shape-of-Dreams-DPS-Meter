using UnityEngine;

public class Ai_MirageSkin_Oblivion_Orb : StandardProjectile
{
	public float finalScale;

	public Vector2 maxHpDamageRatio;

	public float hitGracePeriod;

	private float _ogRadius;

	private float _ogScale;

	private bool _baseCaptured;

	private float _baseRadius;

	private Vector3 _baseEffectScale;

	private bool _baseCanCollideMidFlight;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		if (!_baseCaptured)
		{
			_baseCaptured = true;
			_baseRadius = collisionRadius;
			_baseEffectScale = effectOnFly.transform.localScale;
			_baseCanCollideMidFlight = canCollideMidFlight;
		}
	}

	protected override void OnCreate()
	{
		collisionRadius = _baseRadius;
		effectOnFly.transform.localScale = _baseEffectScale;
		canCollideMidFlight = _baseCanCollideMidFlight;
		base.OnCreate();
		_ogRadius = _baseRadius;
		_ogScale = _baseEffectScale.x;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		collisionRadius = _baseRadius;
		effectOnFly.transform.localScale = _baseEffectScale;
		canCollideMidFlight = _baseCanCollideMidFlight;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		float num = Mathf.Lerp(1f, finalScale, EasingFunction.EaseOutQuad(0f, 1f, normalizedPosition));
		canCollideMidFlight = Time.time - creationTime > hitGracePeriod;
		collisionRadius = _ogRadius * num;
		effectOnFly.transform.localScale = Vector3.one * (_ogScale * num);
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		DefaultDamage(maxHpDamageRatio.Lerp(normalizedPosition) * hit.entity.maxHealth).SetDirection(rotation).SetOriginPosition(position).Dispatch(hit.entity);
	}

	private void MirrorProcessed()
	{
	}
}

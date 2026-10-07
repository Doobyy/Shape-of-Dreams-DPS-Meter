using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class Ai_E_DoomsdayMeteor : InstantDamageInstance, IOtherPlayersTonedDownDisable
{
	public int subMeteorCount = 16;

	public float stunDuration = 3f;

	public Transform meteorTransform;

	private Vector3 _baseMeteorLocalPos;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		if (meteorTransform != null)
		{
			_baseMeteorLocalPos = meteorTransform.localPosition;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (meteorTransform != null)
		{
			ShortcutExtensions.DOKill((Component)meteorTransform, false);
			meteorTransform.localPosition = _baseMeteorLocalPos;
		}
	}

	protected override void OnCreate()
	{
		position = info.point;
		base.OnCreate();
		TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove(meteorTransform, ((Component)(object)this).transform.position + meteorTransform.forward * 5f, 2.15f, false), (Ease)1);
	}

	protected override void OnCollisionCheck()
	{
		base.OnCollisionCheck();
		for (int i = 0; i < subMeteorCount; i++)
		{
			CreateAbilityInstance(position, null, new CastInfo(info.caster, 360f / (float)subMeteorCount * (float)i), (Ai_E_DoomsdayMeteor_SubProjectile ai) =>
			{
				ai.SetCustomStartPosition(position + Vector3.up * 1.5f);
			});
		}
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		CreateBasicEffect(entity, new StunEffect(), stunDuration);
	}

	private void MirrorProcessed()
	{
	}
}

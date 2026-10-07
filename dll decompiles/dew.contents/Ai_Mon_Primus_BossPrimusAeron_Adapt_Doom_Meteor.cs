using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class Ai_Mon_Primus_BossPrimusAeron_Adapt_Doom_Meteor : InstantDamageInstance
{
	public int subMeteorCount = 16;

	public float stunDuration = 3f;

	public Transform meteorTransform;

	private Vector3 _meteorPristineLocalPosition;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_meteorPristineLocalPosition = meteorTransform.localPosition;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove(meteorTransform, ((Component)(object)this).transform.position + meteorTransform.forward * 5f, 2.15f, false), (Ease)1);
	}

	protected override void OnCollisionCheck()
	{
		base.OnCollisionCheck();
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			int adjustedCount = Mathf.RoundToInt((float)subMeteorCount * Mathf.Lerp(0.7f, 1f, NetworkedManagerBase<GameManager>.instance.difficulty.specialSkillChanceMultiplier));
			for (int w = 0; w < 4; w++)
			{
				for (int i = 0; i < adjustedCount; i++)
				{
					CreateAbilityInstance(position, null, new CastInfo(info.caster, 360f / (float)adjustedCount * ((float)i + (float)w * 0.5f)), (Ai_Mon_Primus_BossPrimusAeron_Adapt_Doom_Meteor_SubFireball ai) =>
					{
						ai.SetCustomStartPosition(position + Vector3.up * 1.5f);
					});
				}
				yield return new WaitForSeconds(0.175f);
			}
		}
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		CreateBasicEffect(entity, new StunEffect(), stunDuration);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		ShortcutExtensions.DOKill((Component)meteorTransform, false);
		meteorTransform.localPosition = _meteorPristineLocalPosition;
	}

	private void MirrorProcessed()
	{
	}
}

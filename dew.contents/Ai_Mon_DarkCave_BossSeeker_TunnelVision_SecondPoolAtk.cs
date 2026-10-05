using System;
using Mirror;
using UnityEngine;

public class Ai_Mon_DarkCave_BossSeeker_TunnelVision_SecondPoolAtk : InstantDamageInstance
{
	public float innerAtkDelay;

	public float outerAtkDelay;

	public GameObject fxInnerTelegraph;

	public GameObject fxOuterTelegraph;

	public GameObject fxInnerAtk;

	public GameObject fxInnerAtkOnCaster;

	public GameObject fxOuterAtk;

	public GameObject fxOuterAtkOnCaster;

	public DewCollider innerRange;

	public DewCollider outerRange;

	public DewAnimationClip animPrepare;

	public DewAnimationClip animEnd;

	[NonSerialized]
	public int atkIndex;

	private GameObject _fxTelegraph;

	private GameObject _fxMain;

	private bool _isInnerAtk = true;

	public override bool reuseInRoom => true;

	protected override void OnPrepare()
	{
		base.OnPrepare();
		_isInnerAtk = UnityEngine.Random.value < 0.5f;
		if (atkIndex == 0)
		{
			_isInnerAtk = true;
		}
		if (atkIndex == 1)
		{
			_isInnerAtk = false;
		}
		if (_isInnerAtk)
		{
			damageDelay = innerAtkDelay;
			range = innerRange;
			_fxTelegraph = fxInnerTelegraph;
			_fxMain = fxInnerAtk;
			knockupAmount = 1.5f;
			doKnockback = true;
			knockbackSettings.distance = 0.35f;
			knockbackSettings.duration = 0.75f;
		}
		else if (!_isInnerAtk)
		{
			damageDelay = outerAtkDelay;
			range = outerRange;
			_fxTelegraph = fxOuterTelegraph;
			_fxMain = fxOuterAtk;
			knockupAmount = 0f;
			doKnockback = true;
			knockbackSettings.distance = 6.5f;
			knockbackSettings.duration = 1f;
		}
	}

	protected override void OnCreate()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Animation.PlayAbilityAnimation(animPrepare);
			FxPlayNetworked(_fxTelegraph, info.caster, position, rotation);
			if (!_isInnerAtk)
			{
				FxPlayNetworked(fxOuterAtkOnCaster, info.caster);
			}
			else
			{
				FxPlayNetworked(fxInnerAtkOnCaster, info.caster);
			}
			range.transform.position = position;
			range.transform.rotation = rotation;
		}
		base.OnCreate();
	}

	protected override void OnAfterDelay()
	{
		base.OnAfterDelay();
		if (!info.caster.IsNullOrInactive())
		{
			info.caster.Animation.PlayAbilityAnimation(animEnd);
			FxPlayNetworked(_fxMain, position, rotation);
		}
	}

	private void MirrorProcessed()
	{
	}
}

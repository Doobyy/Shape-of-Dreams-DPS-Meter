using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossWhiteNight_RageInstance : AbilityInstance
{
	public GameObject fxTelegraph;

	public GameObject fxInstance;

	public GameObject fxHit;

	public DewCollider range;

	public ScalingValue dmgFactor;

	public float startDelay;

	private GameObject _pristineFxTelegraph;

	private float _pristineStartDelay;

	private ScalingValue _pristineDmgFactor;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_pristineFxTelegraph = fxTelegraph;
		_pristineStartDelay = startDelay;
		_pristineDmgFactor = dmgFactor;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			FxPlayNetworked(fxTelegraph);
			yield return new SI.WaitForSeconds(startDelay);
			FxPlayNetworked(fxInstance);
			List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity = entities[i];
				CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(((Component)(object)this).transform.position).SetElemental(ElementalType.Light).Dispatch(entity);
				FxPlayNetworked(fxHit, entity);
			}
			handle.Return();
			Destroy();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		fxTelegraph = _pristineFxTelegraph;
		startDelay = _pristineStartDelay;
		dmgFactor = _pristineDmgFactor;
	}

	private void MirrorProcessed()
	{
	}
}

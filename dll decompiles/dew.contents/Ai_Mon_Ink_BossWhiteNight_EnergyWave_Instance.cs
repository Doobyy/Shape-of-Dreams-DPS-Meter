using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossWhiteNight_EnergyWave_Instance : TickDamageInstance
{
	public float rageInstanceDelay;

	public DewCollider rageRange;

	public GameObject fxRageMain;

	public GameObject fxRageTelegraph;

	private GameObject _baseFxTelegraph;

	private GameObject _baseFxLoop;

	private float _baseDelay;

	private DewCollider _baseRange;

	private bool _baseCaptured;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			if (!_baseCaptured)
			{
				_baseFxTelegraph = fxTelegraph;
				_baseFxLoop = fxLoop;
				_baseDelay = delay;
				_baseRange = range;
				_baseCaptured = true;
			}
			if (((Mon_Ink_BossWhiteNight)info.caster)._isRage)
			{
				fxTelegraph = fxRageTelegraph;
				fxLoop = fxRageMain;
				delay = rageInstanceDelay;
				range = rageRange;
			}
			else
			{
				fxTelegraph = _baseFxTelegraph;
				fxLoop = _baseFxLoop;
				delay = _baseDelay;
				range = _baseRange;
			}
		}
		base.OnCreate();
	}

	private void MirrorProcessed()
	{
	}
}

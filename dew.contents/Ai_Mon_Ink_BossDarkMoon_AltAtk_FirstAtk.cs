using System;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossDarkMoon_AltAtk_FirstAtk : DashAttackInstance
{
	public GameObject fxSecondAtkMain;

	public DewCollider secondAtkRange;

	[NonSerialized]
	public bool isSecondAtk;

	private GameObject _baseStartEffectNoStop;

	protected override void Awake()
	{
		base.Awake();
		_baseStartEffectNoStop = startEffectNoStop;
	}

	protected override void OnCreate()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			startEffectNoStop = (isSecondAtk ? fxSecondAtkMain : _baseStartEffectNoStop);
		}
		base.OnCreate();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		isSecondAtk = false;
	}

	private void MirrorProcessed()
	{
	}
}

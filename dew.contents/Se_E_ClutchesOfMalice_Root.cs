using Mirror;
using UnityEngine;

public class Se_E_ClutchesOfMalice_Root : StatusEffect
{
	[HideInInspector]
	public float duration;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoRoot();
			SetTimer(duration);
		}
	}

	private void MirrorProcessed()
	{
	}
}

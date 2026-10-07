using System.Collections;
using Mirror;
using UnityEngine;

public class Gem_E_Aftershock : Gem
{
	public GameObject fxBeforeActivate;

	public float delay;

	private int _version;

	protected override void OnCastComplete(EventInfoCast info)
	{
		base.OnCastComplete(info);
		Se_E_Aftershock_Armor effect;
		while (owner.Status.TryGetStatusEffect<Se_E_Aftershock_Armor>(out effect))
		{
			effect.Destroy();
		}
		_version++;
		_ = _version;
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			info.instance.LockDestroy();
			FxStopNetworked(fxBeforeActivate);
			yield return new WaitForSeconds(0.1f);
			FxPlayNetworked(fxBeforeActivate, owner);
			yield return new WaitForSeconds(delay - 0.1f);
			info.instance.UnlockDestroy();
			if (isValid)
			{
				FxStopNetworked(fxBeforeActivate);
				CreateAbilityInstanceWithSource<Ai_E_Aftershock_Damage>(info.instance, owner.position, Quaternion.identity, new CastInfo(owner));
				NotifyUse();
			}
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		numberDisplay = null;
		if ((Object)(object)oldOwner != null)
		{
			Se_E_Aftershock_Armor effect;
			while (oldOwner.Status.TryGetStatusEffect<Se_E_Aftershock_Armor>(out effect))
			{
				effect.Destroy();
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}

using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Culinary_Pickup : PickupInstance
{
	public List<GameObject> ingredients;

	private GameObject _cachedEffect;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)info.caster.owner).isLocalPlayer)
		{
			_cachedEffect = ingredients[Random.Range(0, ingredients.Count)];
			pickupEffect.GetComponent<DewAudioSource>().clip = _cachedEffect.GetComponent<DewAudioSource>().clip;
			FxPlay(_cachedEffect);
		}
	}

	protected override bool CanBeUsedBy(Hero hero)
	{
		if (!base.CanBeUsedBy(hero))
		{
			return false;
		}
		return (Object)(object)hero == (Object)(object)info.caster;
	}

	protected override void OnPickup(Hero hero)
	{
		base.OnPickup(hero);
		if (((NetworkBehaviour)this).isServer)
		{
			if (hero.Status.TryGetStatusEffect<Se_Gem_L_Culinary_Stack>(out var effect))
			{
				effect.AddStack();
			}
			else
			{
				hero.CreateStatusEffect<Se_Gem_L_Culinary_Stack>(hero, new CastInfo(hero));
			}
			FxStop(_cachedEffect);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		FxStop(_cachedEffect);
	}

	private void MirrorProcessed()
	{
	}
}

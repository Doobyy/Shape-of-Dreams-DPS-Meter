using INab.VFXAssets;
using UnityEngine;

namespace INab.Demo;

public class CharacterEffectAPIShowcase : MonoBehaviour
{
	[Header("Effect Effect Reference")]
	public CharacterEffect characterEffect;

	[Header("Effect Prefabs")]
	public GameObject characterPrefab1;

	public GameObject characterPrefab2;

	public void StartEffect()
	{
		if (characterEffect != null)
		{
			characterEffect.StartEffect();
		}
	}

	public void EndEffect()
	{
		if (characterEffect != null)
		{
			characterEffect.StopEffect();
		}
	}

	public void SetNewEffectPrefab(GameObject newPrefab)
	{
		if (characterEffect != null && newPrefab != null)
		{
			characterEffect.SetNewEffectPrefab(newPrefab);
		}
	}

	public void SetEffectPrefab1()
	{
		SetNewEffectPrefab(characterPrefab1);
		StartEffect();
	}

	public void SetEffectPrefab2()
	{
		SetNewEffectPrefab(characterPrefab2);
		StartEffect();
	}
}

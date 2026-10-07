using System.Collections.Generic;
using INab.VFXAssets;
using UnityEngine;
using UnityEngine.VFX;

namespace INab.Demo;

[ExecuteInEditMode]
public class ShowcaseSpawnerCharacterEffect : MonoBehaviour
{
	public List<GameObject> effectPrefabs = new List<GameObject>();

	public GameObject meshesToSpawn;

	public float stepDistance = 2f;

	public Transform parentTransform;

	public Vector3 direction;

	public bool useCustomPositionOffset;

	public float positionOffset;

	[SerializeField]
	private List<GameObject> spawnedObjects = new List<GameObject>();

	public void OnEnable()
	{
		PlayAll();
	}

	public void SpawnPrefabs()
	{
		for (int i = 0; i < effectPrefabs.Count; i++)
		{
			GameObject gameObject = Object.Instantiate(meshesToSpawn, new Vector3((float)i * stepDistance, 0f, 0f), Quaternion.Euler(0f, 180f, 0f), parentTransform);
			gameObject.name = effectPrefabs[i].name ?? "";
			CharacterEffect[] componentsInChildren = gameObject.GetComponentsInChildren<CharacterEffect>();
			componentsInChildren[0].SetNewEffectPrefab(effectPrefabs[i]);
			VisualEffect componentInChildren = componentsInChildren[0].GetComponentInChildren<VisualEffect>();
			if (componentInChildren.HasFloat("Position Offset") && useCustomPositionOffset)
			{
				componentInChildren.SetFloat("Position Offset", positionOffset);
			}
			if (componentInChildren.HasVector3("Effect Direction_direction"))
			{
				componentInChildren.SetVector3("Effect Direction_direction", direction);
			}
			componentInChildren.initialEventName = "OnPlay";
			spawnedObjects.Add(gameObject);
			gameObject.SetActive(value: true);
		}
	}

	public void DestroyPrefabs()
	{
		foreach (GameObject spawnedObject in spawnedObjects)
		{
			if (spawnedObject != null)
			{
				Object.DestroyImmediate(spawnedObject);
			}
		}
		spawnedObjects.Clear();
	}

	public void PlayAll()
	{
		foreach (GameObject spawnedObject in spawnedObjects)
		{
			if (spawnedObject != null)
			{
				CharacterEffect[] componentsInChildren = spawnedObject.GetComponentsInChildren<CharacterEffect>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].StartEffect();
				}
			}
		}
	}

	public void StopAll()
	{
		foreach (GameObject spawnedObject in spawnedObjects)
		{
			if (spawnedObject != null)
			{
				CharacterEffect[] componentsInChildren = spawnedObject.GetComponentsInChildren<CharacterEffect>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].StopEffect();
				}
			}
		}
	}
}

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class DewSceneReflectionBinder
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void RuntimeInitialize()
	{
		SceneManager.activeSceneChanged -= OnActiveSceneChanged;
		SceneManager.activeSceneChanged += OnActiveSceneChanged;
		Apply(SceneManager.GetActiveScene());
	}

	private static void OnActiveSceneChanged(Scene prev, Scene next)
	{
		Apply(next);
	}

	public static void Apply(Scene scene)
	{
		if (!scene.IsValid() || !scene.isLoaded)
		{
			return;
		}
		ReflectionProbe reflectionProbe = FindSceneReflectionProbe(scene);
		Texture texture = null;
		if (reflectionProbe != null)
		{
			texture = ((reflectionProbe.texture != null) ? reflectionProbe.texture : reflectionProbe.bakedTexture);
		}
		if (texture != null)
		{
			if (RenderSettings.defaultReflectionMode != DefaultReflectionMode.Custom)
			{
				RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
			}
			if (RenderSettings.customReflectionTexture != texture)
			{
				RenderSettings.customReflectionTexture = texture;
			}
		}
		else
		{
			if (RenderSettings.defaultReflectionMode != DefaultReflectionMode.Skybox)
			{
				RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
			}
			if (RenderSettings.customReflectionTexture != null)
			{
				RenderSettings.customReflectionTexture = null;
			}
		}
	}

	private static ReflectionProbe FindSceneReflectionProbe(Scene scene)
	{
		GameObject[] rootGameObjects = scene.GetRootGameObjects();
		for (int i = 0; i < rootGameObjects.Length; i++)
		{
			ReflectionProbe componentInChildren = rootGameObjects[i].GetComponentInChildren<ReflectionProbe>();
			if (componentInChildren != null && componentInChildren.enabled)
			{
				return componentInChildren;
			}
		}
		return null;
	}
}

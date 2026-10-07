using UnityEngine;

[ExecuteAlways]
public class GameCover_FogSettings : MonoBehaviour
{
	public Material skybox;

	public Color fogColor;

	public float fogStartDistance;

	public float fogEndDistance;

	public Color ambientColor;

	private void Update()
	{
		RenderSettings.skybox = skybox;
		RenderSettings.fogColor = fogColor;
		RenderSettings.fogStartDistance = fogStartDistance;
		RenderSettings.fogEndDistance = fogEndDistance;
		RenderSettings.ambientLight = ambientColor;
	}
}

using UnityEngine;

public class DispByDestination : Displacement
{
	public DewEase ease;

	public Vector3 destination;

	public float duration;

	public Vector4 curve = new Vector4(0f, 0f, 1f, 1f);

	public float? curveHorizontalDistance;

	public bool canGoOverTerrain = true;
}

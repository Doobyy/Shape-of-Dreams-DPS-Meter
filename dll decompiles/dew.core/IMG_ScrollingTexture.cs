using UnityEngine;

public class IMG_ScrollingTexture : ImageComponentAnimated
{
	public string propName;

	public Vector2 scrollSpeed;

	public override void Tick()
	{
		base.Tick();
		material.SetTextureOffset(propName, new Vector2(Mathf.Repeat(time * scrollSpeed.x, 1f), Mathf.Repeat(time * scrollSpeed.y, 1f)));
	}
}

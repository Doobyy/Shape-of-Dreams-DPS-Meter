using UnityEngine;

[DewResourceLink(ResourceLinkBy.Name)]
public class Accessory : MonoBehaviour, ICosmetic, IExcludeFromPool
{
	public AccType type;

	public bool excludeFromPool;

	public string category;

	public bool generatedFromServer;

	public string[] dlcIds;

	[Space(30f)]
	public Sprite previewImage;

	[Range(-0.25f, 0.25f)]
	public float previewPadding;

	public Vector3 previewDirection = new Vector3(-0.57735f, -0.57735f, -0.57735f);

	public Transform previewCustomCenter;

	public Vector2 previewOffset;

	private Entity _target;

	bool ICosmetic.generatedFromServer => generatedFromServer;

	string[] ICosmetic.dlcIds => dlcIds;

	bool IExcludeFromPool.excludeFromPool => excludeFromPool;

	public void Setup(Transform target, string entityOrSkinName)
	{
		_target = target.GetComponent<Entity>();
		transform.SetParent(target, worldPositionStays: false);
		Accessory_AttachPoint[] componentsInChildren = GetComponentsInChildren<Accessory_AttachPoint>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].BeginRuntime(target);
		}
	}
}

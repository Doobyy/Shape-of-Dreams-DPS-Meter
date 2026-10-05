public class InteractableMeshHighlightProvider : MeshHighlightProvider
{
	protected override void Start()
	{
		base.Start();
		meshHighlight.outlineColor = ManagerBase<ObjectHighlightManager>.instance.interactable.color;
	}
}

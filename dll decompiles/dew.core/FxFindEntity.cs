using UnityEngine;

public class FxFindEntity : MonoBehaviour, IEffectSetupComponent
{
	public string findType;

	public bool exactMatch;

	public void OnEffectSetup()
	{
		if ((Object)(object)NetworkedManagerBase<ActorManager>.instance == null || string.IsNullOrEmpty(findType))
		{
			return;
		}
		string text = findType.Trim();
		Entity entity = null;
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			if ((exactMatch && ((object)allEntity).GetType().Name == text) || (!exactMatch && ((object)allEntity).GetType().Name.Contains(text)))
			{
				entity = allEntity;
			}
		}
		if ((Object)(object)entity == null)
		{
			Debug.Log("FxFindEntity target not found: " + text);
			return;
		}
		Debug.Log("FxFindEntity target: " + entity.GetActorReadableName());
		ListReturnHandle<IAttachableToEntity> handle;
		foreach (IAttachableToEntity item in ((Component)this).GetComponentsInChildrenNonAlloc(out handle))
		{
			item.OnAttachToEntity(entity);
		}
		handle.Return();
	}
}

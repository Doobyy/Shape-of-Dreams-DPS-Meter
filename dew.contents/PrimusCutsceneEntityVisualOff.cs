using System.Linq;
using UnityEngine;

public class PrimusCutsceneEntityVisualOff : MonoBehaviour
{
	private Entity[] _entities;

	private void OnEnable()
	{
		if ((Object)(object)NetworkedManagerBase<ActorManager>.instance == null)
		{
			return;
		}
		_entities = NetworkedManagerBase<ActorManager>.instance.allEntities.ToArray();
		Entity[] entities = _entities;
		foreach (Entity entity in entities)
		{
			if (!entity.IsNullInactiveDeadOrKnockedOut() && entity.GetRelation(DewPlayer.local.hero) != EntityRelation.Enemy)
			{
				entity.Visual.DisableRenderersLocal();
			}
		}
	}

	private void OnDisable()
	{
		Entity[] entities = _entities;
		foreach (Entity entity in entities)
		{
			if (!entity.IsNullInactiveDeadOrKnockedOut())
			{
				entity.Visual.EnableRenderersLocal();
			}
		}
	}
}

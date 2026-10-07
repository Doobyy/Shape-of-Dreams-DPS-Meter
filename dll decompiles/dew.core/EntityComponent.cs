using UnityEngine;

[LogicUpdatePriority(-300)]
public abstract class EntityComponent : DewNetworkBehaviour
{
	public Entity entity { get; internal set; }

	protected virtual void OnDisable()
	{
		DewPool.ClearEventsAndProcessors((Component)(object)this);
	}

	public virtual void ClearPooledEventsAndProcessors()
	{
	}

	private void MirrorProcessed()
	{
	}
}

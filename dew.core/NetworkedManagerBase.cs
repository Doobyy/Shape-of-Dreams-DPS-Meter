using UnityEngine;

[LogicUpdatePriority(-300)]
public class NetworkedManagerBase<T> : DewNetworkBehaviour where T : DewNetworkBehaviour
{
	public static T instance
	{
		get
		{
			if ((Object)(object)softInstance == null)
			{
				softInstance = Object.FindObjectOfType<T>();
			}
			if ((Object)(object)softInstance == null)
			{
				softInstance = Object.FindObjectOfType<T>(true);
			}
			return softInstance;
		}
	}

	public static T softInstance { get; private set; }

	protected override void Awake()
	{
		softInstance = this as T;
		base.Awake();
	}

	protected virtual void OnEnable()
	{
		if ((Object)(object)softInstance == null || !((Behaviour)(object)softInstance).enabled)
		{
			softInstance = this as T;
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		DewMod.NotifyManagerLifecycle(((object)this).GetType(), enable: true);
	}

	public override void OnStopClient()
	{
		base.OnStopClient();
		DewMod.NotifyManagerLifecycle(((object)this).GetType(), enable: false);
	}

	private void MirrorProcessed()
	{
	}
}

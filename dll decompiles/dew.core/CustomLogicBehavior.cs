using System;

public class CustomLogicBehavior : LogicBehaviour
{
	public Action onStart;

	public Action onDestroy;

	public Action onEnable;

	public Action onDisable;

	public Action onFrameUpdate;

	public Action onLogicUpdate;

	protected virtual void Start()
	{
		onStart?.Invoke();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		onEnable?.Invoke();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		onDisable?.Invoke();
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		onFrameUpdate?.Invoke();
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		onLogicUpdate?.Invoke();
	}

	protected virtual void OnDestroy()
	{
		onDestroy?.Invoke();
	}
}

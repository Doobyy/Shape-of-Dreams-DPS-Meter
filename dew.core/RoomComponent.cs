using UnityEngine;

[RequireComponent(typeof(Room))]
public class RoomComponent : DewNetworkBehaviour
{
	public Room room { get; private set; }

	public bool isRoomActive { get; internal set; }

	public bool isRevisit => room.isRevisit;

	protected override void Awake()
	{
		base.Awake();
		room = ((Component)(object)this).GetComponent<Room>();
	}

	public virtual void OnRoomStartServer()
	{
	}

	public virtual void OnRoomStart()
	{
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (isRoomActive)
		{
			ActiveLogicUpdate(dt);
		}
	}

	protected virtual void ActiveLogicUpdate(float dt)
	{
	}

	public virtual void OnRoomStop()
	{
	}

	public virtual void OnRoomStopServer()
	{
	}

	private void MirrorProcessed()
	{
	}
}

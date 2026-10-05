using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class Room_Barrier : DewNetworkBehaviour
{
	private struct Ad_BarrierPassThrough
	{
		public float lastUseTime;
	}

	[SyncVar(hook = "OnOpenChanged")]
	[SerializeField]
	private bool _isOpen;

	public GameObject openEffect;

	public GameObject closedEffect;

	public bool canPassThroughOneWay;

	public DewCollider passThroughCollider;

	private NavMeshObstacle _navObstacle;

	private Collider _collider;

	public Action<bool, bool> _Mirror_SyncVarHookDelegate__isOpen;

	public bool isOpen => _isOpen;

	public bool Network_isOpen
	{
		get
		{
			return _isOpen;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _isOpen, 1uL, _Mirror_SyncVarHookDelegate__isOpen);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_navObstacle = ((Component)(object)this).GetComponent<NavMeshObstacle>();
		_collider = ((Component)(object)this).GetComponent<Collider>();
		if (!canPassThroughOneWay)
		{
			return;
		}
		passThroughCollider.onEntityEnter.AddListener((Entity e) =>
		{
			if (((NetworkBehaviour)this).isServer && !isOpen && (!e.TryGetData<Ad_BarrierPassThrough>(out var data) || !(Time.time - data.lastUseTime < 2f)) && !e.Visual.isSpawning)
			{
				LetEntityPassThrough(e);
			}
		});
		passThroughCollider.receiveEntityCallbacks = true;
		passThroughCollider.UpdateProxyCollider();
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		SingletonDewNetworkBehaviour<Room>.instance.onRemoveObstacles += new Action(OnRemoveObstacles);
	}

	private void OnRemoveObstacles()
	{
		Open();
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		if ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance != null)
		{
			SingletonDewNetworkBehaviour<Room>.instance.onRemoveObstacles -= new Action(OnRemoveObstacles);
		}
	}

	public override void OnLateStartServer()
	{
		base.OnLateStartServer();
		if ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance != null && SingletonDewNetworkBehaviour<Room>.instance.isRevisit)
		{
			if (NetworkedManagerBase<ZoneManager>.instance.currentNode.type != WorldNodeType.ExitBoss)
			{
				Open();
			}
			else
			{
				Close();
			}
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		OnOpenChanged(_isOpen, _isOpen);
	}

	private void OnOpenChanged(bool oldVal, bool newVal)
	{
		if (newVal)
		{
			if (openEffect != null)
			{
				FxPlay(openEffect);
			}
			if (closedEffect != null)
			{
				FxStop(closedEffect);
			}
		}
		else
		{
			if (openEffect != null)
			{
				FxStop(openEffect);
			}
			if (closedEffect != null)
			{
				FxPlay(closedEffect);
			}
		}
		((Behaviour)(object)_navObstacle).enabled = !newVal;
		_collider.enabled = !newVal;
	}

	[Server]
	public void Open()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room_Barrier::Open()' called when server was not active");
		}
		else
		{
			Network_isOpen = true;
		}
	}

	[Server]
	public void Close()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room_Barrier::Close()' called when server was not active");
		}
		else
		{
			if (!_isOpen)
			{
				return;
			}
			Network_isOpen = false;
			if (!canPassThroughOneWay)
			{
				return;
			}
			ListReturnHandle<Entity> handle;
			foreach (Entity entity in passThroughCollider.GetEntities(out handle))
			{
				if (entity.Visual.isSpawning)
				{
					return;
				}
				LetEntityPassThrough(entity);
			}
			handle.Return();
		}
	}

	[Server]
	public void Toggle()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room_Barrier::Toggle()' called when server was not active");
		}
		else
		{
			Network_isOpen = !_isOpen;
		}
	}

	private void LetEntityPassThrough(Entity e)
	{
		e.RemoveData<Ad_BarrierPassThrough>();
		e.AddData(new Ad_BarrierPassThrough
		{
			lastUseTime = Time.time
		});
		e.CreateStatusEffect(e, default, (Se_BarrierPassThrough s) =>
		{
			Vector3 agentPosition = e.agentPosition;
			agentPosition = ((Component)(object)this).transform.InverseTransformPoint(agentPosition);
			agentPosition.z *= -1f;
			agentPosition = ((Component)(object)this).transform.TransformPoint(agentPosition);
			agentPosition = Dew.GetPositionOnGround(agentPosition);
			RoomSection componentInParent = ((Component)(object)this).GetComponentInParent<RoomSection>();
			if (componentInParent != null)
			{
				agentPosition = Dew.GetValidAgentDestination_Closest(componentInParent.pathablePivot, agentPosition);
			}
			else
			{
				s.destination = agentPosition;
			}
			s.destination = agentPosition;
		});
	}

	private void OnDrawGizmos()
	{
		if (canPassThroughOneWay)
		{
			Vector3 vector = ((Component)(object)this).transform.position + ((Component)(object)this).transform.up * 2f;
			DewGizmos.DrawArrow(vector, vector + ((Component)(object)this).transform.forward * 4f, Color.cyan, 1f);
		}
	}

	public Room_Barrier()
	{
		_Mirror_SyncVarHookDelegate__isOpen = OnOpenChanged;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		((NetworkBehaviour)this).SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, _isOpen);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 1L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _isOpen);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		((NetworkBehaviour)this).DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isOpen, _Mirror_SyncVarHookDelegate__isOpen, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 1L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isOpen, _Mirror_SyncVarHookDelegate__isOpen, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}

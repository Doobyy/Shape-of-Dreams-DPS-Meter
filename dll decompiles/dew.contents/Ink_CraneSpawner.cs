using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ink_CraneSpawner : DewNetworkBehaviour
{
	public GameObject crane;

	public Transform[] startPos;

	public float moveDis;

	public float interval;

	public float moveTime;

	private Vector3 _startPos;

	private float _elapsedTime;

	private Vector3 _endPos;

	private GameObject _item;

	public override void OnStartServer()
	{
		base.OnStartServer();
		if (((NetworkBehaviour)this).isServer)
		{
			int num = Random.Range(0, startPos.Length);
			_item = startPos[num].gameObject;
			_startPos = _item.transform.position;
			_endPos = _startPos + _item.transform.forward * moveDis;
		}
	}

	[ClientRpc]
	public void SpawnCrane()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ink_CraneSpawner::SpawnCrane()", 1947875435, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void OnDrawGizmosSelected()
	{
		Transform[] array = startPos;
		foreach (Transform transform in array)
		{
			if (transform == null)
			{
				break;
			}
			Gizmos.color = Color.cyan;
			Vector3 position = transform.position;
			Vector3 to = position + transform.forward * 30f;
			Gizmos.DrawLine(position, to);
			Gizmos.DrawSphere(position, 1f);
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_SpawnCrane()
	{
		Animator[] componentsInChildren = Object.Instantiate(crane, _item.transform).GetComponentsInChildren<Animator>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].speed = Random.Range(0.8f, 1.2f);
		}
		float num = Vector3.Distance(_startPos, _endPos) / moveTime;
		float movedisPerInterval = num * interval;
		Vector3 dir = (_endPos - _startPos).normalized;
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return new SI.WaitForSeconds(Random.Range(0.5f, 2f));
			_elapsedTime = 0f;
			while (_elapsedTime < moveTime)
			{
				_item.transform.position += dir * movedisPerInterval;
				_elapsedTime += interval;
				yield return new SI.WaitForSeconds(interval);
			}
			yield return new SI.WaitForSeconds(moveTime);
			Object.Destroy(_item);
		}
	}

	protected static void InvokeUserCode_SpawnCrane(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC SpawnCrane called on server.");
		}
		else
		{
			((Ink_CraneSpawner)(object)obj).UserCode_SpawnCrane();
		}
	}

	static Ink_CraneSpawner()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ink_CraneSpawner), "System.Void Ink_CraneSpawner::SpawnCrane()", (RemoteCallDelegate)InvokeUserCode_SpawnCrane);
	}
}

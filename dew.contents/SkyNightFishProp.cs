using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class SkyNightFishProp : DewNetworkBehaviour
{
	public GameObject dest;

	public float interval;

	public float moveTime;

	private Vector3 _startPos;

	private float _elapsedTime;

	private Vector3 _endPos;

	public override void OnStartServer()
	{
		base.OnStartServer();
		if (((NetworkBehaviour)this).isServer)
		{
			_startPos = ((Component)(object)this).transform.position;
			_endPos = dest.transform.position;
			((Component)(object)this).transform.LookAt(_endPos, Vector3.up);
		}
	}

	[ClientRpc]
	public void Open()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void SkyNightFishProp::Open()", 474949345, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void OnDrawGizmosSelected()
	{
		if (dest != null)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(((Component)(object)this).transform.position, dest.transform.position);
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_Open()
	{
		SkyNightFishes[] componentsInChildren = ((Component)(object)this).GetComponentsInChildren<SkyNightFishes>(true);
		foreach (SkyNightFishes skyNightFishes in componentsInChildren)
		{
			if (!skyNightFishes.gameObject.activeSelf)
			{
				skyNightFishes.gameObject.SetActive(value: true);
			}
		}
		float num = Vector3.Distance(_startPos, _endPos) / moveTime;
		float movedisPerInterval = num * interval;
		Vector3 dir = (_endPos - _startPos).normalized;
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			_elapsedTime = 0f;
			while (_elapsedTime < moveTime)
			{
				if (Time.timeScale <= 0f)
				{
					yield return null;
				}
				((Component)(object)this).transform.position += dir * movedisPerInterval;
				_elapsedTime += interval;
				yield return new SI.WaitForSeconds(interval);
			}
			yield return new SI.WaitForSeconds(moveTime);
			Object.Destroy(((Component)(object)this).gameObject);
		}
	}

	protected static void InvokeUserCode_Open(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC Open called on server.");
		}
		else
		{
			((SkyNightFishProp)(object)obj).UserCode_Open();
		}
	}

	static SkyNightFishProp()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(SkyNightFishProp), "System.Void SkyNightFishProp::Open()", (RemoteCallDelegate)InvokeUserCode_Open);
	}
}

using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Shrine_LoopCat : Shrine, ICustomInteractable
{
	public GameObject effect;

	public GameObject telephoneSound;

	private Animator _animator;

	public string nameRawText => DewLocalization.GetUIValue(((object)this).GetType().Name + "_Name");

	public string interactActionRawText => DewLocalization.GetUIValue("InGame_Tooltip_Pet");

	public Vector3 worldOffset => new Vector3(0f, 2.3f, 0f);

	protected override void OnCreate()
	{
		base.OnCreate();
		_animator = ((Component)(object)this).GetComponentInChildren<Animator>();
	}

	protected override bool OnUse(Entity entity)
	{
		if (Random.value <= 0.1f)
		{
			FxPlayNewNetworked(telephoneSound);
		}
		OnUseRoutine();
		return true;
	}

	[ClientRpc]
	private void OnUseRoutine()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Shrine_LoopCat::OnUseRoutine()", 68360535, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_OnUseRoutine()
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			_animator.Play("Up");
			yield return new WaitForSeconds(2f);
			FxPlayNew(effect, ((Component)(object)this).transform.position, null);
			if (((NetworkBehaviour)this).isServer)
			{
				Destroy();
			}
		}
	}

	protected static void InvokeUserCode_OnUseRoutine(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC OnUseRoutine called on server.");
		}
		else
		{
			((Shrine_LoopCat)(object)obj).UserCode_OnUseRoutine();
		}
	}

	static Shrine_LoopCat()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_LoopCat), "System.Void Shrine_LoopCat::OnUseRoutine()", (RemoteCallDelegate)InvokeUserCode_OnUseRoutine);
	}
}

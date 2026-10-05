using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class BossLightElementalKillBehavior : DewNetworkBehaviour
{
	public GameObject sunLight;

	public GameObject sfDecal;

	public GameObject lightDecal;

	public MeshRenderer[] jailRenderers;

	public float fadeDuration;

	[ClientRpc]
	public void PlayBossKillBehaviorNetworked()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void BossLightElementalKillBehavior::PlayBossKillBehaviorNetworked()", -766955189, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private IEnumerator FadeEmissionRoutine(Material mat)
	{
		Color startEmissionColor = mat.GetColor("_EmissionColor");
		Color endColor = new Color(0f, 0f, 0f);
		float time = 0f;
		while (time < fadeDuration)
		{
			Color value = Color.Lerp(startEmissionColor, endColor, time / fadeDuration);
			mat.SetColor("_EmissionColor", value);
			time += Time.deltaTime;
			yield return null;
		}
		mat.SetColor("_EmissionColor", endColor);
	}

	private IEnumerator FadeAlphaRoutine(Material mat)
	{
		float startFactor = mat.GetFloat("_ColorFactor");
		float endFactor = 0.9f;
		float time = 0f;
		while (time < 3f)
		{
			float value = Mathf.Lerp(startFactor, endFactor, time / fadeDuration);
			mat.SetFloat("_ColorFactor", value);
			time += Time.deltaTime;
			yield return null;
		}
		mat.SetFloat("_ColorFactor", endFactor);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_PlayBossKillBehaviorNetworked()
	{
		sunLight.SetActive(value: true);
		Material material = sfDecal.GetComponent<MeshRenderer>().material;
		Material material2 = lightDecal.GetComponent<MeshRenderer>().material;
		MeshRenderer[] array = jailRenderers;
		for (int i = 0; i < array.Length; i++)
		{
			Material material3 = array[i].material;
			((MonoBehaviour)(object)this).StartCoroutine(FadeEmissionRoutine(material3));
		}
		((MonoBehaviour)(object)this).StartCoroutine(FadeAlphaRoutine(material));
		((MonoBehaviour)(object)this).StartCoroutine(FadeAlphaRoutine(material2));
	}

	protected static void InvokeUserCode_PlayBossKillBehaviorNetworked(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC PlayBossKillBehaviorNetworked called on server.");
		}
		else
		{
			((BossLightElementalKillBehavior)(object)obj).UserCode_PlayBossKillBehaviorNetworked();
		}
	}

	static BossLightElementalKillBehavior()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(BossLightElementalKillBehavior), "System.Void BossLightElementalKillBehavior::PlayBossKillBehaviorNetworked()", (RemoteCallDelegate)InvokeUserCode_PlayBossKillBehaviorNetworked);
	}
}

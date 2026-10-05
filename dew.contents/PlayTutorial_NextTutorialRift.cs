using System.Collections;
using UnityEngine;

public class PlayTutorial_NextTutorialRift : Rift
{
	public int nextTutorialIndex;

	public Transform teleportDestStart;

	public Transform teleportDestEnd;

	protected override bool OnInteractRift(Hero hero)
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		return true;
		IEnumerator Routine()
		{
			ManagerBase<ControlManager>.instance.DisableCharacterControls();
			yield return new WaitForSeconds(1f);
			Se_PortalTransition se = hero.CreateStatusEffect<Se_PortalTransition>(hero, default);
			yield return new WaitForSeconds(0.5f);
			DewNetworkManager.instance.SetLoadingStatus(isLoading: true);
			yield return ManagerBase<TransitionManager>.instance.FadeOutRoutine(showTips: false);
			hero.Teleport(hero, teleportDestStart.position);
			PlayTutorialManager.instance.SetTutorial(nextTutorialIndex);
			yield return new WaitForSeconds(0.5f);
			DewNetworkManager.instance.SetLoadingStatus(isLoading: false);
			yield return ManagerBase<TransitionManager>.instance.FadeInRoutine();
			se.Destroy();
			hero.Control.MoveToDestination(teleportDestEnd.position, immediately: true);
			ManagerBase<ControlManager>.instance.EnableCharacterControls();
		}
	}

	private void MirrorProcessed()
	{
	}
}

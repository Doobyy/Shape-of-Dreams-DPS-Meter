using UnityEngine;

public class PlayTutorial_StartGameRift : Rift, IPlayerPathablePoint
{
	public View tutorialDiffView;

	public Vector3 pathablePosition => Dew.GetPositionOnGround(((Component)(object)this).transform.position);

	protected override bool OnInteractRift(Hero hero)
	{
		tutorialDiffView.Show();
		return false;
	}

	private void MirrorProcessed()
	{
	}
}

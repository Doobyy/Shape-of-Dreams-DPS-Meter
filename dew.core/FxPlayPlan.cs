using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.VFX;

[DisallowMultipleComponent]
public class FxPlayPlan : MonoBehaviour
{
	[Serializable]
	public struct Node
	{
		public GameObject go;

		public FxSelectiveVisibility vis;

		public FxGameObject fxGameObject;

		public MonoBehaviour[] setups;

		public int subtreeEnd;
	}

	public Node[] nodes;

	public ParticleSystem[] rootParticleSystems;

	public ParticleSystem[] clearSelfOnStopParticleSystems;

	public ParticleSystem[] clearWithChildrenOnStopParticleSystems;

	public VisualEffect[] visualEffects;

	public CinemachineImpulseSource[] impulses;

	public Component[] effectComponents;
}

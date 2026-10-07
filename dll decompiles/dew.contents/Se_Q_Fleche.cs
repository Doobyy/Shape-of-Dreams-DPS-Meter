using System;
using System.Collections.Generic;
using UnityEngine;

public class Se_Q_Fleche : StackedStatusEffect
{
	public ScalingValue startStack;

	public ScalingValue maxRefundCount;

	public float duration;

	public float refundGraceTime;

	public bool resetDurationOnActivate;

	public Vector2 distanceRange;

	public Transform stackItemParent;

	public GameObject stackDisplayPrefabOn;

	public GameObject stackDisplayPrefabRefundable;

	public GameObject stackDisplayPrefabOff;

	public float stackItemSpacing;

	public GameObject refundEffect;

	[NonSerialized]
	public Dictionary<Entity, float> refundList;

	private List<(GameObject, GameObject, GameObject)> _stackDisplayItems;

	private int _maxRefundCount;

	private int _currentRefundCount;

	protected override void OnCreate()
	{
	}

	protected override void ActiveLogicUpdate(float dt)
	{
	}

	protected override void OnDestroyActor()
	{
	}

	protected override void ActiveFrameUpdate()
	{
		// ILSpy could not decompile this. Please report the exception below,
		// along with the assembly it came from, at https://github.com/icsharpcode/ILSpy/issues/new
		// System.IndexOutOfRangeException: Index was outside the bounds of the array.
		//    at ICSharpCode.Decompiler.Util.BitSet.Set(Int32 index) in /_/ICSharpCode.Decompiler/Util/BitSet.cs:line 196
		//    at ICSharpCode.Decompiler.Util.BitSet.set_Item(Int32 index, Boolean value) in /_/ICSharpCode.Decompiler/Util/BitSet.cs:line 69
		//    at ICSharpCode.Decompiler.IL.ILReader.PrepareBranchTargetsAndStacksForExceptionHandlers() in /_/ICSharpCode.Decompiler/IL/ILReader.cs:line 646
		//    at ICSharpCode.Decompiler.IL.ILReader.ReadInstructions(CancellationToken cancellationToken) in /_/ICSharpCode.Decompiler/IL/ILReader.cs:line 499
		//    at ICSharpCode.Decompiler.IL.ILReader.ReadIL(MethodDefinitionHandle method, MethodBodyBlock body, GenericContext genericContext, ILFunctionKind kind, CancellationToken cancellationToken) in /_/ICSharpCode.Decompiler/IL/ILReader.cs:line 726
		//    at ICSharpCode.Decompiler.CSharp.CSharpDecompiler.DecompileBody(IMethod method, EntityDeclaration entityDecl, DecompileRun decompileRun, ITypeResolveContext decompilationContext, ExtensionInfo extensionInfo) in /_/ICSharpCode.Decompiler/CSharp/CSharpDecompiler.cs:line 2391
	}

	protected override void OnStackChange(int oldStack, int newStack)
	{
	}

	private void MirrorProcessed()
	{
	}
}

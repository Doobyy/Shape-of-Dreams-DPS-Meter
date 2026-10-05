using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public class Se_Nor_WindowToTheSoul_SpiritualDominance : StatusEffect
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct Ad_DidInitialReveal
	{
	}

	private struct Ad_UsedInstance
	{
		public List<Entity> revealed;
	}

	[CompilerGenerated]
	private sealed class _003CRevealWeaknessRoutine_003Ed__19 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Entity victim;

		public Se_Nor_WindowToTheSoul_SpiritualDominance _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CRevealWeaknessRoutine_003Ed__19(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			Se_Nor_WindowToTheSoul_SpiritualDominance se_Nor_WindowToTheSoul_SpiritualDominance = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (victim.Status.HasStatusEffect<Se_Nor_WindowToTheSoul_WeaknessRevealed>())
				{
					_003C_003E2__current = default(SI.WaitForNextLogicUpdate);
					_003C_003E1__state = 1;
					return true;
				}
				break;
			case 1:
				_003C_003E1__state = -1;
				if (victim.Status.HasStatusEffect<Se_Nor_WindowToTheSoul_WeaknessRevealed>())
				{
					return false;
				}
				break;
			}
			se_Nor_WindowToTheSoul_SpiritualDominance.CreateStatusEffect<Se_Nor_WindowToTheSoul_WeaknessRevealed>(victim);
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	public DewCollider revealRange;

	public AbilityTargetValidator revealable;

	public float applyWeaknessInterval;

	public ScalingValue weaknessDmgFactor;

	public ScalingValue shieldFactor;

	public float maxShieldMaxHpFactor;

	public float speedStrength;

	public float speedDecayDuration;

	public float armorAmount;

	private float _lastApplyWeaknessCheckTime;

	private SpeedEffect _speed;

	private ArmorBoostEffect _armor;

	private Action<EventInfoAttackEffect> _cachedHandleAttackEffect;

	private Action<EventInfoAbilityInstance> _cachedHandleAbilityInstance;

	private readonly List<(AbilityInstance instance, Action<EventInfoDamage> handler)> _dealDamageSubscriptions;

	protected override void OnCreate()
	{
	}

	private void HandleAbilityInstance(EventInfoAbilityInstance info)
	{
	}

	[IteratorStateMachine(typeof(_003CRevealWeaknessRoutine_003Ed__19))]
	private IEnumerator RevealWeaknessRoutine(Entity victim)
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		// ILSpy could not decompile this. Please report the exception below,
		// along with the assembly it came from, at https://github.com/icsharpcode/ILSpy/issues/new
		// System.IndexOutOfRangeException: Index was outside the bounds of the array.
		//    at ICSharpCode.Decompiler.Util.BitSet.Set(Int32 index) in /_/ICSharpCode.Decompiler/Util/BitSet.cs:line 196
		//    at ICSharpCode.Decompiler.Util.BitSet.set_Item(Int32 index, Boolean value) in /_/ICSharpCode.Decompiler/Util/BitSet.cs:line 69
		//    at ICSharpCode.Decompiler.IL.ILReader.PrepareBranchTargetsAndStacksForExceptionHandlers() in /_/ICSharpCode.Decompiler/IL/ILReader.cs:line 613
		//    at ICSharpCode.Decompiler.IL.ILReader.ReadInstructions(CancellationToken cancellationToken) in /_/ICSharpCode.Decompiler/IL/ILReader.cs:line 499
		//    at ICSharpCode.Decompiler.IL.ILReader.ReadIL(MethodDefinitionHandle method, MethodBodyBlock body, GenericContext genericContext, ILFunctionKind kind, CancellationToken cancellationToken) in /_/ICSharpCode.Decompiler/IL/ILReader.cs:line 726
		//    at ICSharpCode.Decompiler.CSharp.CSharpDecompiler.DecompileBody(IMethod method, EntityDeclaration entityDecl, DecompileRun decompileRun, ITypeResolveContext decompilationContext, ExtensionInfo extensionInfo) in /_/ICSharpCode.Decompiler/CSharp/CSharpDecompiler.cs:line 2391
	}

	protected override void OnDestroyActor()
	{
		// ILSpy could not decompile this. Please report the exception below,
		// along with the assembly it came from, at https://github.com/icsharpcode/ILSpy/issues/new
		// System.IndexOutOfRangeException: Index was outside the bounds of the array.
		//    at ICSharpCode.Decompiler.Util.BitSet.Set(Int32 index) in /_/ICSharpCode.Decompiler/Util/BitSet.cs:line 196
		//    at ICSharpCode.Decompiler.Util.BitSet.set_Item(Int32 index, Boolean value) in /_/ICSharpCode.Decompiler/Util/BitSet.cs:line 69
		//    at ICSharpCode.Decompiler.IL.ILReader.PrepareBranchTargetsAndStacksForExceptionHandlers() in /_/ICSharpCode.Decompiler/IL/ILReader.cs:line 613
		//    at ICSharpCode.Decompiler.IL.ILReader.ReadInstructions(CancellationToken cancellationToken) in /_/ICSharpCode.Decompiler/IL/ILReader.cs:line 499
		//    at ICSharpCode.Decompiler.IL.ILReader.ReadIL(MethodDefinitionHandle method, MethodBodyBlock body, GenericContext genericContext, ILFunctionKind kind, CancellationToken cancellationToken) in /_/ICSharpCode.Decompiler/IL/ILReader.cs:line 726
		//    at ICSharpCode.Decompiler.CSharp.CSharpDecompiler.DecompileBody(IMethod method, EntityDeclaration entityDecl, DecompileRun decompileRun, ITypeResolveContext decompilationContext, ExtensionInfo extensionInfo) in /_/ICSharpCode.Decompiler/CSharp/CSharpDecompiler.cs:line 2391
	}

	private void HandleAttackEffect(EventInfoAttackEffect info)
	{
	}

	private void MirrorProcessed()
	{
	}
}

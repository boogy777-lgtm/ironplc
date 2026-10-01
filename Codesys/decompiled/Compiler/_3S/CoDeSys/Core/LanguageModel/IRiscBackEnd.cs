using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.TargetSettings;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscBackEnd
	{
		IRiscOptions Options { get; }

		string[] FunctionsToLinkAlways { get; }

		int StackDisplacement { get; }

		int ParameterDisplacement { get; }

		bool MotorolaByteOrder { get; }

		IRegister InstancePointer { get; }

		IRegister StackPointer { get; }

		IRegister FramePointer { get; }

		void Setup(ITargetSettings targetSettings);

		void Initialize(IRiscFrontEnd frontend, ICodeAdapter adapter, IScope scope);

		bool NeedsExternalFunctionCall(IConversionExpression conv, ref string stFunctionName, ref TypeClass tcWithType);

		bool NeedsExternalFunctionCall(IOperatorExpression op, TypeClass tc, ref string stFunctionName, ref TypeClass tcWithType);

		void DefineBreakpoint(IExprement expr);

		void AppendComment(string stComment);

		void Assert(bool bAssert, string stError);

		IRegister AllocateRegister(TypeClass tc);

		void FreeRegister(IRegister Reg);

		bool CheckRegisterManager();

		IRegister GetGlobalDataPointer(int nAddress, int iArea, out int nOffset);

		void GeneratePOUProlog(bool bSystemEntry, int nStackSize, int iArea, ICompiledPOU cpou);

		void GeneratePOUEpilog(bool bSystemExit, int nStackSize);

		ICompiledCode GenerateCode(ICompiledPOU cpou);

		void CodeJump(IRegister RegCond, string stLabel, bool bJumpIfFalse);

		void CodeJump(string stLabel);

		bool CodeJump(Operator op, TypeClass tcOperands, IRegister Reg1, IRegister Reg2, string stLabel);

		bool CodeJump(Operator op, TypeClass tcOperands, IRegister Reg, long l, string stLabel);

		string DefineLabel(string stLabel);

		void CodeCall(IRegister RegAddress, bool bExternal);

		void CodeMove(IRegister RegSource, IRegister RegDest);

		void CodeIncrement(IRegister Reg, byte byCount);

		void CodeStringCopy(IRegister RegSrc, IRegister RegDest, uint uiSize, TypeClass tc);

		void CodeMemcopy(IRegister RegSrc, IRegister RegDest, uint count);

		bool HasDirectAccess(TypeClass tc, Access access);

		void CodeRegisterBitAccess(IRegister RegSrc, IRegister RegDest, Access access, int nBitNr);

		void CodeAllocateStack(int nSize);

		void CodeFreeStack(int nSize);

		void CodeLoadConstant(long l, TypeClass tc, IRegister RegDest);

		void CodeLoadConstant(string st, TypeClass tc, IRegister RegDest);

		void CodeLoad(TypeClass tc, IRegister RegBase, IRegister RegIndex, int nOffset, IRegister RegDest, int nScale);

		void CodeStore(TypeClass tc, IRegister RegBase, IRegister RegIndex, int nOffset, IRegister RegSrc, int nScale);

		void CodeLoadAddress(IRegister RegBase, IRegister RegIndex, int nOffset, IRegister RegDest, int nScale);

		void CodeOperator(Operator op, TypeClass tc, IRegister Reg1, IRegister Reg2, ref IRegister RegDest);

		bool CodeOperatorConst(Operator op, TypeClass tc, IRegister Reg1, long l, ref IRegister RegDest, bool bOp1IsConst);

		IRegister CodeConvert(IRegister RegSrc, TypeClass tcFrom, TypeClass tcTo, bool bImplicit);

		IRiscJumpTable CreateJumpTable(TypeClass tc, long lStart, long lEnd);

		void CodeJumpTable(IRegister RegSwitch, IRiscJumpTable jt);

		void CodeMemoryBitAccess(IRegister RegSrc, IRegister RegDest, Access access, int nBitNr);
	}
}

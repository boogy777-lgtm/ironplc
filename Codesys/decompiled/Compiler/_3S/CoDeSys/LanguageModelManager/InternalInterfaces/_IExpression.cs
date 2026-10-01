using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IExpression : _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression
	{
		int VariableId { get; set; }

		bool IsPOUReference { get; }

		int SignatureId { get; set; }

		new ICompiledType Type { get; set; }

		bool IsCompiled { get; }

		ICompiledType _CompiledType { get; set; }

		new int ScratchOffset { get; set; }

		new int PrecompileVariableId { get; set; }

		new int PrecompileSignatureId { get; set; }

		bool IsVarInOutInput(IScope scope, bool bWriteToConstants, bool bVarInoutConstant);

		bool IsLValue(IScope scope, bool bWriteToConstants);

		bool IsLValue(IScope scope, bool bWriteToConstants, bool bPassToVarInout);

		bool IsLValue(IScope scope, bool bWriteToConstants, bool bPassToVarInout, bool bRefAssign);

		IVariable GetVariable(IPrecompileScope scope);

		bool IsEqual(IExpression expression);

		ILiteralValue LiteralUnchecked(IScope scope);

		ILiteralValue LiteralWithRecursionCheck(IScope scope, IDictionary<IVariable, IVariable> variableStack, bool bAllocatedOK, out bool bRecursionError);

		ILiteralValue LiteralWithRecursionCheck(IPrecompileScope scope, IDictionary<IVariable, IVariable> variableStack, bool bAllocatedOK, out bool bRecursionError);

		ILiteralValue Literal(IScope scope, bool bAllocatedOK);
	}
}

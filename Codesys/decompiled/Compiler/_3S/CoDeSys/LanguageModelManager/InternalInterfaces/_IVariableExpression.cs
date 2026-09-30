using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IVariableExpression : _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IVariableExpression3, IVariableExpression2, IVariableExpression
	{
		new string Name { get; set; }

		new int ScopeId { get; set; }

		IVariableExprInfo VarInfo { get; set; }

		bool NoVirtual { get; set; }

		new int SignatureId { get; set; }

		new int VariableId { get; set; }

		new int PrecompileVariableId { get; set; }

		new int PrecompileSignatureId { get; set; }

		bool GetFlag(VarExprFlag vfFlag);

		void SetFlag(VarExprFlag vfFlag, bool bSetTrue);

		new ISignature GetSignature(IScope scope);

		new IVariable GetVariable(IScope scope);
	}
}

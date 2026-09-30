using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IPrecompileChecker : IExprementVisitor2, IExprementVisitor, IExprementVisitor3590, IOperatorExpressionVisitor
	{
		IType DerivedType { get; }

		IVariable DerivedVariable { get; }

		IPrecompileScope SearchScope { get; }

		ISignature DerivedSignature { get; }

		IPrecompileScope DerivedScope { get; }

		IList<_ICompilerMessage> Messages { get; }

		void Check(_ISignature sign);

		void Check(_IVariable var);

		void Check(_IVariable var, _IType type);

		void Check(_IExpression exp);
	}
}

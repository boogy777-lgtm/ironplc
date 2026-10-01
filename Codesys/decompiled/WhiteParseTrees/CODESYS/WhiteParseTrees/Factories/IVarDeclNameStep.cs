using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees.Factories
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IVarDeclNameStep
	{
		IVarDeclTypeStep WithName(IEnumerable<IWhiteExpression> VariableNames);

		IVarDeclTypeStep WithName(IWhiteExpression VariableName);
	}
}

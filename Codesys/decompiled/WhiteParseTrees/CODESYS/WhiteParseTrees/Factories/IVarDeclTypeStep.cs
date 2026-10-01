using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees.Factories
{
	[ReleasedInterface]
	public interface IVarDeclTypeStep : IVarDeclAtStep
	{
		[NullableContext(1)]
		IVarDeclInitializationStep WithType(IWhiteTypeExpression type);
	}
}

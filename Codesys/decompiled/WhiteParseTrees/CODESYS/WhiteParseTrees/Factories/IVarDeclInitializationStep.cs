using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees.Factories
{
	[ReleasedInterface]
	public interface IVarDeclInitializationStep : IVarDeclTypeStep, IVarDeclAtStep
	{
		[NullableContext(1)]
		IVariableDeclarationBuilder WithInitialization(IWhiteExpression initialization);

		[NullableContext(1)]
		IVariableDeclarationBuilder Finish();
	}
}

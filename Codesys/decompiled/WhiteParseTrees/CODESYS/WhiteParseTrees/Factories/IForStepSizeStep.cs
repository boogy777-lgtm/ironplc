using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees.Factories
{
	[ReleasedInterface]
	public interface IForStepSizeStep : IForControlledStep
	{
		[NullableContext(1)]
		IForControlledStep WithStepSize(IWhiteExpression stepWidth);
	}
}

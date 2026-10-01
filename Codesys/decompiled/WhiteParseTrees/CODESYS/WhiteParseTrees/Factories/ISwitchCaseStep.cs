using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees.Factories
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface ISwitchCaseStep
	{
		ISwithAddCaseStep WithSwitch(IWhiteExpression switchStatement);
	}
}

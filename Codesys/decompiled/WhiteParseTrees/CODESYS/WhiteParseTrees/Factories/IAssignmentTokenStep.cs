using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees.Factories
{
	[ReleasedInterface]
	public interface IAssignmentTokenStep : IRValueStep
	{
		[NullableContext(1)]
		IRValueStep WithAssignmentToken(IAnyAssignmentToken token);
	}
}

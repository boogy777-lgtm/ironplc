using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IVariable4 : IVariable3, IVariable2, IVariable
	{
		int PrecompileId { get; }

		IEnumerable<ICrossReference> PrecompileCrossReferences { get; }
	}
}

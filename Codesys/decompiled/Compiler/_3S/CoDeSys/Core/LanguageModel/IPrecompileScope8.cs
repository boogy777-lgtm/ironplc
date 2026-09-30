using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPrecompileScope8 : IPrecompileScope7, IPrecompileScope6, IPrecompileScope5, IPrecompileScope4, IPrecompileScope3, IPrecompileScope2, IPrecompileScope
	{
		IEnumerable<IIdentifierInfo> GetAllDeclarations(EWhichDeclarations eWhichDeclaration);
	}
}

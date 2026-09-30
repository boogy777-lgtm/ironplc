using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteParserService3 : IWhiteParserService2, IWhiteParserService
	{
		[NullableContext(1)]
		IWhitePOUSyntax[] ParsePOUs(string stInput);

		[NullableContext(1)]
		IEnumerable<IWhiteErrorInformation> CollectErrorStatements(INode root);
	}
}

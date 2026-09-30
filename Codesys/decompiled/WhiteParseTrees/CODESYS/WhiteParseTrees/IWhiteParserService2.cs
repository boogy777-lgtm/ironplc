using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;
using CODESYS.WhiteParseTrees.Factories;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteParserService2 : IWhiteParserService
	{
		IList<IWhiteToken> GetTokenList(string stInput);

		new IWhiteParseTreeBuilderFactory2 GetParseTreeBuilderFactory();
	}
}

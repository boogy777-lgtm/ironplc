using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Parser
{
	public static class PragmaChecker
	{
		[System.Runtime.CompilerServices.NullableContext(1)]
		public static bool CheckFor<T>(string stInput) where T : class, IWhiteOperatorToken
		{
			return new TokenControl(TokenStream.ReadTokenStream(stInput)).CheckNext<T>();
		}
	}
}

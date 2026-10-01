using System.Linq;
using System.Runtime.CompilerServices;
using CODESYS.WhiteParseTrees.Parser;

namespace CODESYS.WhiteParseTrees.Nodes.Factories
{
	public static class TokenFactory<T> where T : [System.Runtime.CompilerServices.Nullable(1)] IWhiteToken
	{
		[System.Runtime.CompilerServices.NullableContext(1)]
		public static T Create(string stInput)
		{
			return (T)TokenStream.ReadTokenStream(stInput).First();
		}
	}
}

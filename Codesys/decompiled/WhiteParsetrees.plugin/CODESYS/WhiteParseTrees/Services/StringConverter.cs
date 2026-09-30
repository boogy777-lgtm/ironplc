using System.Runtime.CompilerServices;
using System.Text;

namespace CODESYS.WhiteParseTrees.Services
{
	public static class StringConverter
	{
		[System.Runtime.CompilerServices.NullableContext(1)]
		public static string ConvertToString(INode node)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (IWhiteToken token in TokenSerializer.GetTokenList(node))
			{
				stringBuilder.Append(token.Text);
			}
			return stringBuilder.ToString();
		}
	}
}

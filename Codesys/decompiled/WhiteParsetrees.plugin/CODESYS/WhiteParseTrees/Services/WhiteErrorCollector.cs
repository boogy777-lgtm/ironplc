using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Services
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public static class WhiteErrorCollector
	{
		public static IEnumerable<IWhiteErrorInformation> CollectErrorStatements(INode root)
		{
			IDictionary<IWhiteToken, int> offsets = OffsetCalculator.CalculateOffsets(root);
			foreach (IWhiteErrorInformation item in CollectErrorStatementsRecur(root, offsets))
			{
				yield return item;
			}
		}

		[return: System.Runtime.CompilerServices.Nullable(2)]
		private static IWhiteToken TryGetErrorToken(IWhiteErrorStatement stmt)
		{
			if (stmt is IWhiteErrorStatement2 whiteErrorStatement && whiteErrorStatement.ErrorToken != null)
			{
				return whiteErrorStatement.ErrorToken;
			}
			if (stmt.TokenList != null && stmt.TokenList.Any())
			{
				return stmt.TokenList.First();
			}
			return null;
		}

		private static IEnumerable<IWhiteErrorInformation> CollectErrorStatementsRecur(INode node, IDictionary<IWhiteToken, int> offsets)
		{
			if (node is IWhiteErrorStatement whiteErrorStatement)
			{
				IWhiteToken whiteToken = TryGetErrorToken(whiteErrorStatement);
				int characterOffset = -1;
				if (whiteToken != null)
				{
					characterOffset = offsets[whiteToken];
				}
				yield return new ErrorInformation(whiteErrorStatement, characterOffset);
			}
			foreach (INode child in node.GetChildren())
			{
				foreach (IWhiteErrorInformation item in CollectErrorStatementsRecur(child, offsets))
				{
					yield return item;
				}
			}
		}
	}
}

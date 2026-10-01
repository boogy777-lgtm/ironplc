using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Services.Formatter.Passes
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteSpaceRemover
	{
		private readonly IFormatterSettings _settings;

		private WhiteSpaceRemover(IFormatterSettings settings)
		{
			_settings = settings;
		}

		public static void RemoveWhiteSpaces(INode node, IFormatterSettings settings)
		{
			new WhiteSpaceRemover(settings).RemoveWhiteSpacesFromNode(node);
		}

		private void RemoveWhiteSpacesFromNode(INode node)
		{
			if (!(node is IWhiteErrorStatement whiteErrorStatement))
			{
				if (node is IWhiteToken token)
				{
					RemoveWhiteSpacesFromToken(token);
				}
				{
					foreach (INode child in node.GetChildren())
					{
						RemoveWhiteSpacesFromNode(child);
					}
					return;
				}
			}
			RemoveWhiteSpacesFromToken(whiteErrorStatement.TokenList.First());
		}

		[System.Runtime.CompilerServices.NullableContext(2)]
		private void RemoveWhiteSpacesFromToken(IWhiteToken token)
		{
			if (token == null)
			{
				return;
			}
			IEnumerable<IWhiteToken> enumerable = from tok in WhiteSpaceFormatter.GetAllLeadingTokens(token)
				where !(tok is IWhitespaceToken)
				select tok;
			Type type = null;
			List<List<IWhiteToken>> list = new List<List<IWhiteToken>>();
			List<IWhiteToken> list2 = new List<IWhiteToken>();
			foreach (IWhiteToken item in enumerable)
			{
				if (item.GetType() == type)
				{
					list2.Add(item);
				}
				else
				{
					list.Add(list2);
					list2 = new List<IWhiteToken> { item };
				}
				type = item.GetType();
			}
			list.Add(list2);
			list.RemoveAt(0);
			IEnumerable<IWhiteToken> source = list.Select(delegate(List<IWhiteToken> group)
			{
				int count = ((!_settings.KeepLeadingEmptyLine) ? 1 : 2);
				return (!(group.FirstOrDefault() is IEndOfLineToken)) ? group : group.Take(count);
			}).SelectMany((IEnumerable<IWhiteToken> e) => e);
			token.Leading = null;
			IWhiteToken whiteToken = token;
			foreach (IWhiteToken item2 in source.Skip(1))
			{
				if (item2 == null)
				{
					break;
				}
				item2.Leading = null;
				whiteToken.Leading = item2;
				whiteToken = whiteToken.Leading;
			}
		}
	}
}

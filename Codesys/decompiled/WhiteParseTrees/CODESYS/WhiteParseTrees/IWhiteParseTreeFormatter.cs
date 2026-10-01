using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteParseTreeFormatter
	{
		void RemoveAllWhitespaces(INode node, IFormatterSettings settings);

		void FormatStatements(INode node, IFormatterSettings settings);
	}
}

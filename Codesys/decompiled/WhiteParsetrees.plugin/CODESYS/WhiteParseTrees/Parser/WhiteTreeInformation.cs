using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Parser
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	internal class WhiteTreeInformation : IWhiteTreeInformation
	{
		public INode RootNode { get; }

		public ISourcePositionMap SourcePositionMap { get; }

		internal WhiteTreeInformation(INode node, ISourcePositionMap map)
		{
			RootNode = node;
			SourcePositionMap = map;
		}
	}
}

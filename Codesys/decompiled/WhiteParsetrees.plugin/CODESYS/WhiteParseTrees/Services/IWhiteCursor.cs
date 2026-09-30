using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Services
{
	[System.Runtime.CompilerServices.NullableContext(2)]
	internal interface IWhiteCursor
	{
		int Offset { get; }

		int Length { get; }

		[System.Runtime.CompilerServices.Nullable(1)]
		INode Node
		{
			[System.Runtime.CompilerServices.NullableContext(1)]
			get;
		}

		WhiteExprementCursor Parent { get; }

		WhiteTokenCursor GetTokenAtOffset(int offset);
	}
}

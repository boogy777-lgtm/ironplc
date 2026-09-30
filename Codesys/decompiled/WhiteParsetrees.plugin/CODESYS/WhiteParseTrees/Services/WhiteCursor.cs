using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Services
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	internal static class WhiteCursor
	{
		[return: System.Runtime.CompilerServices.Nullable(2)]
		public static WhiteExprementCursor TryGetParentOfType<[System.Runtime.CompilerServices.Nullable(0)] T>(this WhiteTokenCursor self) where T : IWhiteExprement
		{
			return self.Parent?.TryGetParentOfType<T>();
		}

		[return: System.Runtime.CompilerServices.Nullable(2)]
		public static WhiteExprementCursor TryGetParentOfType<[System.Runtime.CompilerServices.Nullable(0)] T>(this WhiteExprementCursor self) where T : IWhiteExprement
		{
			WhiteExprementCursor whiteExprementCursor = self;
			do
			{
				if (whiteExprementCursor.Node is T)
				{
					return self;
				}
				whiteExprementCursor = whiteExprementCursor.Parent;
			}
			while (whiteExprementCursor != null);
			return null;
		}
	}
}

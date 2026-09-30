using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees.Factories
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteParseTreeExpressionFactory2 : IWhiteParseTreeExpressionFactory
	{
		IWhitePartialAccessExpression CreateWhitePartialAccessExpression(IWhiteExpression leftExpression, IPeriodToken period, IPartialAccessToken token);

		IWhiteCompoPartialAccessExpression CreateWhiteCompoPartialAccessExpression(IWhiteExpression leftExpression, IPeriodToken period, IWhitePartialAccessExpression rightExpression);
	}
}

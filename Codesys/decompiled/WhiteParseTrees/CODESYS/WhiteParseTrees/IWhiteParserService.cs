using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using CODESYS.WhiteParseTrees.Factories;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteParserService
	{
		IWhiteSequenceStatement ParseStImplementation(string stInput);

		IWhiteTreeInformation ParseStImplementationWithSourcePositions(string stInput);

		[return: Nullable(2)]
		IWhiteExprement FindExpressionAtTextOffset(INode root, int nTextOffset);

		[return: Nullable(2)]
		IWhiteExprement FindExpressionAtSourcePosition(IWhiteTreeInformation treeInformation, long lEditorPosition);

		[return: Nullable(2)]
		IWhiteExprement FindExpressionAtSourcePosition(IWhiteTreeInformation treeInformation, ISourcePosition sourcePosition);

		[return: Nullable(2)]
		IWhiteExprement FindSurroundingOperatorAtTextOffset(INode root, int nTextOffset, Operator operatorToMatch);

		[return: Nullable(2)]
		IWhiteExprement FindSurroundingOperatorAtTextOffset(IWhiteTreeInformation treeInformation, ISourcePosition sourcePosition, Operator operatorToMatch);

		[return: Nullable(2)]
		IWhiteExpression FindMatchingWhiteExpressionForRedExpression(IWhiteTreeInformation treeInformation, IExpression expression);

		IList<IWhiteToken> GetTokenList(INode node);

		IWhiteParseTreeExpressionFactory GetParseTreeExpressionFactory();

		IWhiteParseTreeStatementFactory GetParseTreeStatementFactory();

		IWhiteParseTreeBuilderFactory GetParseTreeBuilderFactory();

		IWhiteParseTreeFormatter GetParseTreeFormatter();

		IDictionary<IWhiteToken, int> CalculateOffsets(INode node);

		T CreateToken<[Nullable(0)] T>(string stInput) where T : IWhiteToken;

		bool ContainsErrorStatement(IWhiteSequenceStatement statement);
	}
}

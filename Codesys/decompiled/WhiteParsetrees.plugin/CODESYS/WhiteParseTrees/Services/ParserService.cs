using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using CODESYS.WhiteParseTrees.Factories;
using CODESYS.WhiteParseTrees.Nodes.Factories;
using CODESYS.WhiteParseTrees.Parser;
using CODESYS.WhiteParseTrees.Services.Formatter;

namespace CODESYS.WhiteParseTrees.Services
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	[ExcludeFromCodeCoverage]
	[TypeGuid("f8e734f5-2639-4aeb-bde2-f54049a15e78")]
	public class ParserService : IWhiteParserService3, IWhiteParserService2, IWhiteParserService
	{
		public IWhiteSequenceStatement ParseStImplementation(string stInput)
		{
			return WhiteTreeParser.ParseStImplementation(stInput);
		}

		public IWhiteTreeInformation ParseStImplementationWithSourcePositions(string stInput)
		{
			return WhiteTreeParser.ParseStImplementationWithSourcePositions(stInput);
		}

		[return: System.Runtime.CompilerServices.Nullable(2)]
		public IWhiteExprement FindExpressionAtTextOffset(INode root, int nTextOffset)
		{
			return ExpressionDeterminationService.FindExpressionAtTextOffset(root, nTextOffset);
		}

		[return: System.Runtime.CompilerServices.Nullable(2)]
		public IWhiteExprement FindExpressionAtSourcePosition(IWhiteTreeInformation treeInformation, long lEditorPosition)
		{
			return ExpressionDeterminationService.FindExpressionAtSourcePosition(treeInformation, lEditorPosition);
		}

		[return: System.Runtime.CompilerServices.Nullable(2)]
		public IWhiteExprement FindExpressionAtSourcePosition(IWhiteTreeInformation treeInformation, ISourcePosition sourcePosition)
		{
			return ExpressionDeterminationService.FindExpressionAtSourcePosition(treeInformation, sourcePosition);
		}

		[return: System.Runtime.CompilerServices.Nullable(2)]
		public IWhiteExprement FindSurroundingOperatorAtTextOffset(INode root, int nTextOffset, Operator operatorToMatch)
		{
			return ExpressionDeterminationService.FindSurroundingOperatorAtTextOffset(root, nTextOffset, operatorToMatch);
		}

		[return: System.Runtime.CompilerServices.Nullable(2)]
		public IWhiteExprement FindSurroundingOperatorAtTextOffset(IWhiteTreeInformation treeInformation, ISourcePosition sourcePosition, Operator operatorToMatch)
		{
			return ExpressionDeterminationService.FindSurroundingOperatorAtSourcePosition(treeInformation, sourcePosition, operatorToMatch);
		}

		[return: System.Runtime.CompilerServices.Nullable(2)]
		public IWhiteExpression FindMatchingWhiteExpressionForRedExpression(IWhiteTreeInformation treeInformation, IExpression expression)
		{
			return ExpressionDeterminationService.FindMatchingWhiteExpressionForRedExpression(treeInformation, expression);
		}

		public IWhiteParseTreeExpressionFactory GetParseTreeExpressionFactory()
		{
			return new ExpressionFactory();
		}

		public IWhiteParseTreeStatementFactory GetParseTreeStatementFactory()
		{
			return new StatementFactory();
		}

		IWhiteParseTreeBuilderFactory2 IWhiteParserService2.GetParseTreeBuilderFactory()
		{
			return new BuilderFactory();
		}

		public IWhiteParseTreeBuilderFactory GetParseTreeBuilderFactory()
		{
			return new BuilderFactory();
		}

		public IWhiteParseTreeFormatter GetParseTreeFormatter()
		{
			return new WhiteParseTreeFormatter();
		}

		public IDictionary<IWhiteToken, int> CalculateOffsets(INode node)
		{
			return OffsetCalculator.CalculateOffsets(node);
		}

		public T CreateToken<[System.Runtime.CompilerServices.Nullable(0)] T>(string stInput) where T : IWhiteToken
		{
			return TokenFactory<T>.Create(stInput);
		}

		public bool ContainsErrorStatement(IWhiteSequenceStatement statement)
		{
			return ErrorStatementVisitor.ContainsErrorStatement(statement);
		}

		public IList<IWhiteToken> GetTokenList(INode node)
		{
			return TokenSerializer.GetTokenList(node);
		}

		public IList<IWhiteToken> GetTokenList(string stInput)
		{
			return TokenStream.ReadTokenStream(stInput);
		}

		public IWhitePOUSyntax[] ParsePOUs(string stInput)
		{
			return WhiteTreeParser.ParsePOUSyntax(stInput);
		}

		public IEnumerable<IWhiteErrorInformation> CollectErrorStatements(INode root)
		{
			return WhiteErrorCollector.CollectErrorStatements(root);
		}
	}
}

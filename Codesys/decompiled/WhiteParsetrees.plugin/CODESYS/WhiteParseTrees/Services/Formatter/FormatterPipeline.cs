using System.Runtime.CompilerServices;
using CODESYS.WhiteParseTrees.Nodes.Statements;
using CODESYS.WhiteParseTrees.Services.Formatter.Passes;

namespace CODESYS.WhiteParseTrees.Services.Formatter
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public static class FormatterPipeline
	{
		public static void Format(INode node, IFormatterSettings settings)
		{
			IWhiteSequenceStatement sequenceStatement = ConvertToSequenceStatement(node);
			PassRemoveWhiteSpaces.Execute(sequenceStatement, settings);
			new StmtExprFormatterVisitor(settings).Perform(sequenceStatement);
			PassInlineCommentFixes.Execute(sequenceStatement);
			PassAlignVarDeclElements.Execute(sequenceStatement, settings);
			PassRemoveLeadingAndTrailing.Execute(sequenceStatement);
			PassFixIndentation.Execute(sequenceStatement);
		}

		private static IWhiteSequenceStatement ConvertToSequenceStatement(INode node)
		{
			WhiteSequenceStatement whiteSequenceStatement = new WhiteSequenceStatement();
			if (node is IWhiteStatement whiteStatement)
			{
				if (!(whiteStatement is IWhiteSequenceStatement))
				{
					whiteSequenceStatement.Add(whiteStatement);
				}
				else if (whiteStatement is IWhiteSequenceStatement whiteSequenceStatement2)
				{
					whiteSequenceStatement = (WhiteSequenceStatement)whiteSequenceStatement2;
				}
			}
			return whiteSequenceStatement;
		}
	}
}

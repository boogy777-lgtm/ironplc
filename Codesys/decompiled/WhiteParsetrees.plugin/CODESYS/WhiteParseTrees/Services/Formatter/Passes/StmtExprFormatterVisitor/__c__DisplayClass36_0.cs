using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Services.Formatter.Passes
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class StmtExprFormatterVisitor : IStatementSyntax2.IStatementVisitor2<bool, LineCtx>, IStatementSyntax.IStatementVisitor<bool, LineCtx>, IExpressionSyntax3.IExpressionVisitor3<bool, LineCtx>, IExpressionSyntax2.IExpressionVisitor2<bool, LineCtx>, IExpressionSyntax.IExpressionVisitor<bool, LineCtx>
	{
		[System.Runtime.CompilerServices.NullableContext(0)]
		private delegate bool VisitDelegate(IStatementSyntax.IStatementVisitor<bool, LineCtx> visitor, LineCtx context);

		private readonly IFormatterSettings _settings;

		private bool _globalFirst = true;

		public void Perform(IWhiteSequenceStatement sequenceStatement)
		{
			sequenceStatement.Accept(this, new LineCtx());
		}

		public StmtExprFormatterVisitor(IFormatterSettings settings)
		{
			_settings = settings;
		}

		private static void AddSpace([System.Runtime.CompilerServices.Nullable(2)] INode node, LineCtx ctx)
		{
			IWhiteToken firstToken = WhiteSpaceFormatter.GetFirstToken(node);
			if (firstToken != null && !(firstToken.Leading is IEndOfLineToken))
			{
				WhiteSpaceFormatter.AddSpace(firstToken);
				ctx.ExtendLineLength(1);
			}
		}

		private static void AddCurrentIndentToLine([System.Runtime.CompilerServices.Nullable(2)] INode node, LineCtx ctx, int additionalTabs)
		{
			AddIndentToLine(node, ctx.Indentation + additionalTabs);
		}

		[System.Runtime.CompilerServices.NullableContext(2)]
		private static void AddIndentToLine(INode node, int tabs)
		{
			if (tabs == 0)
			{
				return;
			}
			IWhiteToken firstToken = WhiteSpaceFormatter.GetFirstToken(node);
			if (firstToken == null)
			{
				return;
			}
			List<IWhiteToken> list = new List<IWhiteToken>();
			for (IWhiteToken whiteToken = firstToken; whiteToken != null; whiteToken = whiteToken.Leading)
			{
				if (whiteToken.Leading is IEndOfLineToken && !(whiteToken is IEndOfLineToken))
				{
					list.Add(whiteToken);
				}
			}
			foreach (IWhiteToken item in list)
			{
				for (int i = 0; i < tabs; i++)
				{
					WhiteSpaceFormatter.AddTab(item);
				}
			}
		}

		private static void AddLine([System.Runtime.CompilerServices.Nullable(2)] INode node, LineCtx ctx)
		{
			IWhiteToken firstToken = WhiteSpaceFormatter.GetFirstToken(node);
			if (firstToken != null && !WhiteSpaceFormatter.TokenLeadingContains(firstToken, typeof(IEndOfLineToken)))
			{
				WhiteSpaceFormatter.AddNewLine(firstToken);
				ctx.ResetLineLength();
			}
		}

		private static void AddLineAndIndentToLine([System.Runtime.CompilerServices.Nullable(2)] INode node, LineCtx ctx, int additionalTabs = 0)
		{
			AddLine(node, ctx);
			AddCurrentIndentToLine(node, ctx, additionalTabs);
			ctx.ResetLineLength();
		}

		private static void BundledTokenChecks(IEnumerable<IWhiteOperatorToken> tokens, LineCtx ctx)
		{
			foreach (IWhiteOperatorToken token in tokens)
			{
				BundledTokenChecks(token, ctx);
			}
		}

		private static void BundledTokenChecks(IEnumerable<IWhiteToken> tokens, LineCtx ctx)
		{
			foreach (IWhiteToken token in tokens)
			{
				BundledTokenChecks(token, ctx);
			}
		}

		private static void BundledTokenChecks([System.Runtime.CompilerServices.Nullable(2)] IWhiteToken token, LineCtx ctx)
		{
			if (token != null)
			{
				IndentUserDefLinebreaks(token, ctx);
				ctx.ExtendLineLength(token);
			}
		}

		private static void IndentUserDefLinebreaks(IWhiteToken token, LineCtx ctx)
		{
			IndentationLevelBeforeNewLine(token, out var indentationLevelBeforeNl, out var foundNewLine);
			if (foundNewLine && indentationLevelBeforeNl == 0)
			{
				int tabs;
				if (ctx.IsInStatement())
				{
					tabs = ctx.Indentation;
				}
				else
				{
					bool upperIsExpressionStatement;
					IList<IWhiteToken> tokenList = TokenSerializer.GetTokenList(ctx.GetTopExpressionInChain(out upperIsExpressionStatement));
					tabs = ((upperIsExpressionStatement && IsLeftMost(token, tokenList)) ? ctx.Indentation : ((!IsMultiLineExpression(tokenList)) ? ctx.Indentation : (ctx.Indentation + 1)));
				}
				AddIndentToLine(token, tabs);
			}
		}

		private static bool IsLeftMost(IWhiteToken token, IEnumerable<IWhiteToken> serialized)
		{
			return serialized.SkipWhile(typeof(IEndOfLineToken).IsInstanceOfType).FirstOrDefault() == token;
		}

		private static void IndentationLevelBeforeNewLine(IWhiteToken token, out int indentationLevelBeforeNl, out bool foundNewLine)
		{
			IWhiteToken whiteToken = token;
			indentationLevelBeforeNl = 0;
			foundNewLine = false;
			while (true)
			{
				if (whiteToken is IWhitespaceToken && whiteToken.Text == "\t")
				{
					indentationLevelBeforeNl++;
				}
				if (whiteToken is IEndOfLineToken)
				{
					foundNewLine = true;
					break;
				}
				if (whiteToken.Leading != null)
				{
					whiteToken = whiteToken.Leading;
					continue;
				}
				break;
			}
		}

		private static bool IsMultiLineExpression(IEnumerable<IWhiteToken> serialized)
		{
			return serialized.SkipWhile(typeof(IEndOfLineToken).IsInstanceOfType).Any(typeof(IEndOfLineToken).IsInstanceOfType);
		}

		private void ScopedIndentation(VisitDelegate func, LineCtx ctx, int indentation)
		{
			ctx.Indent(indentation);
			func(this, ctx);
			ctx.UnIndent();
		}

		public bool visit(IWhiteSequenceStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			foreach (IWhiteStatement item in statement)
			{
				if (_globalFirst)
				{
					_globalFirst = false;
				}
				else if (item is IWhiteCommentStatement || item is IWhiteDocuCommentStatement)
				{
					IWhiteToken leadingToken = item.GetLeadingToken();
					if (leadingToken != null && WhiteSpaceFormatter.TokenLeadingContains(leadingToken, typeof(IEndOfLineToken)))
					{
						AddCurrentIndentToLine(item, ctx, 0);
					}
				}
				else
				{
					AddLineAndIndentToLine(item, ctx);
				}
				item.Accept(this, ctx);
			}
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteExpressionStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			statement.Expr.Accept(this, ctx);
			BundledTokenChecks(statement.Semicolon, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteElseIfStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			AddLineAndIndentToLine(statement.ElseIf, ctx);
			BundledTokenChecks(statement.ElseIf, ctx);
			AddSpace(statement.Condition, ctx);
			statement.Condition.Accept(this, ctx);
			AddSpace(statement.Then, ctx);
			BundledTokenChecks(statement.Then, ctx);
			ScopedIndentation(statement.ThenStatement.Accept, ctx, 1);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteIfStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			BundledTokenChecks(statement.If, ctx);
			AddSpace(statement.Condition, ctx);
			statement.Condition.Accept(this, ctx);
			BundledTokenChecks(statement.Then, ctx);
			AddSpace(statement.Then, ctx);
			ScopedIndentation(statement.ThenStatement.Accept, ctx, 1);
			AddLineAndIndentToLine(statement.Else, ctx);
			BundledTokenChecks(statement.Else, ctx);
			if (statement.ElseStatement != null)
			{
				ScopedIndentation(statement.ElseStatement.Accept, ctx, 1);
			}
			foreach (IWhiteElseIfStatement item in statement.ElseIfStatement)
			{
				item.Accept(this, ctx);
			}
			AddLineAndIndentToLine(statement.EndIf, ctx);
			BundledTokenChecks(statement.EndIf, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteCaseStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			BundledTokenChecks(statement.Case, ctx);
			AddSpace(statement.Switch, ctx);
			statement.Switch.Accept(this, ctx);
			AddSpace(statement.Of, ctx);
			BundledTokenChecks(statement.Of, ctx);
			foreach (IWhiteCase @case in statement.Cases)
			{
				ScopedIndentation(@case.Label.Accept, ctx, 1);
				ScopedIndentation(@case.Controlled.Accept, ctx, 2);
			}
			AddLineAndIndentToLine(statement.ElseToken, ctx, 1);
			BundledTokenChecks(statement.ElseToken, ctx);
			if (statement.Else != null)
			{
				ScopedIndentation(statement.Else.Accept, ctx, 2);
			}
			AddLineAndIndentToLine(statement.EndCase, ctx);
			BundledTokenChecks(statement.EndCase, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteCaseLabelStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			AddLineAndIndentToLine(statement, ctx);
			foreach (IWhiteExpression caseExpression in statement.CaseExpressionList)
			{
				caseExpression.Accept(this, ctx);
			}
			BundledTokenChecks(statement.Colon, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteForStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			BundledTokenChecks(statement.For, ctx);
			AddSpace(statement.StartExpression, ctx);
			statement.StartExpression.Accept(this, ctx);
			AddSpace(statement.To, ctx);
			BundledTokenChecks(statement.To, ctx);
			AddSpace(statement.UpperBound, ctx);
			statement.UpperBound.Accept(this, ctx);
			AddSpace(statement.By, ctx);
			BundledTokenChecks(statement.By, ctx);
			AddSpace(statement.StepWidth, ctx);
			statement.StepWidth?.Accept(this, ctx);
			AddSpace(statement.Do, ctx);
			BundledTokenChecks(statement.Do, ctx);
			ScopedIndentation(statement.Controlled.Accept, ctx, 1);
			AddLineAndIndentToLine(statement.EndFor, ctx);
			BundledTokenChecks(statement.EndFor, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteWhileStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			BundledTokenChecks(statement.While, ctx);
			AddSpace(statement.Condition, ctx);
			statement.Condition.Accept(this, ctx);
			AddSpace(statement.Do, ctx);
			BundledTokenChecks(statement.Do, ctx);
			ScopedIndentation(statement.Controlled.Accept, ctx, 1);
			AddLineAndIndentToLine(statement.EndWhile, ctx);
			BundledTokenChecks(statement.EndWhile, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteRepeatStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			BundledTokenChecks(statement.Repeat, ctx);
			ScopedIndentation(statement.Controlled.Accept, ctx, 1);
			AddLineAndIndentToLine(statement.Until, ctx);
			BundledTokenChecks(statement.Until, ctx);
			AddSpace(statement.Condition, ctx);
			statement.Condition.Accept(this, ctx);
			AddLineAndIndentToLine(statement.EndRepeat, ctx);
			BundledTokenChecks(statement.EndRepeat, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteTryCatchStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			BundledTokenChecks(statement.__Try, ctx);
			ScopedIndentation(statement.TrySequence.Accept, ctx, 1);
			AddLineAndIndentToLine(statement.__Catch, ctx);
			BundledTokenChecks(statement.__Catch, ctx);
			ScopedIndentation(statement.CatchSequence.Accept, ctx, 1);
			AddLineAndIndentToLine(statement.__Finally, ctx);
			BundledTokenChecks(statement.__Finally, ctx);
			if (statement.FinallySequence != null)
			{
				ScopedIndentation(statement.FinallySequence.Accept, ctx, 1);
			}
			statement.ExceptionExpression?.Accept(this, ctx);
			AddLineAndIndentToLine(statement.__EndTry, ctx);
			BundledTokenChecks(statement.__EndTry, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteErrorStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			BundledTokenChecks(statement.TokenList, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteVariableDeclarationListStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			if (statement.BeginVarOp.Operator == Operator.Union || statement.BeginVarOp.Operator == Operator.Struct)
			{
				AddLineAndIndentToLine(statement.BeginVarOp, ctx);
			}
			BundledTokenChecks(statement.BeginVarOp, ctx);
			foreach (IWhiteOperatorToken item in statement.PersistantRetain)
			{
				AddSpace(item, ctx);
			}
			BundledTokenChecks(statement.PersistantRetain, ctx);
			ScopedIndentation(statement.Declarations.Accept, ctx, 1);
			AddLineAndIndentToLine(statement.EndVarOp, ctx);
			BundledTokenChecks(statement.EndVarOp, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteVariableDeclarationStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			foreach (IWhiteExpression variableName in statement.VariableNames)
			{
				variableName.Accept(this, ctx);
			}
			AddSpace(statement.At, ctx);
			BundledTokenChecks(statement.At, ctx);
			if (statement != null && statement.At != null && statement.AddressLocation != null)
			{
				AddSpace(statement.AddressLocation, ctx);
				statement.AddressLocation.Accept(this, ctx);
			}
			AddSpace(statement.Colon, ctx);
			BundledTokenChecks(statement.Colon, ctx);
			AddSpace(statement.DeclaredType, ctx);
			statement.DeclaredType.Accept(this, ctx);
			BundledTokenChecks(statement.Assignment, ctx);
			if (statement != null && statement.Assignment != null && statement.InitializationExpression != null)
			{
				AddSpace(statement.Assignment, ctx);
				AddSpace(statement.InitializationExpression, ctx);
				statement.InitializationExpression.Accept(this, ctx);
			}
			BundledTokenChecks(statement.Semicolon, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhitePropertyDeclarationStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			BundledTokenChecks(statement.PropertyOp, ctx);
			foreach (IAccessSpecifierToken item in statement.Access)
			{
				AddSpace(item, ctx);
			}
			BundledTokenChecks(statement.Access, ctx);
			AddSpace(statement.NameExpression, ctx);
			statement.NameExpression.Accept(this, ctx);
			if (statement != null && statement.ReturnType != null && statement.Colon != null)
			{
				AddSpace(statement.Colon, ctx);
				AddSpace(statement.ReturnType, ctx);
				BundledTokenChecks(statement.Colon, ctx);
				statement.ReturnType.Accept(this, ctx);
			}
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteProgramDeclarationStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			BundledTokenChecks(statement.Class, ctx);
			AddSpace(statement.NameExpression, ctx);
			statement.NameExpression.Accept(this, ctx);
			foreach (IAccessSpecifierToken item in statement.Access)
			{
				AddSpace(item, ctx);
			}
			BundledTokenChecks(statement.Access, ctx);
			statement.Declarations.Accept(this, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteFunctionBlockDeclarationStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			BundledTokenChecks(statement.Class, ctx);
			foreach (IAccessSpecifierToken item in statement.Access)
			{
				AddSpace(item, ctx);
			}
			BundledTokenChecks(statement.Access, ctx);
			AddSpace(statement.NameExpression, ctx);
			statement.NameExpression.Accept(this, ctx);
			statement.GenericDeclarations.Accept(this, ctx);
			BundledTokenChecks(statement.ExtendsOp, ctx);
			if (statement.ExtendsOp != null)
			{
				AddSpace(statement.ExtendsOp, ctx);
				AddSpace(statement.Extends.FirstOrDefault(), ctx);
				foreach (IWhiteExpression extend in statement.Extends)
				{
					extend.Accept(this, ctx);
				}
			}
			BundledTokenChecks(statement.ImplementsOp, ctx);
			if (statement.ImplementsOp != null)
			{
				AddSpace(statement.ImplementsOp, ctx);
				AddSpace(statement.Implements.FirstOrDefault(), ctx);
				foreach (IWhiteExpression implement in statement.Implements)
				{
					implement.Accept(this, ctx);
				}
			}
			statement.Declarations.Accept(this, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteMethodDeclarationStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			BundledTokenChecks(statement.Class, ctx);
			AddSpace(statement.NameExpression, ctx);
			statement.NameExpression.Accept(this, ctx);
			foreach (IAccessSpecifierToken item in statement.Access)
			{
				AddSpace(item, ctx);
			}
			BundledTokenChecks(statement.Access, ctx);
			BundledTokenChecks(statement.Colon, ctx);
			if (statement != null && statement.ReturnType != null && statement.Colon != null)
			{
				AddSpace(statement.Colon, ctx);
				AddSpace(statement.ReturnType, ctx);
				statement.ReturnType.Accept(this, ctx);
			}
			statement.Declarations.Accept(this, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteFunctionDeclarationStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			BundledTokenChecks(statement.Class, ctx);
			AddSpace(statement.NameExpression, ctx);
			statement.NameExpression.Accept(this, ctx);
			statement.Access.ToList().ForEach(delegate(IAccessSpecifierToken e)
			{
				AddSpace(e, ctx);
			});
			BundledTokenChecks(statement.Access, ctx);
			BundledTokenChecks(statement.Colon, ctx);
			if (statement != null && statement.ReturnType != null && statement.Colon != null)
			{
				AddSpace(statement.Colon, ctx);
				AddSpace(statement.ReturnType, ctx);
				statement.ReturnType.Accept(this, ctx);
			}
			BundledTokenChecks(statement.ReturnTypeTrailingSemicolon, ctx);
			statement.Declarations.Accept(this, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteInterfaceDeclarationStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			BundledTokenChecks(statement.Class, ctx);
			AddSpace(statement.NameExpression, ctx);
			statement.NameExpression.Accept(this, ctx);
			if (statement is IWhiteInterfaceDeclarationStatement2 whiteInterfaceDeclarationStatement)
			{
				whiteInterfaceDeclarationStatement.Access.ToList().ForEach(delegate(IAccessSpecifierToken e)
				{
					AddSpace(e, ctx);
				});
				BundledTokenChecks(whiteInterfaceDeclarationStatement.Access, ctx);
			}
			BundledTokenChecks(statement.ExtendsOp, ctx);
			if (statement.ExtendsOp != null)
			{
				AddSpace(statement.ExtendsOp, ctx);
				AddSpace(statement.Extends.FirstOrDefault(), ctx);
				foreach (IWhiteExpression extend in statement.Extends)
				{
					extend.Accept(this, ctx);
				}
			}
			if (statement is IWhiteInterfaceDeclarationStatement3 whiteInterfaceDeclarationStatement2 && whiteInterfaceDeclarationStatement2.ImplementsOp != null)
			{
				AddSpace(whiteInterfaceDeclarationStatement2.ImplementsOp, ctx);
				AddSpace(whiteInterfaceDeclarationStatement2.Implements.FirstOrDefault(), ctx);
				foreach (IWhiteExpression implement in whiteInterfaceDeclarationStatement2.Implements)
				{
					implement.Accept(this, ctx);
				}
			}
			statement.Declarations.Accept(this, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteUnionDeclarationStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			BundledTokenChecks(statement.TypeOp, ctx);
			if (statement is IWhiteUnionDeclarationStatement2 whiteUnionDeclarationStatement)
			{
				whiteUnionDeclarationStatement.Access.ForEach(delegate(IAccessSpecifierToken a)
				{
					AddSpace(a, ctx);
				});
				BundledTokenChecks(whiteUnionDeclarationStatement.Access, ctx);
			}
			AddSpace(statement.NameExpression, ctx);
			statement.NameExpression.Accept(this, ctx);
			AddSpace(statement.ColonOp, ctx);
			BundledTokenChecks(statement.ColonOp, ctx);
			statement.Declaration.Accept(this, ctx);
			AddLineAndIndentToLine(statement.EndTypeOp, ctx);
			BundledTokenChecks(statement.EndTypeOp, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteEnumDeclarationStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			BundledTokenChecks(statement.TypeOp, ctx);
			if (statement is IWhiteEnumDeclarationStatement2 whiteEnumDeclarationStatement)
			{
				whiteEnumDeclarationStatement.Access.ForEach(delegate(IAccessSpecifierToken a)
				{
					AddSpace(a, ctx);
				});
				BundledTokenChecks(whiteEnumDeclarationStatement.Access, ctx);
			}
			AddSpace(statement.NameExpression, ctx);
			statement.NameExpression.Accept(this, ctx);
			AddSpace(statement.ColonOp, ctx);
			BundledTokenChecks(statement.ColonOp, ctx);
			statement.EnumerationTypeExpression.Accept(this, ctx);
			AddSpace(statement.TypeExpression, ctx);
			statement.TypeExpression?.Accept(this, ctx);
			AddSpace(statement.AssignmentOp, ctx);
			BundledTokenChecks(statement.AssignmentOp, ctx);
			AddSpace(statement.Initialization, ctx);
			statement.Initialization?.Accept(this, ctx);
			BundledTokenChecks(statement.Semicolon, ctx);
			AddLineAndIndentToLine(statement.EndTypeOp, ctx);
			BundledTokenChecks(statement.EndTypeOp, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteAliasDeclarationStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			BundledTokenChecks(statement.TypeOp, ctx);
			if (statement is IWhiteAliasDeclarationStatement2 whiteAliasDeclarationStatement)
			{
				whiteAliasDeclarationStatement.Access.ForEach(delegate(IAccessSpecifierToken a)
				{
					AddSpace(a, ctx);
				});
				BundledTokenChecks(whiteAliasDeclarationStatement.Access, ctx);
			}
			AddSpace(statement.NameExpression, ctx);
			statement.NameExpression.Accept(this, ctx);
			AddSpace(statement.ColonOp, ctx);
			BundledTokenChecks(statement.ColonOp, ctx);
			AddSpace(statement.Type, ctx);
			statement.Type.Accept(this, ctx);
			BundledTokenChecks(statement.Semicolon, ctx);
			AddSpace(statement.EndTypeOp, ctx);
			BundledTokenChecks(statement.EndTypeOp, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteStructDeclarationStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			BundledTokenChecks(statement.TypeOp, ctx);
			if (statement is IWhiteStructDeclarationStatement2 whiteStructDeclarationStatement)
			{
				whiteStructDeclarationStatement.Access.ForEach(delegate(IAccessSpecifierToken a)
				{
					AddSpace(a, ctx);
				});
				BundledTokenChecks(whiteStructDeclarationStatement.Access, ctx);
			}
			AddSpace(statement.NameExpression, ctx);
			statement.NameExpression.Accept(this, ctx);
			AddSpace(statement.ExtendsOp, ctx);
			BundledTokenChecks(statement.ExtendsOp, ctx);
			AddSpace(statement.Extends.FirstOrDefault(), ctx);
			foreach (IWhiteExpression extend in statement.Extends)
			{
				extend.Accept(this, ctx);
			}
			AddSpace(statement.ColonOp, ctx);
			BundledTokenChecks(statement.ColonOp, ctx);
			statement.Declaration.Accept(this, ctx);
			AddLineAndIndentToLine(statement.EndTypeOp, ctx);
			BundledTokenChecks(statement.EndTypeOp, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteSubrangeDeclarationStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			foreach (IWhiteExpression variableName in statement.VariableNames)
			{
				variableName.Accept(this, ctx);
			}
			AddSpace(statement.Colon, ctx);
			BundledTokenChecks(statement.Colon, ctx);
			AddSpace(statement.SubRangeType, ctx);
			statement.SubRangeType.Accept(this, ctx);
			BundledTokenChecks(statement.Semicolon, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteJumpStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			BundledTokenChecks(statement.Jump, ctx);
			statement.Condition?.Accept(this, ctx);
			AddSpace(statement.LabelToken, ctx);
			BundledTokenChecks(statement.LabelToken, ctx);
			BundledTokenChecks(statement.Semicolon, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhitePragmaStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			BundledTokenChecks(statement.PragmaToken, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteDocuCommentStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			BundledTokenChecks(statement.DocuCommentToken, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteCommentStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			BundledTokenChecks(statement.CommentToken, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteLabelStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			BundledTokenChecks(statement.LabelToken, ctx);
			BundledTokenChecks(statement.Colon, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteEmptyStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			BundledTokenChecks(statement.Semicolon, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteFinalStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			ctx.ExtendLineLength(statement.FinalToken);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteReturnStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			BundledTokenChecks(statement.Return, ctx);
			statement.Condition?.Accept(this, ctx);
			BundledTokenChecks(statement.Semicolon, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteExitStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			BundledTokenChecks(statement.Exit, ctx);
			BundledTokenChecks(statement.Semicolon, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteContinueStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			BundledTokenChecks(statement.Continue, ctx);
			BundledTokenChecks(statement.Semicolon, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteAssignmentExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			expression.LValue.Accept(this, ctx);
			AddSpace(expression.AssignmentToken, ctx);
			BundledTokenChecks(expression.AssignmentToken, ctx);
			AddSpace(expression.RValue, ctx);
			expression.RValue.Accept(this, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteBinaryOperatorExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			expression.First.Accept(this, ctx);
			AddSpace(expression.OperatorToken, ctx);
			BundledTokenChecks(expression.OperatorToken, ctx);
			AddSpace(expression.Second, ctx);
			expression.Second.Accept(this, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhitePrefixedOperatorExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.Operator, ctx);
			BundledTokenChecks(expression.LeftParenthesis, ctx);
			foreach (IWhiteExpression operand in expression.Operands)
			{
				operand.Accept(this, ctx);
			}
			BundledTokenChecks(expression.RightParenthesis, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteUnaryOperatorExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.OperatorToken, ctx);
			if (expression.KindOf == Operator.Not)
			{
				AddSpace(expression.Operand, ctx);
			}
			expression.Operand.Accept(this, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteVariableExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.IdentToken, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteCallExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			expression.Callee.Accept(this, ctx);
			BundledTokenChecks(expression.LeftParenthesis, ctx);
			int num = expression.Params.Count();
			int num2 = expression.Params.Aggregate(0, (int lenSum, IWhiteExpression param) => lenSum + TextLengthCalculator.CalculateTextLength(param));
			bool flag = expression.Params.SelectMany(TokenSerializer.GetTokenList).Any((IWhiteToken t) => t is IEndOfLineToken);
			bool flag2 = false;
			foreach (IWhiteExpression param in expression.Params)
			{
				if (flag || num >= _settings.MaxCalleeParamsBeforeBreak || num2 >= _settings.MaxCalleeParamsCharCountBeforeBreak)
				{
					flag2 = true;
					if (param is ILeadByCommaExpression leadByCommaExpression)
					{
						AddLineAndIndentToLine(leadByCommaExpression.Expression, ctx, 1);
					}
					else
					{
						AddLineAndIndentToLine(param, ctx, 1);
					}
					ctx.Indent();
					param.Accept(this, ctx);
					ctx.UnIndent();
				}
				else
				{
					param.Accept(this, ctx);
				}
			}
			if (flag2)
			{
				AddLineAndIndentToLine(expression.RightParenthesis, ctx);
			}
			ctx.PopCallee();
			BundledTokenChecks(expression.RightParenthesis, ctx);
			return true;
		}

		public bool visit(ILeadByCommaExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.Comma, ctx);
			IWhiteToken firstToken = WhiteSpaceFormatter.GetFirstToken(expression.Expression);
			if (firstToken != null && !WhiteSpaceFormatter.TokenLeadingContains(firstToken, typeof(IEndOfLineToken)))
			{
				AddSpace(expression.Expression, ctx);
			}
			if (expression.Expression.GetLeadingToken() is ICommentToken)
			{
				AddLine(expression.Expression, ctx);
				AddCurrentIndentToLine(expression.Expression, ctx, 1);
			}
			expression.Expression.Accept(this, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteRangeExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			expression.Low.Accept(this, ctx);
			BundledTokenChecks(expression.Range, ctx);
			expression.High.Accept(this, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteIntegerLiteralExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.Integer, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteRealLiteralExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.Real, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteBoolLiteralExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.Boolean, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteDurationLiteralExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.Duration, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteMultipleIndexInitialization expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			expression.Number.Accept(this, ctx);
			expression.Value.Accept(this, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteDateLiteralExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.DateToken, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteLDateLiteralExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.LDateToken, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteLTimeOfDayLiteralExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.LTimeOfDayToken, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteLDateAndTimeLiteralExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.LDateAndTimeToken, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteDateAndTimeLiteralExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.DateAndTimeToken, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteTimeOfDayLiteralExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.TimeOfDayToken, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteLDurationLiteralExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.LDurationToken, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteSingleByteStringLiteralExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.StringToken, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteDoubleByteStringLiteralExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.StringToken, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteXStringLiteralExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.StringToken, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteDirectVariableExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.DirectVariableToken, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteIncompleteDirectVariableExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.DirectVariableToken, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteDeRefAccessExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			expression.BaseExpression.Accept(this, ctx);
			BundledTokenChecks(expression.DeRefToken, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteCompoAccessExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			expression.LeftExpression.Accept(this, ctx);
			BundledTokenChecks(expression.Period, ctx);
			expression.RightExpression.Accept(this, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteBitAccessExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			expression.LeftExpression.Accept(this, ctx);
			BundledTokenChecks(expression.Period, ctx);
			expression.RightExpression.Accept(this, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteIndexAccessExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			expression.Base.Accept(this, ctx);
			BundledTokenChecks(expression.LeftBracket, ctx);
			foreach (IWhiteExpression access in expression.Accesses)
			{
				access.Accept(this, ctx);
			}
			BundledTokenChecks(expression.RightBracket, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IParenthesizedExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.LeftParenthesis, ctx);
			expression.Expression.Accept(this, ctx);
			BundledTokenChecks(expression.RightParenthesis, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteThisExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.This, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteSuperExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.Super, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteNamespaceAccessExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			expression.Namespace.Accept(this, ctx);
			BundledTokenChecks(expression.HashTag, ctx);
			expression.VariableExpression.Accept(this, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteSpecialScopeExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.Scope, ctx);
			BundledTokenChecks(expression.Period, ctx);
			expression.Right.Accept(this, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteGlobalScopeExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.Period, ctx);
			expression.Right.Accept(this, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteConversionExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.ConversionOperator, ctx);
			BundledTokenChecks(expression.LeftParenthesis, ctx);
			expression.Expression.Accept(this, ctx);
			BundledTokenChecks(expression.RightParenthesis, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteTypeExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			ctx.ExtendLineLength(expression.Class.ToString().Length);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteSimpleTypeExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.SimpleTypeOperator, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteUserDefTypeExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			expression.NameExpression.Accept(this, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhitePointerTypeExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.Pointer, ctx);
			AddSpace(expression.To, ctx);
			BundledTokenChecks(expression.To, ctx);
			AddSpace(expression.BaseType, ctx);
			expression.BaseType.Accept(this, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteReferenceTypeExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.Reference, ctx);
			AddSpace(expression.To, ctx);
			BundledTokenChecks(expression.To, ctx);
			AddSpace(expression.BaseType, ctx);
			expression.BaseType.Accept(this, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteArrayInitializationExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.LeftBracket, ctx);
			foreach (IWhiteExpression initValue in expression.InitValues)
			{
				initValue.Accept(this, ctx);
			}
			BundledTokenChecks(expression.RightBracket, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteArrayTypeExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.Array, ctx);
			BundledTokenChecks(expression.LeftBracket, ctx);
			foreach (IWhiteExpression range in expression.Ranges)
			{
				range.Accept(this, ctx);
			}
			BundledTokenChecks(expression.RightBracket, ctx);
			AddSpace(expression.Of, ctx);
			BundledTokenChecks(expression.Of, ctx);
			AddSpace(expression.BaseType, ctx);
			expression.BaseType.Accept(this, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(ISubRangeTypeExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			expression.BaseType.Accept(this, ctx);
			BundledTokenChecks(expression.LeftParenthesis, ctx);
			expression.Range.Accept(this, ctx);
			BundledTokenChecks(expression.RightParenthesis, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IStringTypeExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.StringSimpleOperator, ctx);
			BundledTokenChecks(expression.LeftParenthesis, ctx);
			expression.Length?.Accept(this, ctx);
			BundledTokenChecks(expression.RightParenthesis, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IEnumerationTypeExpression expression, LineCtx ctx)
		{
			AddLineAndIndentToLine(expression.LeftParenthesis, ctx);
			BundledTokenChecks(expression.LeftParenthesis, ctx);
			ctx.PushCallee(expression);
			foreach (IWhiteExpression definition in expression.Definitions)
			{
				if (definition is ILeadByCommaExpression leadByCommaExpression)
				{
					AddLineAndIndentToLine(leadByCommaExpression.Expression, ctx, 1);
				}
				else
				{
					AddLineAndIndentToLine(definition, ctx, 1);
				}
				definition.Accept(this, ctx);
			}
			ctx.PopCallee();
			AddLineAndIndentToLine(expression.RightParenthesis, ctx);
			BundledTokenChecks(expression.RightParenthesis, ctx);
			return true;
		}

		public bool visit(IVectorTypeExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.Vector, ctx);
			BundledTokenChecks(expression.LeftBracket, ctx);
			expression.Length.Accept(this, ctx);
			BundledTokenChecks(expression.RightBracket, ctx);
			AddSpace(expression.Of, ctx);
			BundledTokenChecks(expression.Of, ctx);
			AddSpace(expression.BaseType, ctx);
			expression.BaseType.Accept(this, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteSizeOfExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.SizeOf, ctx);
			BundledTokenChecks(expression.LeftParenthesis, ctx);
			expression.TypeExpression.Accept(this, ctx);
			BundledTokenChecks(expression.RightParenthesis, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IEmptyExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteStructureInitialization expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.LeftParenthesis, ctx);
			foreach (IWhiteExpression compoInit in expression.CompoInits)
			{
				compoInit.Accept(this, ctx);
			}
			BundledTokenChecks(expression.RightParenthesis, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhitePragmaIfStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			AddLineAndIndentToLine(statement.If, ctx);
			ScopedIndentation(statement.ThenStatement.Accept, ctx, 1);
			AddLineAndIndentToLine(statement.Else, ctx);
			if (statement.ElseStatement != null)
			{
				ScopedIndentation(statement.ElseStatement.Accept, ctx, 1);
			}
			foreach (IWhitePragmaElseIfStatement item in statement.ElseIfStatement)
			{
				item.Accept(this, ctx);
			}
			AddLineAndIndentToLine(statement.EndIf, ctx);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhitePragmaElseIfStatement statement, LineCtx ctx)
		{
			ctx.PushCallee(statement);
			AddLineAndIndentToLine(statement.ElseIf, ctx);
			ScopedIndentation(statement.ThenStatement.Accept, ctx, 1);
			ctx.PopCallee();
			return true;
		}

		public bool visit(IWhiteNamespaceDeclarationStatement statement, LineCtx context)
		{
			context.PushCallee(statement);
			BundledTokenChecks(statement.Class, context);
			AddSpace(statement.NameExpression, context);
			statement.NameExpression.Accept(this, context);
			foreach (IAccessSpecifierToken item in statement.Access)
			{
				AddSpace(item, context);
			}
			BundledTokenChecks(statement.Access, context);
			ScopedIndentation(statement.Declarations.Accept, context, 1);
			foreach (IWhitePOUSyntax subPOU in statement.SubPOUs)
			{
				ScopedIndentation(subPOU.Accept, context, 1);
			}
			context.PopCallee();
			return true;
		}

		public bool visit(IWhitePropertyAccessorDeclarationStatement statement, LineCtx context)
		{
			context.PushCallee(statement);
			BundledTokenChecks(statement.Class, context);
			AddSpace(statement.NameExpression, context);
			statement.NameExpression.Accept(this, context);
			foreach (IAccessSpecifierToken item in statement.Access)
			{
				AddSpace(item, context);
			}
			BundledTokenChecks(statement.Access, context);
			statement.Declarations.Accept(this, context);
			context.PopCallee();
			return true;
		}

		public bool visit(IWhitePropertyAccessorStatement statement, LineCtx context)
		{
			context.PushCallee(statement);
			statement.BeforeDeclarationStatements.Accept(this, context);
			statement.DeclarationStatement.Accept(this, context);
			ScopedIndentation(statement.Implementation.Accept, context, 1);
			BundledTokenChecks(statement.EndProperty, context);
			context.PopCallee();
			return true;
		}

		public bool visit(IWhiteActionDeclarationStatement statement, LineCtx context)
		{
			return true;
		}

		public bool visit(IWhiteTransitionDeclarationStatement statement, LineCtx context)
		{
			return true;
		}

		private bool visitPOU(IWhitePOU statement, LineCtx context)
		{
			context.PushCallee(statement);
			statement.BeforeDeclarationStatements.Accept(this, context);
			statement.DeclarationStatement.Accept(this, context);
			statement.Implementation.Accept(this, context);
			foreach (IWhitePOUSyntax subPOU in statement.SubPOUs)
			{
				subPOU.Accept(this, context);
			}
			BundledTokenChecks(statement.EndPOU, context);
			return true;
		}

		public bool visit(IWhiteFunction statement, LineCtx context)
		{
			return visitPOU(statement, context);
		}

		public bool visit(IWhiteFunctionBlock statement, LineCtx context)
		{
			return visitPOU(statement, context);
		}

		public bool visit(IWhiteProgram statement, LineCtx context)
		{
			return visitPOU(statement, context);
		}

		public bool visit(IWhiteInterface statement, LineCtx context)
		{
			return visitPOU(statement, context);
		}

		public bool visit(IWhiteMethod statement, LineCtx context)
		{
			return visitPOU(statement, context);
		}

		public bool visit(IWhiteAction statement, LineCtx context)
		{
			return visitPOU(statement, context);
		}

		public bool visit(IWhiteTransition statement, LineCtx context)
		{
			return visitPOU(statement, context);
		}

		public bool visit(IWhiteDUT statement, LineCtx context)
		{
			context.PushCallee(statement);
			statement.BeforeDeclarationStatements.Accept(this, context);
			statement.TypeDeclarationStatement.Accept(this, context);
			context.PopCallee();
			return true;
		}

		public bool visit(IWhiteErrorPOU statement, LineCtx context)
		{
			return statement.ErrorStatement.Accept(this, context);
		}

		public bool visit(IWhitePartialAccessExpression expression, LineCtx context)
		{
			BundledTokenChecks(expression.PartialAccessToken, context);
			return true;
		}

		public bool visit(IWhiteCompoPartialAccessExpression expression, LineCtx context)
		{
			context.PushCallee(expression);
			expression.LeftExpression.Accept(this, context);
			BundledTokenChecks(expression.Period, context);
			expression.RightExpression.Accept(this, context);
			context.PopCallee();
			return true;
		}

		public bool visit(IWhiteVariableArrayTypeExpression expression, LineCtx ctx)
		{
			ctx.PushCallee(expression);
			BundledTokenChecks(expression.Array, ctx);
			BundledTokenChecks(expression.LeftBracket, ctx);
			BundledTokenChecks(expression.TimesOp, ctx);
			BundledTokenChecks(expression.RightBracket, ctx);
			AddSpace(expression.Of, ctx);
			BundledTokenChecks(expression.Of, ctx);
			AddSpace(expression.BaseType, ctx);
			expression.BaseType.Accept(this, ctx);
			ctx.PopCallee();
			return true;
		}
	}
}

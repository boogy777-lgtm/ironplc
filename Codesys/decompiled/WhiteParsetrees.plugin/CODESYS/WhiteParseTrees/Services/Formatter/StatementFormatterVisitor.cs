using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CODESYS.WhiteParseTrees.Services.Formatter.Passes;

namespace CODESYS.WhiteParseTrees.Services.Formatter
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public abstract class StatementFormatterVisitor : IStatementSyntax2.IStatementVisitor2<bool, int>, IStatementSyntax.IStatementVisitor<bool, int>, IStatementFormattingPass
	{
		protected readonly IFormatterSettings _settings;

		[System.Runtime.CompilerServices.Nullable(2)]
		private readonly IExpressionFormattingPass _expressionFormatter;

		private readonly bool _isEnabled;

		public abstract void Perform(IWhiteSequenceStatement sequenceStatement);

		private void FormatExpr(IWhiteExpression expression, int indentation)
		{
			if (_isEnabled && _expressionFormatter != null)
			{
				_expressionFormatter.Format(expression, _settings, indentation);
			}
		}

		private void FormatExpr(IEnumerable<IWhiteExpression> expressions, int indentation)
		{
			foreach (IWhiteExpression expression in expressions)
			{
				FormatExpr(expression, indentation);
			}
		}

		private bool visitPOU(IWhitePOU statement, int indentation)
		{
			statement.DeclarationStatement.Accept(this, indentation);
			statement.Implementation.Accept(this, indentation);
			foreach (IWhitePOUSyntax subPOU in statement.SubPOUs)
			{
				subPOU.Accept(this, indentation);
			}
			return true;
		}

		protected StatementFormatterVisitor(IFormatterSettings settings, [System.Runtime.CompilerServices.Nullable(2)] IExpressionFormattingPass expressionFormatter)
		{
			_isEnabled = true;
			_settings = settings;
			_expressionFormatter = expressionFormatter;
		}

		[System.Runtime.CompilerServices.NullableContext(2)]
		protected void AddSpace(INode node)
		{
			if (_isEnabled)
			{
				FormatUtil.CheckedAddSpace(node);
			}
		}

		[System.Runtime.CompilerServices.NullableContext(2)]
		protected void AddIndent(INode node, int nNumOfTabs)
		{
			if (_isEnabled)
			{
				FormatUtil.CheckedAddIndent(node, nNumOfTabs);
			}
		}

		[System.Runtime.CompilerServices.NullableContext(2)]
		protected void AddLineAndIndent(INode node, int nNumOfTabs)
		{
			if (_isEnabled)
			{
				FormatUtil.CheckedAddLineAndIndent(node, nNumOfTabs);
			}
		}

		[System.Runtime.CompilerServices.NullableContext(2)]
		protected void AddLine(INode node)
		{
			if (_isEnabled)
			{
				FormatUtil.CheckedAddLine(node);
			}
		}

		public virtual bool visit(IWhiteSequenceStatement statement, int indentation)
		{
			foreach (IWhiteStatement item in statement)
			{
				item.Accept(this, indentation);
			}
			return true;
		}

		public virtual bool visit(IWhiteExpressionStatement statement, int indentation)
		{
			FormatExpr(statement.Expr, indentation);
			return true;
		}

		public virtual bool visit(IWhiteElseIfStatement statement, int indentation)
		{
			FormatExpr(statement.Condition, indentation);
			statement.ThenStatement.Accept(this, indentation + 1);
			return true;
		}

		public virtual bool visit(IWhiteIfStatement statement, int indentation)
		{
			statement.ThenStatement.Accept(this, indentation + 1);
			FormatExpr(statement.Condition, indentation);
			statement.ElseStatement?.Accept(this, indentation + 1);
			foreach (IWhiteElseIfStatement item in statement.ElseIfStatement)
			{
				item.Accept(this, indentation);
			}
			return true;
		}

		public virtual bool visit(IWhitePragmaIfStatement statement, int indentation)
		{
			statement.ThenStatement.Accept(this, indentation + 1);
			statement.ElseStatement?.Accept(this, indentation + 1);
			foreach (IWhitePragmaElseIfStatement item in statement.ElseIfStatement)
			{
				item.Accept(this, indentation);
			}
			return true;
		}

		public virtual bool visit(IWhitePragmaElseIfStatement statement, int indentation)
		{
			statement.ThenStatement.Accept(this, indentation + 1);
			return true;
		}

		public virtual bool visit(IWhiteCaseStatement statement, int indentation)
		{
			foreach (IWhiteCase @case in statement.Cases)
			{
				@case.Label.Accept(this, indentation + 1);
				@case.Controlled.Accept(this, indentation + 2);
			}
			statement.Else?.Accept(this, indentation + 2);
			return true;
		}

		public virtual bool visit(IWhiteCaseLabelStatement statement, int indentation)
		{
			FormatExpr(statement.CaseExpressionList, indentation);
			return true;
		}

		public virtual bool visit(IWhiteForStatement statement, int indentation)
		{
			FormatExpr(statement.StartExpression, indentation);
			if (statement.StepWidth != null)
			{
				FormatExpr(statement.StepWidth, indentation);
			}
			FormatExpr(statement.UpperBound, indentation);
			statement.Controlled.Accept(this, indentation + 1);
			return true;
		}

		public virtual bool visit(IWhiteWhileStatement statement, int indentation)
		{
			FormatExpr(statement.Condition, indentation);
			statement.Controlled.Accept(this, indentation + 1);
			return true;
		}

		public virtual bool visit(IWhiteRepeatStatement statement, int indentation)
		{
			statement.Controlled.Accept(this, indentation + 1);
			FormatExpr(statement.Condition, indentation);
			return true;
		}

		public virtual bool visit(IWhiteTryCatchStatement statement, int indentation)
		{
			statement.TrySequence.Accept(this, indentation + 1);
			statement.CatchSequence.Accept(this, indentation + 1);
			statement.FinallySequence?.Accept(this, indentation + 1);
			if (statement.ExceptionExpression != null)
			{
				FormatExpr(statement.ExceptionExpression, indentation);
			}
			return true;
		}

		public virtual bool visit(IWhiteErrorStatement statement, int indentation)
		{
			return true;
		}

		public virtual bool visit(IWhiteVariableDeclarationListStatement statement, int indentation)
		{
			statement.Declarations.Accept(this, indentation + 1);
			return true;
		}

		public virtual bool visit(IWhiteVariableDeclarationStatement statement, int indentation)
		{
			FormatExpr(statement.VariableNames, indentation);
			if (statement.At != null && statement.AddressLocation != null)
			{
				FormatExpr(statement.AddressLocation, indentation);
			}
			FormatExpr(statement.DeclaredType, indentation);
			if (statement.Assignment != null && statement.InitializationExpression != null)
			{
				FormatExpr(statement.InitializationExpression, indentation);
			}
			return true;
		}

		public virtual bool visit(IWhitePropertyDeclarationStatement statement, int indentation)
		{
			if (statement.ReturnType != null && statement.Colon != null)
			{
				FormatExpr(statement.ReturnType, indentation);
			}
			return true;
		}

		public virtual bool visit(IWhiteProgramDeclarationStatement statement, int indentation)
		{
			FormatExpr(statement.NameExpression, indentation);
			statement.Declarations.Accept(this, indentation);
			return true;
		}

		public virtual bool visit(IWhiteFunctionBlockDeclarationStatement statement, int indentation)
		{
			FormatExpr(statement.NameExpression, indentation);
			if (statement.ExtendsOp != null)
			{
				FormatExpr(statement.Extends, indentation);
			}
			if (statement.ImplementsOp != null)
			{
				FormatExpr(statement.Implements, indentation);
			}
			statement.Declarations.Accept(this, indentation);
			return true;
		}

		public virtual bool visit(IWhiteMethodDeclarationStatement statement, int indentation)
		{
			FormatExpr(statement.NameExpression, indentation);
			if (statement.ReturnType != null && statement.Colon != null)
			{
				FormatExpr(statement.ReturnType, indentation);
			}
			statement.Declarations.Accept(this, indentation);
			return true;
		}

		public virtual bool visit(IWhiteFunctionDeclarationStatement statement, int indentation)
		{
			FormatExpr(statement.NameExpression, indentation);
			if (statement.ReturnType != null && statement.Colon != null)
			{
				FormatExpr(statement.ReturnType, indentation);
			}
			statement.Declarations.Accept(this, indentation);
			return true;
		}

		public virtual bool visit(IWhiteInterfaceDeclarationStatement statement, int indentation)
		{
			FormatExpr(statement.NameExpression, indentation);
			if (statement.ExtendsOp != null)
			{
				FormatExpr(statement.Extends, indentation);
			}
			statement.Declarations.Accept(this, indentation);
			return true;
		}

		public virtual bool visit(IWhiteUnionDeclarationStatement statement, int indentation)
		{
			statement.Declaration.Accept(this, indentation);
			return true;
		}

		public virtual bool visit(IWhiteEnumDeclarationStatement statement, int indentation)
		{
			FormatExpr(statement.NameExpression, indentation);
			FormatExpr(statement.EnumerationTypeExpression, indentation);
			return true;
		}

		public virtual bool visit(IWhiteAliasDeclarationStatement statement, int indentation)
		{
			FormatExpr(statement.NameExpression, indentation);
			FormatExpr(statement.Type, indentation);
			return true;
		}

		public virtual bool visit(IWhiteStructDeclarationStatement statement, int indentation)
		{
			statement.Declaration.Accept(this, indentation);
			FormatExpr(statement.Extends, indentation);
			FormatExpr(statement.NameExpression, indentation);
			return true;
		}

		public virtual bool visit(IWhiteSubrangeDeclarationStatement statement, int indentation)
		{
			FormatExpr(statement.VariableNames, indentation);
			FormatExpr(statement.SubRangeType, indentation);
			return true;
		}

		public virtual bool visit(IWhiteJumpStatement statement, int indentation)
		{
			return true;
		}

		public virtual bool visit(IWhitePragmaStatement statement, int indentation)
		{
			return true;
		}

		public virtual bool visit(IWhiteDocuCommentStatement statement, int indentation)
		{
			return true;
		}

		public virtual bool visit(IWhiteCommentStatement statement, int indentation)
		{
			return true;
		}

		public virtual bool visit(IWhiteLabelStatement statement, int indentation)
		{
			return true;
		}

		public virtual bool visit(IWhiteEmptyStatement statement, int indentation)
		{
			return true;
		}

		public virtual bool visit(IWhiteFinalStatement statement, int indentation)
		{
			return true;
		}

		public virtual bool visit(IWhiteReturnStatement statement, int indentation)
		{
			return true;
		}

		public virtual bool visit(IWhiteExitStatement statement, int indentation)
		{
			return true;
		}

		public virtual bool visit(IWhiteContinueStatement statement, int indentation)
		{
			return true;
		}

		public virtual bool visit(IWhiteNamespaceDeclarationStatement statement, int indentation)
		{
			return visitPOU(statement, indentation);
		}

		public virtual bool visit(IWhitePropertyAccessorDeclarationStatement statement, int indentation)
		{
			return visit(statement.Declarations, indentation);
		}

		public virtual bool visit(IWhitePropertyAccessorStatement statement, int indentation)
		{
			return visitPOU(statement, indentation);
		}

		public virtual bool visit(IWhiteActionDeclarationStatement statement, int indentation)
		{
			return visit(statement.Declarations, indentation);
		}

		public virtual bool visit(IWhiteTransitionDeclarationStatement statement, int indentation)
		{
			return visit(statement.Declarations, indentation);
		}

		public virtual bool visit(IWhiteFunction statement, int indentation)
		{
			return visitPOU(statement, indentation);
		}

		public virtual bool visit(IWhiteFunctionBlock statement, int indentation)
		{
			return visitPOU(statement, indentation);
		}

		public virtual bool visit(IWhiteProgram statement, int indentation)
		{
			return visitPOU(statement, indentation);
		}

		public virtual bool visit(IWhiteInterface statement, int indentation)
		{
			return visitPOU(statement, indentation);
		}

		public virtual bool visit(IWhiteErrorPOU statement, int indentation)
		{
			return visit(statement.ErrorStatement, indentation);
		}

		public virtual bool visit(IWhiteMethod statement, int indentation)
		{
			return visitPOU(statement, indentation);
		}

		public virtual bool visit(IWhiteAction statement, int indentation)
		{
			return visitPOU(statement, indentation);
		}

		public virtual bool visit(IWhiteTransition statement, int indentation)
		{
			return visitPOU(statement, indentation);
		}

		public virtual bool visit(IWhiteDUT statement, int indentation)
		{
			return visitPOU(statement, indentation);
		}
	}
}

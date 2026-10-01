using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Services.Formatter
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class ErrorStatementVisitor : IStatementSyntax2.IStatementVisitor2<bool>, IStatementSyntax.IStatementVisitor<bool>
	{
		public static bool ContainsErrorStatement(IWhiteSequenceStatement statement)
		{
			return statement.Accept(new ErrorStatementVisitor());
		}

		[System.Runtime.CompilerServices.NullableContext(2)]
		private bool CheckSequence(IWhiteSequenceStatement statement)
		{
			if (statement == null)
			{
				return false;
			}
			foreach (IWhiteStatement item in statement)
			{
				if (item.Accept(this))
				{
					return true;
				}
			}
			return false;
		}

		private bool VisitPOU(IWhitePOU statement)
		{
			if (statement.DeclarationStatement.Accept(this))
			{
				return true;
			}
			if (statement.Implementation.Accept(this))
			{
				return true;
			}
			foreach (IWhitePOUSyntax subPOU in statement.SubPOUs)
			{
				if (subPOU.Accept(this))
				{
					return true;
				}
			}
			return false;
		}

		public bool visit(IWhiteSequenceStatement statement)
		{
			foreach (IWhiteStatement item in statement)
			{
				if (item.Accept(this))
				{
					return true;
				}
			}
			return false;
		}

		public bool visit(IWhiteExpressionStatement statement)
		{
			return false;
		}

		public bool visit(IWhiteElseIfStatement statement)
		{
			return CheckSequence(statement.ThenStatement);
		}

		public bool visit(IWhiteIfStatement statement)
		{
			if (CheckSequence(statement.ThenStatement))
			{
				return true;
			}
			if (CheckSequence(statement.ElseStatement))
			{
				return true;
			}
			foreach (IWhiteElseIfStatement item in statement.ElseIfStatement)
			{
				if (item.Accept(this))
				{
					return true;
				}
			}
			return false;
		}

		public bool visit(IWhitePragmaIfStatement statement)
		{
			if (CheckSequence(statement.ThenStatement))
			{
				return true;
			}
			if (CheckSequence(statement.ElseStatement))
			{
				return true;
			}
			foreach (IWhitePragmaElseIfStatement item in statement.ElseIfStatement)
			{
				if (item.Accept(this))
				{
					return true;
				}
			}
			return false;
		}

		public bool visit(IWhitePragmaElseIfStatement statement)
		{
			return CheckSequence(statement.ThenStatement);
		}

		public bool visit(IWhiteErrorStatement statement)
		{
			return true;
		}

		public bool visit(IWhiteDocuCommentStatement statement)
		{
			return false;
		}

		public bool visit(IWhiteCommentStatement statement)
		{
			return false;
		}

		public bool visit(IWhitePragmaStatement statement)
		{
			return false;
		}

		public bool visit(IWhiteLabelStatement statement)
		{
			return false;
		}

		public bool visit(IWhiteEmptyStatement statement)
		{
			return false;
		}

		public bool visit(IWhiteFinalStatement statement)
		{
			return false;
		}

		public bool visit(IWhiteReturnStatement statement)
		{
			return false;
		}

		public bool visit(IWhiteJumpStatement statement)
		{
			return false;
		}

		public bool visit(IWhiteExitStatement statement)
		{
			return false;
		}

		public bool visit(IWhiteContinueStatement statement)
		{
			return false;
		}

		public bool visit(IWhiteCaseStatement statement)
		{
			return false;
		}

		public bool visit(IWhiteCaseLabelStatement statement)
		{
			return false;
		}

		public bool visit(IWhiteForStatement statement)
		{
			return statement.Controlled.Accept(this);
		}

		public bool visit(IWhiteWhileStatement statement)
		{
			return statement.Controlled.Accept(this);
		}

		public bool visit(IWhiteRepeatStatement statement)
		{
			return statement.Controlled.Accept(this);
		}

		public bool visit(IWhiteTryCatchStatement statement)
		{
			if (CheckSequence(statement.TrySequence))
			{
				return true;
			}
			if (CheckSequence(statement.CatchSequence))
			{
				return true;
			}
			if (CheckSequence(statement.FinallySequence))
			{
				return true;
			}
			return false;
		}

		public bool visit(IWhiteVariableDeclarationListStatement statement)
		{
			if (CheckSequence(statement.Declarations))
			{
				return true;
			}
			return false;
		}

		public bool visit(IWhiteVariableDeclarationStatement statement)
		{
			return false;
		}

		public bool visit(IWhitePropertyDeclarationStatement statement)
		{
			return false;
		}

		public bool visit(IWhiteProgramDeclarationStatement statement)
		{
			if (CheckSequence(statement.Declarations))
			{
				return true;
			}
			return false;
		}

		public bool visit(IWhiteFunctionBlockDeclarationStatement statement)
		{
			if (CheckSequence(statement.GenericDeclarations))
			{
				return true;
			}
			if (CheckSequence(statement.Declarations))
			{
				return true;
			}
			return false;
		}

		public bool visit(IWhiteMethodDeclarationStatement statement)
		{
			if (CheckSequence(statement.Declarations))
			{
				return true;
			}
			return false;
		}

		public bool visit(IWhiteInterfaceDeclarationStatement statement)
		{
			if (CheckSequence(statement.Declarations))
			{
				return true;
			}
			return false;
		}

		public bool visit(IWhiteFunctionDeclarationStatement statement)
		{
			if (CheckSequence(statement.Declarations))
			{
				return true;
			}
			return false;
		}

		public bool visit(IWhiteUnionDeclarationStatement statement)
		{
			return statement.Declaration.Accept(this);
		}

		public bool visit(IWhiteEnumDeclarationStatement statement)
		{
			return false;
		}

		public bool visit(IWhiteAliasDeclarationStatement statement)
		{
			return false;
		}

		public bool visit(IWhiteStructDeclarationStatement statement)
		{
			return statement.Declaration.Accept(this);
		}

		public bool visit(IWhiteSubrangeDeclarationStatement statement)
		{
			return false;
		}

		public bool visit(IWhiteNamespaceDeclarationStatement statement)
		{
			return statement.Declarations.Accept(this);
		}

		public bool visit(IWhitePropertyAccessorDeclarationStatement statement)
		{
			return statement.Declarations.Accept(this);
		}

		public bool visit(IWhitePropertyAccessorStatement statement)
		{
			return VisitPOU(statement);
		}

		public bool visit(IWhiteActionDeclarationStatement statement)
		{
			return statement.Declarations.Accept(this);
		}

		public bool visit(IWhiteTransitionDeclarationStatement statement)
		{
			return statement.Declarations.Accept(this);
		}

		public bool visit(IWhiteFunction statement)
		{
			return VisitPOU(statement);
		}

		public bool visit(IWhiteFunctionBlock statement)
		{
			return VisitPOU(statement);
		}

		public bool visit(IWhiteProgram statement)
		{
			return VisitPOU(statement);
		}

		public bool visit(IWhiteInterface statement)
		{
			return VisitPOU(statement);
		}

		public bool visit(IWhiteErrorPOU statement)
		{
			return true;
		}

		public bool visit(IWhiteMethod statement)
		{
			return VisitPOU(statement);
		}

		public bool visit(IWhiteAction statement)
		{
			return VisitPOU(statement);
		}

		public bool visit(IWhiteTransition statement)
		{
			return VisitPOU(statement);
		}

		public bool visit(IWhiteDUT statement)
		{
			return VisitPOU(statement);
		}
	}
}

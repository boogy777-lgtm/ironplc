using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IStatementSyntax
	{
		public interface IStatementVisitor
		{
			void visit(IWhiteSequenceStatement statement);

			void visit(IWhiteExpressionStatement statement);

			void visit(IWhiteElseIfStatement statement);

			void visit(IWhiteIfStatement statement);

			void visit(IWhiteErrorStatement statement);

			void visit(IWhiteDocuCommentStatement statement);

			void visit(IWhiteCommentStatement statement);

			void visit(IWhitePragmaStatement statement);

			void visit(IWhiteLabelStatement statement);

			void visit(IWhiteEmptyStatement statement);

			void visit(IWhiteFinalStatement statement);

			void visit(IWhiteReturnStatement statement);

			void visit(IWhiteJumpStatement statement);

			void visit(IWhiteExitStatement statement);

			void visit(IWhiteContinueStatement statement);

			void visit(IWhiteCaseStatement statement);

			void visit(IWhiteCaseLabelStatement statement);

			void visit(IWhiteForStatement statement);

			void visit(IWhiteWhileStatement statement);

			void visit(IWhiteRepeatStatement statement);

			void visit(IWhiteTryCatchStatement statement);

			void visit(IWhiteVariableDeclarationListStatement statement);

			void visit(IWhiteVariableDeclarationStatement statement);

			void visit(IWhitePropertyDeclarationStatement statement);

			void visit(IWhiteProgramDeclarationStatement statement);

			void visit(IWhiteFunctionBlockDeclarationStatement statement);

			void visit(IWhiteMethodDeclarationStatement statement);

			void visit(IWhiteInterfaceDeclarationStatement statement);

			void visit(IWhiteFunctionDeclarationStatement statement);

			void visit(IWhiteUnionDeclarationStatement statement);

			void visit(IWhiteEnumDeclarationStatement statement);

			void visit(IWhiteAliasDeclarationStatement statement);

			void visit(IWhiteStructDeclarationStatement statement);

			void visit(IWhiteSubrangeDeclarationStatement statement);

			void visit(IWhitePragmaIfStatement statement);

			void visit(IWhitePragmaElseIfStatement statement);
		}

		public interface IStatementVisitor<[Nullable(2)] out T>
		{
			T visit(IWhiteSequenceStatement statement);

			T visit(IWhiteExpressionStatement statement);

			T visit(IWhiteElseIfStatement statement);

			T visit(IWhiteIfStatement statement);

			T visit(IWhiteErrorStatement statement);

			T visit(IWhiteDocuCommentStatement statement);

			T visit(IWhiteCommentStatement statement);

			T visit(IWhitePragmaStatement statement);

			T visit(IWhiteLabelStatement statement);

			T visit(IWhiteEmptyStatement statement);

			T visit(IWhiteFinalStatement statement);

			T visit(IWhiteReturnStatement statement);

			T visit(IWhiteJumpStatement statement);

			T visit(IWhiteExitStatement statement);

			T visit(IWhiteContinueStatement statement);

			T visit(IWhiteCaseStatement statement);

			T visit(IWhiteCaseLabelStatement statement);

			T visit(IWhiteForStatement statement);

			T visit(IWhiteWhileStatement statement);

			T visit(IWhiteRepeatStatement statement);

			T visit(IWhiteTryCatchStatement statement);

			T visit(IWhiteVariableDeclarationListStatement statement);

			T visit(IWhiteVariableDeclarationStatement statement);

			T visit(IWhitePropertyDeclarationStatement statement);

			T visit(IWhiteProgramDeclarationStatement statement);

			T visit(IWhiteFunctionBlockDeclarationStatement statement);

			T visit(IWhiteMethodDeclarationStatement statement);

			T visit(IWhiteInterfaceDeclarationStatement statement);

			T visit(IWhiteFunctionDeclarationStatement statement);

			T visit(IWhiteUnionDeclarationStatement statement);

			T visit(IWhiteEnumDeclarationStatement statement);

			T visit(IWhiteAliasDeclarationStatement statement);

			T visit(IWhiteStructDeclarationStatement statement);

			T visit(IWhiteSubrangeDeclarationStatement statement);

			T visit(IWhitePragmaIfStatement statement);

			T visit(IWhitePragmaElseIfStatement statement);
		}

		public interface IStatementVisitor<[Nullable(2)] out T, [Nullable(2)] in TContext>
		{
			T visit(IWhiteSequenceStatement statement, TContext context);

			T visit(IWhiteExpressionStatement statement, TContext context);

			T visit(IWhiteElseIfStatement statement, TContext context);

			T visit(IWhiteIfStatement statement, TContext context);

			T visit(IWhiteErrorStatement statement, TContext context);

			T visit(IWhiteDocuCommentStatement statement, TContext context);

			T visit(IWhiteCommentStatement statement, TContext context);

			T visit(IWhitePragmaStatement statement, TContext context);

			T visit(IWhiteLabelStatement statement, TContext context);

			T visit(IWhiteEmptyStatement statement, TContext context);

			T visit(IWhiteFinalStatement statement, TContext context);

			T visit(IWhiteReturnStatement statement, TContext context);

			T visit(IWhiteJumpStatement statement, TContext context);

			T visit(IWhiteExitStatement statement, TContext context);

			T visit(IWhiteContinueStatement statement, TContext context);

			T visit(IWhiteCaseStatement statement, TContext context);

			T visit(IWhiteCaseLabelStatement statement, TContext context);

			T visit(IWhiteForStatement statement, TContext context);

			T visit(IWhiteWhileStatement statement, TContext context);

			T visit(IWhiteRepeatStatement statement, TContext context);

			T visit(IWhiteTryCatchStatement statement, TContext context);

			T visit(IWhiteVariableDeclarationListStatement statement, TContext context);

			T visit(IWhiteVariableDeclarationStatement statement, TContext context);

			T visit(IWhitePropertyDeclarationStatement statement, TContext context);

			T visit(IWhiteProgramDeclarationStatement statement, TContext context);

			T visit(IWhiteFunctionBlockDeclarationStatement statement, TContext context);

			T visit(IWhiteMethodDeclarationStatement statement, TContext context);

			T visit(IWhiteInterfaceDeclarationStatement statement, TContext context);

			T visit(IWhiteFunctionDeclarationStatement statement, TContext context);

			T visit(IWhiteUnionDeclarationStatement statement, TContext context);

			T visit(IWhiteEnumDeclarationStatement statement, TContext context);

			T visit(IWhiteAliasDeclarationStatement statement, TContext context);

			T visit(IWhiteStructDeclarationStatement statement, TContext context);

			T visit(IWhiteSubrangeDeclarationStatement statement, TContext context);

			T visit(IWhitePragmaIfStatement statement, TContext context);

			T visit(IWhitePragmaElseIfStatement statement, TContext context);
		}

		void Accept(IStatementVisitor visitor);

		T Accept<[Nullable(2)] T>(IStatementVisitor<T> visitor);

		T Accept<[Nullable(2)] T, [Nullable(2)] TContext>(IStatementVisitor<T, TContext> visitor, TContext context);
	}
}

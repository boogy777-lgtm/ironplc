using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IStatementSyntax2 : IStatementSyntax
	{
		public interface IStatementVisitor2 : IStatementVisitor
		{
			void visit(IWhiteNamespaceDeclarationStatement statement);

			void visit(IWhitePropertyAccessorDeclarationStatement statement);

			void visit(IWhitePropertyAccessorStatement statement);

			void visit(IWhiteActionDeclarationStatement statement);

			void visit(IWhiteTransitionDeclarationStatement statement);

			void visit(IWhiteFunction statement);

			void visit(IWhiteFunctionBlock statement);

			void visit(IWhiteProgram statement);

			void visit(IWhiteInterface statement);

			void visit(IWhiteErrorPOU statement);

			void visit(IWhiteMethod statement);

			void visit(IWhiteAction statement);

			void visit(IWhiteTransition statement);

			void visit(IWhiteDUT statement);
		}

		public interface IStatementVisitor2<[Nullable(2)] out T> : IStatementVisitor<T>
		{
			T visit(IWhiteNamespaceDeclarationStatement statement);

			T visit(IWhitePropertyAccessorDeclarationStatement statement);

			T visit(IWhitePropertyAccessorStatement statement);

			T visit(IWhiteActionDeclarationStatement statement);

			T visit(IWhiteTransitionDeclarationStatement statement);

			T visit(IWhiteFunction statement);

			T visit(IWhiteFunctionBlock statement);

			T visit(IWhiteProgram statement);

			T visit(IWhiteInterface statement);

			T visit(IWhiteErrorPOU statement);

			T visit(IWhiteMethod statement);

			T visit(IWhiteAction statement);

			T visit(IWhiteTransition statement);

			T visit(IWhiteDUT statement);
		}

		public interface IStatementVisitor2<[Nullable(2)] out T, [Nullable(2)] in TContext> : IStatementVisitor<T, TContext>
		{
			T visit(IWhiteNamespaceDeclarationStatement statement, TContext context);

			T visit(IWhitePropertyAccessorDeclarationStatement statement, TContext context);

			T visit(IWhitePropertyAccessorStatement statement, TContext context);

			T visit(IWhiteActionDeclarationStatement statement, TContext context);

			T visit(IWhiteTransitionDeclarationStatement statement, TContext context);

			T visit(IWhiteFunction statement, TContext context);

			T visit(IWhiteFunctionBlock statement, TContext context);

			T visit(IWhiteProgram statement, TContext context);

			T visit(IWhiteInterface statement, TContext context);

			T visit(IWhiteErrorPOU statement, TContext context);

			T visit(IWhiteMethod statement, TContext context);

			T visit(IWhiteAction statement, TContext context);

			T visit(IWhiteTransition statement, TContext context);

			T visit(IWhiteDUT statement, TContext context);
		}

		void Accept(IStatementVisitor2 visitor);

		T Accept<[Nullable(2)] T>(IStatementVisitor2<T> visitor);

		T Accept<[Nullable(2)] T, [Nullable(2)] TContext>(IStatementVisitor2<T, TContext> visitor, TContext context);
	}
}

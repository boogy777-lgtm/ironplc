using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees.Factories
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteParseTreeStatementFactory
	{
		IWhiteSequenceStatement ParseStatement(string stInput);

		T ParseStatement<[Nullable(0)] T>(string stInput) where T : IWhiteStatement;

		IWhiteProgramDeclarationStatement CreateWhiteProgramDeclarationStatement(IProgramToken pouClass, IEnumerable<IAccessSpecifierToken> access, IWhiteExpression nameExpression, IWhiteSequenceStatement declarations);

		IWhiteFunctionBlockDeclarationStatement CreateWhiteFunctionBlockDeclarationStatement(IFunctionBlockToken pouClass, IEnumerable<IAccessSpecifierToken> access, IWhiteExpression nameExpression, IWhiteSequenceStatement genericDeclarations, [Nullable(2)] IExtendsToken extendsOp, IEnumerable<IWhiteExpression> extends, [Nullable(2)] IImplementsToken implementsOp, IEnumerable<IWhiteExpression> implements, IWhiteSequenceStatement declarations);

		IWhiteFunctionDeclarationStatement CreateWhiteFunctionDeclarationStatement(IFunctionToken pouClass, IEnumerable<IAccessSpecifierToken> access, IWhiteExpression nameExpression, [Nullable(2)] IColonToken colon, [Nullable(2)] IWhiteTypeExpression returnType, [Nullable(2)] ISemicolonToken returnTypeTrailingSemicolon, IWhiteSequenceStatement declarations);

		IWhiteMethodDeclarationStatement CreateWhiteMethodDeclarationStatement(IMethodToken pouClass, IEnumerable<IAccessSpecifierToken> access, IWhiteExpression nameExpression, [Nullable(2)] IColonToken colon, [Nullable(2)] IWhiteTypeExpression returnType, IWhiteSequenceStatement declarations);

		IWhiteInterfaceDeclarationStatement CreateWhiteInterfaceDeclarationStatement(IInterfaceToken pouClass, IWhiteExpression nameExpression, [Nullable(2)] IExtendsToken extendsOp, IEnumerable<IWhiteExpression> extends, IWhiteSequenceStatement declarations);

		IWhitePropertyDeclarationStatement CreateWhitePropertyDeclarationStatement(IPropertyToken propertyOp, IEnumerable<IAccessSpecifierToken> access, IWhiteExpression nameExpression, IColonToken colon, IWhiteTypeExpression returnType);

		IWhiteVariableDeclarationListStatement CreateWhiteVariableListDeclarationStatement(IVarListStartToken beginVarOp, IEnumerable<IVarAccessToken> persistantRetain, IWhiteSequenceStatement declarations, IVarListEndToken endVarOp);

		IWhiteVariableDeclarationStatement CreateWhiteVariableDeclarationStatement(IList<IWhiteExpression> variableNames, [Nullable(2)] IAtToken at, [Nullable(2)] IWhiteDirectVariableExpression addressLocation, IColonToken colon, IWhiteTypeExpression declaredType, [Nullable(2)] IAnyAssignmentToken assignment, [Nullable(2)] IWhiteExpression initializationExpression, ISemicolonToken semicolon);

		IWhiteExpressionStatement CreateWhiteExpressionStatement(IWhiteExpression whiteExpression);

		IWhiteElseIfStatement CreateWhiteElseIfStatement(IElseIfToken elseIf, IWhiteExpression condition, IThenToken then, IWhiteSequenceStatement thenStatement);

		IWhiteIfStatement CreateWhiteIfStatement(IIfToken _if, IWhiteExpression condition, IThenToken _then, IWhiteSequenceStatement thenStatement, IEnumerable<IWhiteElseIfStatement> elseIfStatements, IElseToken _else, IWhiteSequenceStatement elseStatement, IEndIfToken _endif);

		IWhiteForStatement CreateWhiteForStatement(IForToken _for, IWhiteAssignmentExpression startExpression, IToToken to, IWhiteExpression upperBound, IByToken by, IWhiteExpression stepWidth, IDoToken _do, IWhiteSequenceStatement controlled, IEndForToken endFor);

		IWhiteWhileStatement CreateWhiteWhileStatement(IWhileToken _while, IWhiteExpression condition, IDoToken _do, IWhiteSequenceStatement controlled, IEndWhileToken endWhile);

		IWhiteRepeatStatement CreateWhiteRepeatStatement(IRepeatToken repeat, IWhiteSequenceStatement controlled, IUntilToken until, IWhiteExpression condition, IEndRepeatToken endRepeat);

		IWhiteErrorStatement CreateWhiteErrorStatement(IEnumerable<IWhiteToken> tokens);

		IWhiteDocuCommentStatement CreateWhiteDocuCommentStatement(IDocCommentToken token);

		IWhiteCommentStatement CreateWhiteCommentStatement(ICommentToken token);

		IWhitePragmaStatement CreateWhitePragmaStatement(IPragmaToken token);

		IWhiteEmptyStatement CreateWhiteEmptyStatement(ISemicolonToken semicolon);

		IWhiteFinalStatement CreateWhiteFinalStatement(IEndToken leading);

		IWhiteCaseLabelStatement CreateWhiteCaseLabelStatement(IEnumerable<IWhiteExpression> caseExpressionList, IColonToken colon);

		IWhiteCaseStatement CreateWhiteCaseStatement(ICaseToken _case, IWhiteExpression _switch, IOfToken _of, IEnumerable<IWhiteCase> cases, IElseToken elsetoken, IWhiteSequenceStatement _else, IEndCaseToken endcase);

		IWhiteTryCatchStatement CreateWhiteTryCatchStatement(ITryToken _try, IWhiteSequenceStatement trySequence, ICatchToken _catch, IParenthesizedExpression exceptionExpression, IWhiteSequenceStatement catchSequence, IFinallyToken _finally, IWhiteSequenceStatement finallySequence, IEndTryToken endtry);

		IWhiteReturnStatement CreateWhiteReturnStatement(IReturnToken tokenReturn, IParenthesizedExpression condition, ISemicolonToken semicolon);

		IWhiteExitStatement CreateWhiteExitStatement(IExitToken exitToken, ISemicolonToken semicolon);

		IWhiteContinueStatement CreateWhiteContinueStatement(IContinueToken cContinue, ISemicolonToken semicolon);

		IWhiteSequenceStatement CreateWhiteSequenceStatement();
	}
}

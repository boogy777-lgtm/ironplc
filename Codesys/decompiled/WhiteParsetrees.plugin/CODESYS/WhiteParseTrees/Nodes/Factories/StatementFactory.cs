using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CODESYS.WhiteParseTrees.Factories;
using CODESYS.WhiteParseTrees.Nodes.Statements;
using CODESYS.WhiteParseTrees.Parser;

namespace CODESYS.WhiteParseTrees.Nodes.Factories
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class StatementFactory : IWhiteParseTreeStatementFactory3, IWhiteParseTreeStatementFactory2, IWhiteParseTreeStatementFactory
	{
		public IWhiteSequenceStatement ParseStatement(string stInput)
		{
			return WhiteTreeParser.ParseStatement(stInput);
		}

		public T ParseStatement<[System.Runtime.CompilerServices.Nullable(0)] T>(string stInput) where T : IWhiteStatement
		{
			IWhiteStatement whiteStatement = ParseStatement(stInput).FirstOrDefault();
			return (T)(whiteStatement ?? throw new InvalidOperationException("stInput does not contain any statements"));
		}

		public IWhiteProgramDeclarationStatement CreateWhiteProgramDeclarationStatement(IProgramToken pouClass, IEnumerable<IAccessSpecifierToken> access, IWhiteExpression nameExpression, IWhiteSequenceStatement declarations)
		{
			return new WhiteProgramDeclarationStatement(pouClass, access, nameExpression, declarations);
		}

		public IWhiteFunctionBlockDeclarationStatement CreateWhiteFunctionBlockDeclarationStatement(IFunctionBlockToken pouClass, IEnumerable<IAccessSpecifierToken> access, IWhiteExpression nameExpression, IWhiteSequenceStatement genericDeclarations, [System.Runtime.CompilerServices.Nullable(2)] IExtendsToken extendsOp, IEnumerable<IWhiteExpression> extends, [System.Runtime.CompilerServices.Nullable(2)] IImplementsToken implementsOp, IEnumerable<IWhiteExpression> implements, IWhiteSequenceStatement declarations)
		{
			return new WhiteFunctionBlockDeclarationStatement(pouClass, access, nameExpression, genericDeclarations, extendsOp, extends, implementsOp, implements, declarations);
		}

		public IWhiteFunctionDeclarationStatement CreateWhiteFunctionDeclarationStatement(IFunctionToken pouClass, IEnumerable<IAccessSpecifierToken> access, IWhiteExpression nameExpression, [System.Runtime.CompilerServices.Nullable(2)] IColonToken colon, [System.Runtime.CompilerServices.Nullable(2)] IWhiteTypeExpression returnType, [System.Runtime.CompilerServices.Nullable(2)] ISemicolonToken returnTypeTrailingSemicolon, IWhiteSequenceStatement declarations)
		{
			return new WhiteFunctionDeclarationStatement(pouClass, access, nameExpression, colon, returnType, returnTypeTrailingSemicolon, declarations);
		}

		public IWhiteMethodDeclarationStatement CreateWhiteMethodDeclarationStatement(IMethodToken pouClass, IEnumerable<IAccessSpecifierToken> access, IWhiteExpression nameExpression, [System.Runtime.CompilerServices.Nullable(2)] IColonToken colon, [System.Runtime.CompilerServices.Nullable(2)] IWhiteTypeExpression returnType, IWhiteSequenceStatement declarations)
		{
			return new WhiteMethodDeclarationStatement(pouClass, access, nameExpression, colon, returnType, declarations);
		}

		public IWhiteInterfaceDeclarationStatement CreateWhiteInterfaceDeclarationStatement(IInterfaceToken pouClass, IWhiteExpression nameExpression, [System.Runtime.CompilerServices.Nullable(2)] IExtendsToken extendsOp, IEnumerable<IWhiteExpression> extends, IWhiteSequenceStatement declarations)
		{
			return new WhiteInterfaceDeclarationStatement(pouClass, Enumerable.Empty<IAccessSpecifierToken>(), nameExpression, extendsOp, extends, null, Enumerable.Empty<IWhiteExpression>(), declarations);
		}

		IWhiteInterfaceDeclarationStatement IWhiteParseTreeStatementFactory2.CreateWhiteInterfaceDeclarationStatement(IInterfaceToken pouClass, IEnumerable<IAccessSpecifierToken> access, IWhiteExpression nameExpression, [System.Runtime.CompilerServices.Nullable(2)] IExtendsToken extendsOp, IEnumerable<IWhiteExpression> extends, IWhiteSequenceStatement declarations)
		{
			return new WhiteInterfaceDeclarationStatement(pouClass, access, nameExpression, extendsOp, extends, null, Enumerable.Empty<IWhiteExpression>(), declarations);
		}

		public IWhiteInterfaceDeclarationStatement2 CreateWhiteInterfaceDeclarationStatement(IInterfaceToken pouClass, IEnumerable<IAccessSpecifierToken> access, IWhiteExpression nameExpression, [System.Runtime.CompilerServices.Nullable(2)] IExtendsToken extendsOp, IEnumerable<IWhiteExpression> extends, IWhiteSequenceStatement declarations)
		{
			return new WhiteInterfaceDeclarationStatement(pouClass, access, nameExpression, extendsOp, extends, null, Enumerable.Empty<IWhiteExpression>(), declarations);
		}

		public IWhiteInterfaceDeclarationStatement3 CreateWhiteInterfaceDeclarationStatement(IInterfaceToken pouClass, IEnumerable<IAccessSpecifierToken> access, IWhiteExpression nameExpression, [System.Runtime.CompilerServices.Nullable(2)] IExtendsToken extendsOp, IEnumerable<IWhiteExpression> extends, [System.Runtime.CompilerServices.Nullable(2)] IImplementsToken implementsToken, IEnumerable<IWhiteExpression> implements, IWhiteSequenceStatement declarations)
		{
			return new WhiteInterfaceDeclarationStatement(pouClass, access, nameExpression, extendsOp, extends, implementsToken, implements, declarations);
		}

		public IWhiteStructDeclarationStatement2 CreateWhiteStructDeclarationStatement(ITypeToken typeOp, List<IAccessSpecifierToken> access, IWhiteExpression nameExpression, [System.Runtime.CompilerServices.Nullable(2)] IExtendsToken extendsOp, IEnumerable<IWhiteExpression> extends, IColonToken colonOp, IWhiteStatement declaration, IEndTypeToken endTypeOp)
		{
			return new WhiteStructDeclarationStatement(typeOp, access, nameExpression, extendsOp, extends, colonOp, declaration, endTypeOp);
		}

		public IWhiteEnumDeclarationStatement2 CreateWhiteEnumDeclarationStatement(ITypeToken typeOp, List<IAccessSpecifierToken> access, IWhiteExpression nameExpression, IColonToken colonOp, IEnumerationTypeExpression enumerationTypeExpression, [System.Runtime.CompilerServices.Nullable(2)] IWhiteTypeExpression typeExpression, [System.Runtime.CompilerServices.Nullable(2)] IAssignToken assignmentOp, [System.Runtime.CompilerServices.Nullable(2)] IWhiteExpression initialization, ISemicolonToken semicolon, IEndTypeToken endTypeOp)
		{
			return new WhiteEnumDeclarationStatement(typeOp, access, nameExpression, colonOp, enumerationTypeExpression, typeExpression, assignmentOp, initialization, semicolon, endTypeOp);
		}

		public IWhiteUnionDeclarationStatement2 CreateWhiteUnionDeclarationStatement(ITypeToken typeOp, List<IAccessSpecifierToken> access, IWhiteExpression nameExpression, IColonToken colonOp, IWhiteStatement declaration, IEndTypeToken endTypeOp)
		{
			return new WhiteUnionDeclarationStatement(typeOp, access, nameExpression, colonOp, declaration, endTypeOp);
		}

		public IWhiteAliasDeclarationStatement2 CreateWhiteAliasDeclarationStatement(ITypeToken typeToken, List<IAccessSpecifierToken> access, IWhiteExpression nameExpression, IColonToken colonOp, IWhiteTypeExpression type, ISemicolonToken semicolon, IEndTypeToken endTypeOp)
		{
			return new WhiteAliasDeclarationStatement(typeToken, access, nameExpression, colonOp, type, semicolon, endTypeOp);
		}

		public IWhitePropertyDeclarationStatement CreateWhitePropertyDeclarationStatement(IPropertyToken propertyOp, IEnumerable<IAccessSpecifierToken> access, IWhiteExpression nameExpression, IColonToken colon, IWhiteTypeExpression returnType)
		{
			return new WhitePropertyDeclarationStatement(propertyOp, access, nameExpression, colon, returnType);
		}

		public IWhiteVariableDeclarationListStatement CreateWhiteVariableListDeclarationStatement(IVarListStartToken beginVarOp, IEnumerable<IVarAccessToken> persistantRetain, IWhiteSequenceStatement declarations, IVarListEndToken endVarOp)
		{
			return new WhiteVariableDeclarationListStatement(beginVarOp, persistantRetain, declarations, endVarOp);
		}

		public IWhiteVariableDeclarationStatement CreateWhiteVariableDeclarationStatement(IList<IWhiteExpression> variableNames, [System.Runtime.CompilerServices.Nullable(2)] IAtToken at, [System.Runtime.CompilerServices.Nullable(2)] IWhiteDirectVariableExpression addressLocation, IColonToken colon, IWhiteTypeExpression declaredType, [System.Runtime.CompilerServices.Nullable(2)] IAnyAssignmentToken assignment, [System.Runtime.CompilerServices.Nullable(2)] IWhiteExpression initializationExpression, ISemicolonToken semicolon)
		{
			return new WhiteVariableDeclarationStatement(variableNames, at, addressLocation, colon, declaredType, assignment, initializationExpression, semicolon);
		}

		public IWhiteExpressionStatement CreateWhiteExpressionStatement(IWhiteExpression whiteExpression)
		{
			return new WhiteExpressionStatement(whiteExpression, TokenFactory<ISemicolonToken>.Create(";"));
		}

		public IWhiteElseIfStatement CreateWhiteElseIfStatement(IElseIfToken elseIf, IWhiteExpression condition, IThenToken then, IWhiteSequenceStatement thenStatement)
		{
			return new WhiteElseIfStatement(elseIf, condition, then, thenStatement);
		}

		public IWhiteIfStatement CreateWhiteIfStatement(IIfToken _if, IWhiteExpression condition, IThenToken _then, IWhiteSequenceStatement thenStatement, IEnumerable<IWhiteElseIfStatement> elseIfStatements, IElseToken _else, IWhiteSequenceStatement elseStatement, IEndIfToken _endif)
		{
			return new WhiteIfStatement(_if, condition, _then, thenStatement, elseIfStatements, _else, elseStatement, _endif);
		}

		public IWhiteForStatement CreateWhiteForStatement(IForToken _for, IWhiteAssignmentExpression startExpression, IToToken to, IWhiteExpression upperBound, IByToken by, IWhiteExpression stepWidth, IDoToken _do, IWhiteSequenceStatement controlled, IEndForToken endFor)
		{
			return new WhiteForStatement(_for, startExpression, to, upperBound, by, stepWidth, _do, controlled, endFor);
		}

		public IWhiteWhileStatement CreateWhiteWhileStatement(IWhileToken _while, IWhiteExpression condition, IDoToken _do, IWhiteSequenceStatement controlled, IEndWhileToken endWhile)
		{
			return new WhiteWhileStatement(_while, condition, _do, controlled, endWhile);
		}

		public IWhiteRepeatStatement CreateWhiteRepeatStatement(IRepeatToken repeat, IWhiteSequenceStatement controlled, IUntilToken until, IWhiteExpression condition, IEndRepeatToken endRepeat)
		{
			return new WhiteRepeatStatement(repeat, controlled, until, condition, endRepeat);
		}

		public IWhiteErrorStatement CreateWhiteErrorStatement(IEnumerable<IWhiteToken> tokens)
		{
			return new WhiteErrorStatement(tokens);
		}

		public IWhiteErrorStatement CreateWhiteErrorStatement(IEnumerable<IWhiteToken> tokens, string errorMessage, IWhiteToken errorToken)
		{
			return new WhiteErrorStatement(tokens, errorMessage, errorToken);
		}

		public IWhiteDocuCommentStatement CreateWhiteDocuCommentStatement(IDocCommentToken token)
		{
			return new WhiteDocuCommentStatement(token);
		}

		public IWhiteCommentStatement CreateWhiteCommentStatement(ICommentToken token)
		{
			return new WhiteCommentStatement(token);
		}

		public IWhitePragmaStatement CreateWhitePragmaStatement(IPragmaToken token)
		{
			return new WhitePragmaStatement(token);
		}

		public IWhiteEmptyStatement CreateWhiteEmptyStatement(ISemicolonToken semicolon)
		{
			return new WhiteEmptyStatement(semicolon);
		}

		public IWhiteFinalStatement CreateWhiteFinalStatement(IEndToken leading)
		{
			return new WhiteFinalStatement(leading);
		}

		public IWhiteCaseLabelStatement CreateWhiteCaseLabelStatement(IEnumerable<IWhiteExpression> caseExpressionList, IColonToken colon)
		{
			return new WhiteCaseLabelStatement(caseExpressionList, colon);
		}

		public IWhiteCaseStatement CreateWhiteCaseStatement(ICaseToken _case, IWhiteExpression _switch, IOfToken _of, IEnumerable<IWhiteCase> cases, IElseToken elsetoken, IWhiteSequenceStatement _else, IEndCaseToken endcase)
		{
			return new WhiteCaseStatement(_case, _switch, _of, cases, elsetoken, _else, endcase);
		}

		public IWhiteTryCatchStatement CreateWhiteTryCatchStatement(ITryToken _try, IWhiteSequenceStatement trySequence, ICatchToken _catch, IParenthesizedExpression exceptionExpression, IWhiteSequenceStatement catchSequence, IFinallyToken _finally, IWhiteSequenceStatement finallySequence, IEndTryToken endtry)
		{
			return new WhiteTryCatchStatement(_try, trySequence, _catch, exceptionExpression, catchSequence, _finally, finallySequence, endtry);
		}

		public IWhiteReturnStatement CreateWhiteReturnStatement(IReturnToken tokenReturn, IParenthesizedExpression condition, ISemicolonToken semicolon)
		{
			return new WhiteReturnStatement(tokenReturn, condition, semicolon);
		}

		public IWhiteExitStatement CreateWhiteExitStatement(IExitToken exitToken, ISemicolonToken semicolon)
		{
			return new WhiteExitStatement(exitToken, semicolon);
		}

		public IWhiteContinueStatement CreateWhiteContinueStatement(IContinueToken cContinue, ISemicolonToken semicolon)
		{
			return new WhiteContinueStatement(cContinue, semicolon);
		}

		public IWhiteSequenceStatement CreateWhiteSequenceStatement()
		{
			return new WhiteSequenceStatement();
		}
	}
}

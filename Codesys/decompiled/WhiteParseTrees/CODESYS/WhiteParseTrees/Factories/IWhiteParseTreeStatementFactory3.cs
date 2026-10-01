using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees.Factories
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteParseTreeStatementFactory3 : IWhiteParseTreeStatementFactory2, IWhiteParseTreeStatementFactory
	{
		IWhiteInterfaceDeclarationStatement3 CreateWhiteInterfaceDeclarationStatement(IInterfaceToken pouClass, IEnumerable<IAccessSpecifierToken> access, IWhiteExpression nameExpression, [Nullable(2)] IExtendsToken extendsOp, IEnumerable<IWhiteExpression> extends, [Nullable(2)] IImplementsToken implementsToken, IEnumerable<IWhiteExpression> implements, IWhiteSequenceStatement declarations);

		IWhiteStructDeclarationStatement2 CreateWhiteStructDeclarationStatement(ITypeToken typeOp, List<IAccessSpecifierToken> access, IWhiteExpression nameExpression, [Nullable(2)] IExtendsToken extendsOp, IEnumerable<IWhiteExpression> extends, IColonToken colonOp, IWhiteStatement declaration, IEndTypeToken endTypeOp);

		IWhiteEnumDeclarationStatement2 CreateWhiteEnumDeclarationStatement(ITypeToken typeOp, List<IAccessSpecifierToken> access, IWhiteExpression nameExpression, IColonToken colonOp, IEnumerationTypeExpression enumerationTypeExpression, [Nullable(2)] IWhiteTypeExpression typeExpression, [Nullable(2)] IAssignToken assignmentOp, [Nullable(2)] IWhiteExpression initialization, ISemicolonToken semicolon, IEndTypeToken endTypeOp);

		IWhiteUnionDeclarationStatement2 CreateWhiteUnionDeclarationStatement(ITypeToken typeOp, List<IAccessSpecifierToken> access, IWhiteExpression nameExpression, IColonToken colonOp, IWhiteStatement declaration, IEndTypeToken endTypeOp);

		IWhiteAliasDeclarationStatement2 CreateWhiteAliasDeclarationStatement(ITypeToken typeToken, List<IAccessSpecifierToken> access, IWhiteExpression nameExpression, IColonToken colonOp, IWhiteTypeExpression type, ISemicolonToken semicolon, IEndTypeToken endTypeOp);

		IWhiteErrorStatement CreateWhiteErrorStatement(IEnumerable<IWhiteToken> tokens, string errorMessage, IWhiteToken errorToken);
	}
}

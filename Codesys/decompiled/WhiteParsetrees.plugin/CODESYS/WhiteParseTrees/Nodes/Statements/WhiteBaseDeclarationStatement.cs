using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public abstract class WhiteBaseDeclarationStatement : WhiteStatement, IWhitePouDeclarationStatement2, IWhitePouDeclarationStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax, IWhiteDeclarationStatement
	{
		public IPouTypeToken Class { get; set; }

		public IWhiteExpression NameExpression { get; set; }

		public IWhiteSequenceStatement Declarations { get; set; }

		protected WhiteBaseDeclarationStatement(IPouTypeToken pouClass, IWhiteExpression nameExpression, IWhiteSequenceStatement declarations)
		{
			NameExpression = nameExpression;
			Declarations = declarations;
			Class = pouClass;
		}
	}
}

using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Declaration
{
	public class DeclarationParser
	{
		private ParserContext Context { get; }

		internal bool Library { get; set; }

		internal bool AllowPaths { get; set; }

		internal Operator OpCurrentVariableList { get; set; }

		internal DeclarationParser(ParserContext context)
		{
			Context = context;
		}

		internal _IStatement ParsePOUDeclaration(IToken tokenPOUType, Operator opParam)
		{
			return POUDeclarationParser.ParsePOUDeclaration(Context, tokenPOUType, opParam);
		}

		internal _IStatement ParseVariableList(IToken tokenVar)
		{
			return VariableListParser.ParseVariableList(Context, tokenVar);
		}

		internal _IStatement ParseVariableDeclaration(IToken tokenIdent)
		{
			return VariableDeclarationParser.ParseVariableDeclaration(Context, tokenIdent);
		}

		internal _IStatement ParseTypeDeclaration(IToken tokenType)
		{
			return TypeDeclarationParser.ParseTypeDeclaration(Context, tokenType);
		}
	}
}

using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Declaration;
using CODESYS.Parser35210.Expressions;
using CODESYS.Parser35210.Pragmas;
using CODESYS.Parser35210.Scanner;
using CODESYS.Parser35210.Statements;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210
{
	[TypeGuid("{0d868684-5659-43b6-9010-1cfcb94174ab}")]
	public class ParserService : IParserService, ILanguageVersionDependentService
	{
		internal static readonly Version s_LanguageVersion = new Version(3, 5, 21, 0);

		public Version LanguageVersion => s_LanguageVersion;

		public IInternalParser CreateParser(IScanner9 scanner, IErrorHandler errorhandler, _ILanguageModelBuilder7 factory, ITypeTable3 typeTable, ILMCompileOptions3 compileOptions, _ICompilerVersionSettings compilerVersionSettings)
		{
			ParserContext parserContext = new ParserContext(scanner, errorhandler, factory, typeTable, compileOptions, TokenFactoryClass.Singleton, compilerVersionSettings);
			parserContext.StatementParser = new StatementParser(parserContext);
			parserContext.ExpressionParser = new ExpressionParser(parserContext);
			parserContext.TypeParser = new TypeParser(parserContext);
			parserContext.OperandParser = new OperandParser(parserContext);
			parserContext.InitializationParser = new InitializationParser(parserContext);
			parserContext.InternalParser = new InternalParser(parserContext);
			parserContext.DeclarationParser = new DeclarationParser(parserContext);
			parserContext.PragmaStatementParser = new PragmaStatementParser(parserContext);
			return parserContext.InternalParser;
		}
	}
}

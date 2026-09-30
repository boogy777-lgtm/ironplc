using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Declaration;
using CODESYS.Parser35220.Expressions;
using CODESYS.Parser35220.Pragmas;
using CODESYS.Parser35220.Scanner;
using CODESYS.Parser35220.Statements;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220
{
	// Token: 0x02000009 RID: 9
	[TypeGuid("{a1e5ee95-c0be-4c81-be2f-298307a92b89}")]
	public class ParserService : IParserService, ILanguageVersionDependentService
	{
		// Token: 0x1700000C RID: 12
		// (get) Token: 0x0600003C RID: 60 RVA: 0x00002DE9 File Offset: 0x00000FE9
		public Version LanguageVersion
		{
			get
			{
				return ParserService.s_LanguageVersion;
			}
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00002DF0 File Offset: 0x00000FF0
		public IInternalParser CreateParser(IScanner9 scanner, IErrorHandler errorhandler, _ILanguageModelBuilder7 factory, ITypeTable3 typeTable, ILMCompileOptions3 compileOptions, _ICompilerVersionSettings compilerVersionSettings)
		{
			ParserContext parserContext = new ParserContext(scanner, errorhandler, (_ILanguageModelBuilder8)factory, typeTable, compileOptions, TokenFactoryClass.Singleton, compilerVersionSettings);
			parserContext.StatementParser = new StatementParser(parserContext);
			parserContext.ExpressionParser = new ExpressionParser(parserContext);
			parserContext.TypeParser = new TypeParser(parserContext);
			parserContext.OperandParser = new OperandParser(parserContext);
			parserContext.InitializationParser = new InitializationParser(parserContext);
			parserContext.InternalParser = new InternalParser(parserContext);
			parserContext.DeclarationParser = new DeclarationParser(parserContext);
			IScanner9 scannerForPragmaParser = ((InternalScanner)scanner).CreateScanner(string.Empty);
			parserContext.PragmaStatementParser = new PragmaStatementParser(parserContext, scannerForPragmaParser);
			parserContext.InfixOperationParser = new InfixOperationParser(parserContext);
			parserContext.ParenthesizedExpressionParser = new ParenthesizedExpressionParser(parserContext);
			parserContext.FunctionCallParser = new FunctionCallParser(parserContext);
			return parserContext.InternalParser;
		}

		// Token: 0x04000005 RID: 5
		internal static readonly Version s_LanguageVersion = new Version(3, 5, 22, 0);
	}
}

using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Declaration;
using CODESYS.Parser35220.Expressions;
using CODESYS.Parser35220.Pragmas;
using CODESYS.Parser35220.Statements;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Utilities
{
	// Token: 0x0200001D RID: 29
	internal class ParserContext
	{
		// Token: 0x06000207 RID: 519 RVA: 0x0000C4B8 File Offset: 0x0000A6B8
		internal ParserContext(IScanner9 scanner, IErrorHandler errorhandler, _ILanguageModelBuilder8 factory, ITypeTable3 typeTable, ILMCompileOptions3 compileOptions, ITokenFactory tokenFactory, _ICompilerVersionSettings compilerVersionSettings)
		{
			this.Scanner = scanner;
			this.ErrorHandler = errorhandler;
			this.LMItemFactory = factory;
			this.TypeTable = typeTable;
			this.CompileOptions = compileOptions;
			this.TokenFactory = tokenFactory;
			this.CompilerVersionSettings = compilerVersionSettings;
			Version v = compilerVersionSettings.CompilerVersionToUseInternal();
			this._bReportSP18Feature = (v < ParserContext.CompilerVersion18);
			this._bReportSP19Feature = (v < ParserContext.CompilerVersion19);
			this._bReportSP20Feature = (v < ParserContext.CompilerVersion20);
		}

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x06000208 RID: 520 RVA: 0x0000C53B File Offset: 0x0000A73B
		public _ICompilerVersionSettings CompilerVersionSettings { get; }

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x06000209 RID: 521 RVA: 0x0000C543 File Offset: 0x0000A743
		public _ILanguageModelBuilder8 LMItemFactory { get; }

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x0600020A RID: 522 RVA: 0x0000C54B File Offset: 0x0000A74B
		public ITypeTable3 TypeTable { get; }

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x0600020B RID: 523 RVA: 0x0000C553 File Offset: 0x0000A753
		public ILMCompileOptions3 CompileOptions { get; }

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x0600020C RID: 524 RVA: 0x0000C55B File Offset: 0x0000A75B
		public ITokenFactory TokenFactory { get; }

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x0600020D RID: 525 RVA: 0x0000C563 File Offset: 0x0000A763
		// (set) Token: 0x0600020E RID: 526 RVA: 0x0000C56B File Offset: 0x0000A76B
		public IScanner9 Scanner { get; set; }

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x0600020F RID: 527 RVA: 0x0000C574 File Offset: 0x0000A774
		public IErrorHandler ErrorHandler { get; }

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x06000210 RID: 528 RVA: 0x0000C57C File Offset: 0x0000A77C
		// (set) Token: 0x06000211 RID: 529 RVA: 0x0000C584 File Offset: 0x0000A784
		public StatementParser StatementParser { get; set; }

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x06000212 RID: 530 RVA: 0x0000C58D File Offset: 0x0000A78D
		// (set) Token: 0x06000213 RID: 531 RVA: 0x0000C595 File Offset: 0x0000A795
		public DeclarationParser DeclarationParser { get; set; }

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x06000214 RID: 532 RVA: 0x0000C59E File Offset: 0x0000A79E
		// (set) Token: 0x06000215 RID: 533 RVA: 0x0000C5A6 File Offset: 0x0000A7A6
		public TypeParser TypeParser { get; set; }

		// Token: 0x17000054 RID: 84
		// (get) Token: 0x06000216 RID: 534 RVA: 0x0000C5AF File Offset: 0x0000A7AF
		// (set) Token: 0x06000217 RID: 535 RVA: 0x0000C5B7 File Offset: 0x0000A7B7
		public ExpressionParser ExpressionParser { get; set; }

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x06000218 RID: 536 RVA: 0x0000C5C0 File Offset: 0x0000A7C0
		// (set) Token: 0x06000219 RID: 537 RVA: 0x0000C5C8 File Offset: 0x0000A7C8
		public OperandParser OperandParser { get; set; }

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x0600021A RID: 538 RVA: 0x0000C5D1 File Offset: 0x0000A7D1
		// (set) Token: 0x0600021B RID: 539 RVA: 0x0000C5D9 File Offset: 0x0000A7D9
		public InitializationParser InitializationParser { get; set; }

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x0600021C RID: 540 RVA: 0x0000C5E2 File Offset: 0x0000A7E2
		// (set) Token: 0x0600021D RID: 541 RVA: 0x0000C5EA File Offset: 0x0000A7EA
		public PragmaStatementParser PragmaStatementParser { get; set; }

		// Token: 0x17000058 RID: 88
		// (get) Token: 0x0600021E RID: 542 RVA: 0x0000C5F3 File Offset: 0x0000A7F3
		// (set) Token: 0x0600021F RID: 543 RVA: 0x0000C5FB File Offset: 0x0000A7FB
		public InfixOperationParser InfixOperationParser { get; set; }

		// Token: 0x17000059 RID: 89
		// (get) Token: 0x06000220 RID: 544 RVA: 0x0000C604 File Offset: 0x0000A804
		// (set) Token: 0x06000221 RID: 545 RVA: 0x0000C60C File Offset: 0x0000A80C
		public InternalParser InternalParser { get; set; }

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x06000222 RID: 546 RVA: 0x0000C615 File Offset: 0x0000A815
		// (set) Token: 0x06000223 RID: 547 RVA: 0x0000C61D File Offset: 0x0000A81D
		public ParenthesizedExpressionParser ParenthesizedExpressionParser { get; set; }

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x06000224 RID: 548 RVA: 0x0000C626 File Offset: 0x0000A826
		// (set) Token: 0x06000225 RID: 549 RVA: 0x0000C62E File Offset: 0x0000A82E
		public FunctionCallParser FunctionCallParser { get; set; }

		// Token: 0x06000226 RID: 550 RVA: 0x0000C637 File Offset: 0x0000A837
		public void AddUnsupportedFeatureError(_IExprement exprement, string featureName, Version minVersion)
		{
			this.ErrorHandler.AddErrorST(exprement, 579, new object[]
			{
				featureName,
				this.CompilerVersionSettings.MapFromInternalToOEMTextSave(minVersion)
			});
		}

		// Token: 0x04000065 RID: 101
		public static readonly Version CompilerVersion18 = new Version(3, 5, 18, 0);

		// Token: 0x04000066 RID: 102
		public static readonly Version CompilerVersion19 = new Version(3, 5, 19, 0);

		// Token: 0x04000067 RID: 103
		public static readonly Version CompilerVersion20 = new Version(3, 5, 20, 0);

		// Token: 0x04000068 RID: 104
		public readonly bool _bReportSP18Feature;

		// Token: 0x04000069 RID: 105
		public readonly bool _bReportSP19Feature;

		// Token: 0x0400006A RID: 106
		public readonly bool _bReportSP20Feature;
	}
}

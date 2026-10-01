using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Declaration;
using CODESYS.Parser35210.Expressions;
using CODESYS.Parser35210.Pragmas;
using CODESYS.Parser35210.Statements;

namespace CODESYS.Parser35210.Utilities
{
	internal class ParserContext
	{
		public static readonly Version CompilerVersion18 = new Version(3, 5, 18, 0);

		public static readonly Version CompilerVersion19 = new Version(3, 5, 19, 0);

		public static readonly Version CompilerVersion20 = new Version(3, 5, 20, 0);

		public readonly bool _bReportSP18Feature;

		public readonly bool _bReportSP19Feature;

		public readonly bool _bReportSP20Feature;

		public _ICompilerVersionSettings CompilerVersionSettings { get; }

		public _ILanguageModelBuilder7 LMItemFactory { get; }

		public ITypeTable3 TypeTable { get; }

		public ILMCompileOptions3 CompileOptions { get; }

		public ITokenFactory TokenFactory { get; }

		public IScanner9 Scanner { get; set; }

		public IErrorHandler ErrorHandler { get; }

		public StatementParser StatementParser { get; set; }

		public DeclarationParser DeclarationParser { get; set; }

		public TypeParser TypeParser { get; set; }

		public ExpressionParser ExpressionParser { get; set; }

		public OperandParser OperandParser { get; set; }

		public InitializationParser InitializationParser { get; set; }

		public PragmaStatementParser PragmaStatementParser { get; set; }

		public InternalParser InternalParser { get; set; }

		internal ParserContext(IScanner9 scanner, IErrorHandler errorhandler, _ILanguageModelBuilder7 factory, ITypeTable3 typeTable, ILMCompileOptions3 compileOptions, ITokenFactory tokenFactory, _ICompilerVersionSettings compilerVersionSettings)
		{
			Scanner = scanner;
			ErrorHandler = errorhandler;
			LMItemFactory = factory;
			TypeTable = typeTable;
			CompileOptions = compileOptions;
			TokenFactory = tokenFactory;
			CompilerVersionSettings = compilerVersionSettings;
			Version version = compilerVersionSettings.CompilerVersionToUseInternal();
			_bReportSP18Feature = version < CompilerVersion18;
			_bReportSP19Feature = version < CompilerVersion19;
			_bReportSP20Feature = version < CompilerVersion20;
		}

		public void AddUnsupportedFeatureError(_IExprement exprement, string featureName, Version minVersion)
		{
			ErrorHandler.AddErrorST(exprement, MessageId.Err_UnsupportedFeature, featureName, CompilerVersionSettings.MapFromInternalToOEMTextSave(minVersion));
		}
	}
}

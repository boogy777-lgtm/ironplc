using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.CommonCompilerData;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000CA RID: 202
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "Further reduction of class coupling will lead to unreasonably splitting this class")]
	internal class CompilerProxy
	{
		// Token: 0x17000369 RID: 873
		// (get) Token: 0x06000CA6 RID: 3238 RVA: 0x0001FB22 File Offset: 0x0001EB22
		internal static ILMQualifierService _QualifierService_OrNull
		{
			get
			{
				return VersionedCompilerFactory._QualifierService_OrNull;
			}
		}

		// Token: 0x1700036A RID: 874
		// (get) Token: 0x06000CA7 RID: 3239 RVA: 0x0001FB29 File Offset: 0x0001EB29
		internal static ILMTypeService _TypeService_OrNull
		{
			get
			{
				return VersionedCompilerFactory._TypeService_OrNull;
			}
		}

		// Token: 0x1700036B RID: 875
		// (get) Token: 0x06000CA8 RID: 3240 RVA: 0x0001FB30 File Offset: 0x0001EB30
		internal static ILMStringEncodingService _StringEncodingService_OrNull
		{
			get
			{
				return VersionedCompilerFactory._StringEncodingService_OrNull;
			}
		}

		// Token: 0x1700036C RID: 876
		// (get) Token: 0x06000CA9 RID: 3241 RVA: 0x0001FB37 File Offset: 0x0001EB37
		internal static ICompilerServiceExprementWriter _ExprementWriter
		{
			get
			{
				return VersionedCompilerFactory._ExprementWriter;
			}
		}

		// Token: 0x1700036D RID: 877
		// (get) Token: 0x06000CAA RID: 3242 RVA: 0x0001FB3E File Offset: 0x0001EB3E
		internal static ITypeTable _TypeTable
		{
			get
			{
				return CompilerProxy._Compiler.TypeTable;
			}
		}

		// Token: 0x1700036E RID: 878
		// (get) Token: 0x06000CAB RID: 3243 RVA: 0x0001FB4A File Offset: 0x0001EB4A
		private static ICompiler _Compiler
		{
			get
			{
				return VersionedCompilerFactory._Compiler;
			}
		}

		// Token: 0x1700036F RID: 879
		// (get) Token: 0x06000CAC RID: 3244 RVA: 0x0001FB51 File Offset: 0x0001EB51
		private static ICompilerHelper _Helper
		{
			get
			{
				return VersionedCompilerFactory._Helper;
			}
		}

		// Token: 0x17000370 RID: 880
		// (get) Token: 0x06000CAD RID: 3245 RVA: 0x0001FB58 File Offset: 0x0001EB58
		private static ITypeCompiler _TypeCompiler
		{
			get
			{
				return VersionedCompilerFactory._TypeCompiler;
			}
		}

		// Token: 0x17000371 RID: 881
		// (get) Token: 0x06000CAE RID: 3246 RVA: 0x0001FB5F File Offset: 0x0001EB5F
		internal static _ICompilerMessageCreator _CompilerMessageCreator
		{
			get
			{
				return VersionedCompilerFactory._CompilerMessageCreator;
			}
		}

		// Token: 0x17000372 RID: 882
		// (get) Token: 0x06000CAF RID: 3247 RVA: 0x0001FB66 File Offset: 0x0001EB66
		internal static ILMSerializationService SerializationService_OrNull
		{
			get
			{
				return VersionedCompilerFactory._SerializationService_OrNull;
			}
		}

		// Token: 0x17000373 RID: 883
		// (get) Token: 0x06000CB0 RID: 3248 RVA: 0x0001FB6D File Offset: 0x0001EB6D
		internal static IComparisonService ComparisonService
		{
			get
			{
				return VersionedCompilerFactory._ComparisonService;
			}
		}

		// Token: 0x17000374 RID: 884
		// (get) Token: 0x06000CB1 RID: 3249 RVA: 0x0001FB74 File Offset: 0x0001EB74
		internal static IGreenTreeConverter GreenTreeConverter_OrNull
		{
			get
			{
				return VersionedCompilerFactory._GreenTreeConverter_OrNull;
			}
		}

		// Token: 0x17000375 RID: 885
		// (get) Token: 0x06000CB2 RID: 3250 RVA: 0x0001FB7B File Offset: 0x0001EB7B
		internal static IParseTreeService ParseTreeService
		{
			get
			{
				return VersionedCompilerFactory.ParseTreeService;
			}
		}

		// Token: 0x17000376 RID: 886
		// (get) Token: 0x06000CB3 RID: 3251 RVA: 0x0001FB82 File Offset: 0x0001EB82
		internal static IStringEncodingService StringEncodingService
		{
			get
			{
				return VersionedCompilerFactory._StringEncodingService;
			}
		}

		// Token: 0x06000CB4 RID: 3252 RVA: 0x0001FB89 File Offset: 0x0001EB89
		internal static _IType CheckType(_IType type, IScope scope, _ICompileContext comcon, ISourcePosition sp, _ISignature signDecl, ref IExpression expInitial)
		{
			return CompilerProxy._TypeCompiler.CheckType(type, scope, comcon, sp, signDecl, ref expInitial);
		}

		// Token: 0x06000CB5 RID: 3253 RVA: 0x0001FB9D File Offset: 0x0001EB9D
		internal static bool ParseAttributePragma(IPragmaStatement pragma, out string name, out string value)
		{
			return CompilerProxy._Helper.ParseAttributePragma(pragma, out name, out value);
		}

		// Token: 0x06000CB6 RID: 3254 RVA: 0x0001FBAC File Offset: 0x0001EBAC
		public static IEnumerable<KeyValuePair<string, string>> GetAttributesDefinedAtTopOfSequence(ISequenceStatement stmt)
		{
			return CompilerProxy._Helper.GetAttributesDefinedAtTopOfSequence(stmt);
		}

		// Token: 0x06000CB7 RID: 3255 RVA: 0x0001FBB9 File Offset: 0x0001EBB9
		internal static ICheckSumVisitor CreateChecksumVisitor(bool bNoInit)
		{
			return CompilerProxy._Helper.CreateChecksumVisitor(bNoInit);
		}

		// Token: 0x06000CB8 RID: 3256 RVA: 0x0001FBC6 File Offset: 0x0001EBC6
		internal static _IVariableDeclarationChecksumGenerator CreateVariableDeclarationChecksumGenerator()
		{
			return CompilerProxy._Helper.CreateVariableDeclarationChecksumGenerator();
		}

		// Token: 0x06000CB9 RID: 3257 RVA: 0x0001FBD2 File Offset: 0x0001EBD2
		internal static IPrecompileChecker CreatePrecompileChecker(_ISignature signature, int nProjectHandle, _IPreCompileContext precomLocal, bool bTrackCrossReferences)
		{
			return CompilerProxy._Helper.CreatePrecompileChecker(signature, nProjectHandle, precomLocal, bTrackCrossReferences);
		}

		// Token: 0x06000CBA RID: 3258 RVA: 0x0001FBE2 File Offset: 0x0001EBE2
		internal static bool Compile(Guid guidApplication, IProgressCallback callback, bool bCheckAll, bool bKeepCompileInformation)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.DelayedLoader.CompleteLanguageModel(callback);
			return CompilerProxy._Compiler.Compile(guidApplication, callback, bCheckAll, bKeepCompileInformation);
		}

		// Token: 0x06000CBB RID: 3259 RVA: 0x0001FC08 File Offset: 0x0001EC08
		internal static bool Compile(_IPreCompileContext comconPrecompiled, Guid guidApplication, bool bOnlineChange, bool bBootProject, bool bDoTypification, out bool bUpToDate, out _ICompileContext comconNew, IProgressCallback callback, bool bCheckAll, bool bKeepCompileInformation)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.DelayedLoader.CompleteLanguageModel(callback);
			return CompilerProxy._Compiler.Compile(comconPrecompiled, guidApplication, bOnlineChange, bBootProject, bDoTypification, out bUpToDate, out comconNew, callback, bCheckAll, bKeepCompileInformation);
		}

		// Token: 0x06000CBC RID: 3260 RVA: 0x0001FC45 File Offset: 0x0001EC45
		internal static bool GenerateCode(Guid guidApplication, bool bOnlineChange, bool bBootProject, bool bKeepCompileInformation, out IOnlineChangeDetails ocd, out IMessage[] errors, out IMessage[] warnings)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.DelayedLoader.CompleteLanguageModel(null);
			return CompilerProxy._Compiler.GenerateCode(guidApplication, bOnlineChange, bBootProject, bKeepCompileInformation, out ocd, out errors, out warnings);
		}

		// Token: 0x06000CBD RID: 3261 RVA: 0x0001FC70 File Offset: 0x0001EC70
		internal static ICodegenerator CreateCodegenerator(Guid guidDevice, Guid guidApplication, bool bSimulationMode, bool bKeepCompileInformation)
		{
			return CompilerProxy._Compiler.CreateCodegenerator(guidDevice, guidApplication, bSimulationMode, bKeepCompileInformation);
		}

		// Token: 0x06000CBE RID: 3262 RVA: 0x0001FC80 File Offset: 0x0001EC80
		internal static _ICheckerThread GetCheckerThread()
		{
			return CompilerProxy._Helper.GetCheckerThread();
		}

		// Token: 0x06000CBF RID: 3263 RVA: 0x0001FC8C File Offset: 0x0001EC8C
		internal static IPrecompileScope CreatePrecompileScope(_IPreCompileContext precomLocal, _ISignature signature)
		{
			return CompilerProxy._Helper.CreatePrecompileScope(precomLocal, signature);
		}

		// Token: 0x06000CC0 RID: 3264 RVA: 0x0001FC9A File Offset: 0x0001EC9A
		internal static IPrecompileScope CreatePrecompileScope(Guid applicationGuid, _IPreCompileContext precomLocal, _ISignature signature)
		{
			return CompilerProxy._Helper.CreatePrecompileScope(applicationGuid, precomLocal, signature);
		}

		// Token: 0x06000CC1 RID: 3265 RVA: 0x0001FCA9 File Offset: 0x0001ECA9
		internal static _IPrecompileScope CreatePrecompileScope(_ISignature sign, _IPreCompileContext precomLocal, _IPreCompileContext precomPool)
		{
			return CompilerProxy._Helper.CreatePrecompileScope(sign, precomLocal, precomPool);
		}

		// Token: 0x06000CC2 RID: 3266 RVA: 0x0001FCB8 File Offset: 0x0001ECB8
		internal static _IPrecompileScope CreatePrecompileScope(int Pointersize, _ILibraryTable libtable, _ISignature sign, _IPreCompileContext precomLocal, _IPreCompileContext precomPool)
		{
			return CompilerProxy._Helper.CreatePrecompileScope(Pointersize, libtable, sign, precomLocal, precomPool);
		}

		// Token: 0x06000CC3 RID: 3267 RVA: 0x0001FCCA File Offset: 0x0001ECCA
		internal static ChecksumStream CreateChecksumStream(bool bCompilerVersionDependent)
		{
			return CompilerProxy._Helper.CreateChecksumStream(bCompilerVersionDependent);
		}

		// Token: 0x06000CC4 RID: 3268 RVA: 0x0001FCD7 File Offset: 0x0001ECD7
		internal static IIdentifierInfo[] FindSubelements(_IPreCompileContext precom, Guid guidSignature, string stAccessPathOrType, FindSubelementsFlags flags, out bool bError)
		{
			return CompilerProxy._Helper.FindSubelements(precom, guidSignature, stAccessPathOrType, flags, out bError);
		}

		// Token: 0x06000CC5 RID: 3269 RVA: 0x0001FCE9 File Offset: 0x0001ECE9
		internal static IExpressionInfo GetExpressionInfo(_IPreCompileContext precom, Guid guidSignature, string stExpression)
		{
			return CompilerProxy._Helper.GetExpressionInfo(precom, guidSignature, stExpression);
		}

		// Token: 0x06000CC6 RID: 3270 RVA: 0x0001FCF8 File Offset: 0x0001ECF8
		internal static IExpressionInfo GetExpressionInfo(_IPreCompileContext precom, Guid guidSignature, string stExpression, bool bImplicit)
		{
			ICompilerHelper5 compilerHelper = CompilerProxy._Helper as ICompilerHelper5;
			if (compilerHelper != null)
			{
				return compilerHelper.GetExpressionInfo(precom, guidSignature, stExpression, bImplicit);
			}
			return null;
		}

		// Token: 0x06000CC7 RID: 3271 RVA: 0x0001FD1F File Offset: 0x0001ED1F
		internal static IIdentifierInfo[] GetIdentifierInfo(_IPreCompileContext precom, Guid guidSignature, string stAccessPath)
		{
			return CompilerProxy._Helper.GetIdentifierInfo(precom, guidSignature, stAccessPath);
		}

		// Token: 0x06000CC8 RID: 3272 RVA: 0x0001FD2E File Offset: 0x0001ED2E
		internal static IIdentifierInfo[] GetIdentifierInfoFast(_IPreCompileContext precom, Guid guidSignature, string stAccessPath)
		{
			return CompilerProxy._Helper.GetIdentifierInfoFast(precom, guidSignature, stAccessPath);
		}

		// Token: 0x06000CC9 RID: 3273 RVA: 0x0001FD3D File Offset: 0x0001ED3D
		internal static IEnumerable<IDeclarationInfo> ParseForUnknownIdentifiers(_IPreCompileContext precom, string stCode, string stPOUName, string stSubObjectName)
		{
			return CompilerProxy._Helper.ParseForUnknownIdentifiers(precom, stCode, stPOUName, stSubObjectName);
		}

		// Token: 0x06000CCA RID: 3274 RVA: 0x0001FD4D File Offset: 0x0001ED4D
		internal static ILMPreCompileTypifier CreatePreCompileTypifier(Guid guidApplication)
		{
			if (CompilerProxy._Helper is ICompilerHelper2)
			{
				return (CompilerProxy._Helper as ICompilerHelper2).CreatePreCompileTypifier(guidApplication);
			}
			return null;
		}

		// Token: 0x06000CCB RID: 3275 RVA: 0x0001FD70 File Offset: 0x0001ED70
		internal static IEnumerable<ISourcePosition> GetUnusedStatementPositions(Guid guidApplication, ISignature signature)
		{
			ICompilerHelper4 compilerHelper = CompilerProxy._Helper as ICompilerHelper4;
			if (compilerHelper != null)
			{
				return compilerHelper.GetUnusedStatementPositions(guidApplication, signature);
			}
			return Enumerable.Empty<ISourcePosition>();
		}

		// Token: 0x06000CCC RID: 3276 RVA: 0x0001FD9C File Offset: 0x0001ED9C
		internal static IEnumerable<ISourcePosition> GetUnusedStatementPositions(Guid guidApplication, ISignature signature, EPouScopeFlags eForWhichScope)
		{
			ICompilerHelper5 compilerHelper = CompilerProxy._Helper as ICompilerHelper5;
			if (compilerHelper != null)
			{
				return compilerHelper.GetUnusedStatementPositions(guidApplication, signature, eForWhichScope);
			}
			return Array.Empty<ISourcePosition>();
		}

		// Token: 0x06000CCD RID: 3277 RVA: 0x0001FDC6 File Offset: 0x0001EDC6
		internal static IIdentifierInfo[] GetIdentifierInfoAtSourcePosition(string stName, ISourcePosition sourcepos, WhatToFind what)
		{
			return CompilerProxy._Helper.GetIdentifierInfoAtSourcePosition(stName, sourcepos, what);
		}

		// Token: 0x06000CCE RID: 3278 RVA: 0x0001FDD5 File Offset: 0x0001EDD5
		internal static IExprement FindExpressionAtSourcePosition(ISourcePosition sourcepos, WhatToFind what, out IPreCompileContext precom)
		{
			return CompilerProxy._Helper.FindExpressionAtSourcePosition(sourcepos, what, out precom);
		}

		// Token: 0x06000CCF RID: 3279 RVA: 0x0001FDE4 File Offset: 0x0001EDE4
		internal static IDownloadInfo GetDownloadInfo(Guid guidApplication, bool bOnlineChange, bool bBootProject, bool bOfflineBootProject)
		{
			return CompilerProxy._Compiler.GetDownloadInfo(guidApplication, bOnlineChange, bBootProject, bOfflineBootProject);
		}

		// Token: 0x06000CD0 RID: 3280 RVA: 0x0001FDF4 File Offset: 0x0001EDF4
		internal static IDownloadInfo GetRelocatedDownloadInfo(Guid guidApplication, bool bOnlineChange, bool bBootProject, bool bOfflineBootProject, int[] nAreaMapping)
		{
			if (CompilerProxy._Compiler is ICompiler3)
			{
				return (CompilerProxy._Compiler as ICompiler3).GetRelocatedDownloadInfo(guidApplication, bOnlineChange, bBootProject, bOfflineBootProject, nAreaMapping);
			}
			throw new NotSupportedException("Individually relocated code is not supported by current compiler version.");
		}

		// Token: 0x06000CD1 RID: 3281 RVA: 0x0001FE22 File Offset: 0x0001EE22
		internal static void AddImplicitMethods(_ISignature sign, _ISignature signOld, _ICompileContext comconNew, _ICompileContext comconRef)
		{
			CompilerProxy._Compiler.AddImplicitMethods(sign, signOld, comconNew, comconRef);
		}

		// Token: 0x06000CD2 RID: 3282 RVA: 0x0001FE34 File Offset: 0x0001EE34
		internal static void AddInstVarsToParent(_ISignature sign, _ICompileContext comconNew, _ICompileContext comconRef)
		{
			ICompilerHandleInstanceVariables compilerHandleInstanceVariables = CompilerProxy._Compiler as ICompilerHandleInstanceVariables;
			if (compilerHandleInstanceVariables != null)
			{
				compilerHandleInstanceVariables.AddInstVarsToParent(sign, comconNew, comconRef);
				return;
			}
			CompilerProxy.DummyCompatibilityCompiler.AddInstVarsToParentSign(sign, comconNew, comconRef);
		}

		// Token: 0x06000CD3 RID: 3283 RVA: 0x0001FE61 File Offset: 0x0001EE61
		internal static void AddInterfaceUnion(_ICompileContext comconNew, _ISignature sign, _ICompileContext comconOld)
		{
			CompilerProxy._Compiler.AddInterfaceUnion(comconNew, sign, comconOld);
		}

		// Token: 0x06000CD4 RID: 3284 RVA: 0x0001FE70 File Offset: 0x0001EE70
		internal static void AddImplicitToStringFunction(_ICompileContext comconNew, _ISignature sign, _ICompileContext comconOld)
		{
			CompilerProxy._Compiler.AddImplicitToStringFunction(comconNew, sign, comconOld);
		}

		// Token: 0x06000CD5 RID: 3285 RVA: 0x0001FE7F File Offset: 0x0001EE7F
		internal static IApplicationContent GetApplicationContent(_ICompileContext comcon)
		{
			return CompilerProxy._Compiler.GetApplicationContent(comcon);
		}

		// Token: 0x06000CD6 RID: 3286 RVA: 0x0001FE8C File Offset: 0x0001EE8C
		internal static IApplicationContent BuildApplicationContentFromUpload(byte[] bytes, bool bIsMotorolaByteOrder, bool bByteSupport)
		{
			return CompilerProxy._Compiler.BuildApplicationContentFromUpload(bytes, bIsMotorolaByteOrder, bByteSupport);
		}

		// Token: 0x06000CD7 RID: 3287 RVA: 0x0001FE9B File Offset: 0x0001EE9B
		internal static IList<_ICompilerMessage> GetGlobalErrors(_ICompileContext comconNew)
		{
			return CompilerProxy._Compiler.GetGlobalErrors(comconNew);
		}

		// Token: 0x06000CD8 RID: 3288 RVA: 0x0001FEA8 File Offset: 0x0001EEA8
		internal static string GetStringOfMessageId(MessageId nId)
		{
			return CompilerProxy._Compiler.GetStringOfMessageId(nId);
		}

		// Token: 0x06000CD9 RID: 3289 RVA: 0x0001FEB5 File Offset: 0x0001EEB5
		internal static bool MessageOutput(_ICompileContext comcon, IMessageStorage messagestorage, CompilerMessageCategory cmc)
		{
			return CompilerProxy._Compiler.MessageOutput(comcon, messagestorage, cmc);
		}

		// Token: 0x06000CDA RID: 3290 RVA: 0x0001FEC4 File Offset: 0x0001EEC4
		public static void AddLateLanguageModelForPOU(_ICompileContext comconNew, ILMPOU lmpou, _ICompileContext comconRef, ref bool bRet, bool bTestOnly, bool bBootProject)
		{
			CompilerProxy._Compiler.AddLateLanguageModelForPOU(comconNew, lmpou, comconRef, ref bRet, bTestOnly, bBootProject);
		}

		// Token: 0x06000CDB RID: 3291 RVA: 0x0001FED8 File Offset: 0x0001EED8
		public static void AddLateLanguageModelForGVL(_ICompileContext comconNew, ILMGlobVarlist lmgvl, _ICompileContext comconRef, ref bool bRet, bool bTestOnly, bool bBootProject)
		{
			CompilerProxy._Compiler.AddLateLanguageModelForGVL(comconNew, lmgvl, comconRef, ref bRet, bTestOnly, bBootProject);
		}

		// Token: 0x06000CDC RID: 3292 RVA: 0x0001FEEC File Offset: 0x0001EEEC
		public static void AddLateLanguageModelForDUT(_ICompileContext comconNew, ILMDataType lmdut, _ICompileContext comconRef, ref bool bRet, bool bTestOnly, bool bBootProject)
		{
			CompilerProxy._Compiler.AddLateLanguageModelForDUT(comconNew, lmdut, comconRef, ref bRet, bTestOnly, bBootProject);
		}

		// Token: 0x06000CDD RID: 3293 RVA: 0x0001FF00 File Offset: 0x0001EF00
		public static string DumpExprement(_IExprement expr)
		{
			return CompilerProxy._Compiler.DumpExprement(expr);
		}

		// Token: 0x06000CDE RID: 3294 RVA: 0x0001FF10 File Offset: 0x0001EF10
		public static IMessage[] GetExprementMessages(_IExprement expr)
		{
			return CompilerProxy._Compiler.GetExprementMessages(expr);
		}

		// Token: 0x06000CDF RID: 3295 RVA: 0x0001FF2C File Offset: 0x0001EF2C
		public static IMessage[] GetPOUMessages(_ICompiledPOU cpou, bool bVisitErrorStatements)
		{
			return CompilerProxy._Compiler.GetPOUMessages(cpou, bVisitErrorStatements);
		}

		// Token: 0x06000CE0 RID: 3296 RVA: 0x0001FF48 File Offset: 0x0001EF48
		public static IEnumerable<_ICompilerMessage> GetPOUMessages(_ICompiledPOU cpou, bool bVisitErrorStatements, bool bVisitErrorStatementsInConditionalPragmas)
		{
			ICompiler2 compiler = CompilerProxy._Compiler as ICompiler2;
			if (compiler != null)
			{
				return compiler.GetPOUMessages(cpou, bVisitErrorStatements, bVisitErrorStatementsInConditionalPragmas);
			}
			return CompilerProxy._Compiler.GetPOUMessages(cpou, bVisitErrorStatements);
		}

		// Token: 0x06000CE1 RID: 3297 RVA: 0x0001FF79 File Offset: 0x0001EF79
		public static void TypifyExprement(IExprement expr, IScope scope, _ICompileContext comcon, ICompiledType ctypeExpected, bool bInterpretPragmas, bool bContributeToCompile, _ICompiledPOU cpou)
		{
			CompilerProxy._Compiler.TypifyExprement(expr, scope, comcon, ctypeExpected, bInterpretPragmas, bContributeToCompile, cpou);
		}

		// Token: 0x06000CE2 RID: 3298 RVA: 0x0001FF90 File Offset: 0x0001EF90
		public static void TypifyExprement(IExprement expr, IScope scope, _ICompileContext comcon, ICompiledType ctypeExpected, bool bInterpretPragmas, bool bContributeToCompile, bool bTreatReferenceAsPointer, _ICompiledPOU cpou)
		{
			CompilerProxy._Compiler.TypifyExprement(expr, scope, comcon, ctypeExpected, bInterpretPragmas, bContributeToCompile, bTreatReferenceAsPointer, cpou);
		}

		// Token: 0x06000CE3 RID: 3299 RVA: 0x0001FFB3 File Offset: 0x0001EFB3
		public static _ICompilerMessage[] TypifyAndCheckExprement(_IExprement expr, IScope scope, _ICompileContext comcon)
		{
			return CompilerProxy._Compiler.TypifyAndCheckExprement(expr, scope, comcon);
		}

		// Token: 0x06000CE4 RID: 3300 RVA: 0x0001FFC2 File Offset: 0x0001EFC2
		internal static bool MakeImplicitConversionIfNecessary(ICompiledType typeSource, ICompiledType typeDest, ref IExpression exprToConvert)
		{
			return CompilerProxy._Compiler.MakeImplicitConversionIfNecessary(typeSource, typeDest, ref exprToConvert);
		}

		// Token: 0x06000CE5 RID: 3301 RVA: 0x0001FFD1 File Offset: 0x0001EFD1
		public static void CheckSyntax(_ICompiledPOU cpou)
		{
			CompilerProxy._Compiler.CheckSyntax(cpou);
		}

		// Token: 0x06000CE6 RID: 3302 RVA: 0x0001FFDE File Offset: 0x0001EFDE
		public static bool AddTemporaryVariableAfterCompile(_ICompileContext comcon, ISignature signToModify, IExprementPosition pos, string stVariableName, ICompiledType ctype)
		{
			return CompilerProxy._Compiler.AddTemporaryVariableAfterCompile(comcon, signToModify, pos, stVariableName, ctype);
		}

		// Token: 0x06000CE7 RID: 3303 RVA: 0x0001FFF0 File Offset: 0x0001EFF0
		public static IExpressionTypifier CreateTypifier(IScope scope, _ICompileContext comcon, ICompiledType ctypeExpected, bool bInterpretPragmas, bool bContributeToCompile, bool bTreatReferenceAsPointer, _ICompiledPOU cpou)
		{
			return CompilerProxy._Compiler.CreateTypifier(scope, comcon, ctypeExpected, bInterpretPragmas, bContributeToCompile, bTreatReferenceAsPointer, cpou);
		}

		// Token: 0x06000CE8 RID: 3304 RVA: 0x00020006 File Offset: 0x0001F006
		public static IExpressionTypifier CreateTypifier(int idLocalSignature, _ICompileContext comcon, ICompiledType ctypeExpected, bool bInterpretPragmas, bool bContributeToCompile, bool bTreatReferenceAsPointer, _ICompiledPOU cpou)
		{
			return CompilerProxy._Compiler.CreateTypifier(idLocalSignature, comcon, ctypeExpected, bInterpretPragmas, bContributeToCompile, bTreatReferenceAsPointer, cpou);
		}

		// Token: 0x06000CE9 RID: 3305 RVA: 0x0002001C File Offset: 0x0001F01C
		internal static ISymbolTables CreatePrecompileSymbolTables(_ICompileContext comcon)
		{
			return CompilerProxy._Compiler.CreatePrecompileSymbolTables(comcon);
		}

		// Token: 0x06000CEA RID: 3306 RVA: 0x00020029 File Offset: 0x0001F029
		internal static void InitCodegenerator(ICodegenerator codegen, _ICompileContext comcon, ITargetSettings targetSettings)
		{
			CompilerProxy._Compiler.InitCodegenerator(codegen, comcon, targetSettings);
		}

		// Token: 0x06000CEB RID: 3307 RVA: 0x00020038 File Offset: 0x0001F038
		internal static void StartCompilation()
		{
			CompilerProxy._Compiler.StartCompilation();
		}

		// Token: 0x06000CEC RID: 3308 RVA: 0x00020044 File Offset: 0x0001F044
		internal static byte[] GetTaskIds(IVariable var, ISignature signDecl, bool bWriteOnly, ICompileContext comcon)
		{
			return CompilerProxy._Helper.GetTaskIds(var, signDecl, bWriteOnly, comcon);
		}

		// Token: 0x06000CED RID: 3309 RVA: 0x00020054 File Offset: 0x0001F054
		internal static IList<ITaskCrossref> GetTaskRefIds(IVariable var, ISignature signDecl, bool bWriteOnly, bool bDeclarationReferences, ICompileContext comcon)
		{
			return CompilerProxy._Helper.GetTaskRefIds(var, signDecl, bWriteOnly, bDeclarationReferences, comcon);
		}

		// Token: 0x06000CEE RID: 3310 RVA: 0x00020066 File Offset: 0x0001F066
		internal static bool IsPrefixOperator(Operator op)
		{
			return CompilerProxy._Helper.IsPrefixOperator(op);
		}

		// Token: 0x06000CEF RID: 3311 RVA: 0x00020073 File Offset: 0x0001F073
		internal static string GetImplicitInitFunctionName(_ISignature sign)
		{
			return CompilerProxy._Helper.GetImplicitInitFunctionName(sign);
		}

		// Token: 0x06000CF0 RID: 3312 RVA: 0x00020080 File Offset: 0x0001F080
		internal static IScope5 CreateGlobalScope(_ICompileContext comcon)
		{
			return CompilerProxy._Compiler.CreateGlobalScope(comcon);
		}

		// Token: 0x06000CF1 RID: 3313 RVA: 0x0002008D File Offset: 0x0001F08D
		internal static IScope5 CreateScope(_ICompileContext comcon, int nIdLocal, int nIdMethod)
		{
			return CompilerProxy._Compiler.CreateScope(comcon, nIdLocal, nIdMethod);
		}

		// Token: 0x06000CF2 RID: 3314 RVA: 0x0002009C File Offset: 0x0001F09C
		internal static IScope5 CreateScope(_ICompileContext comcon, int nIdLocal)
		{
			return CompilerProxy._Compiler.CreateScope(comcon, nIdLocal);
		}

		// Token: 0x06000CF3 RID: 3315 RVA: 0x000200AA File Offset: 0x0001F0AA
		internal static IScope5 CreateOnlineExpressionScope(_ICompileContext comcon, int nIdLocal)
		{
			return CompilerProxy._Compiler.CreateOnlineExpressionScope(comcon, nIdLocal);
		}

		// Token: 0x06000CF4 RID: 3316 RVA: 0x000200B8 File Offset: 0x0001F0B8
		internal static _IScanner CreateMultiStringScanner(IList<string> strlText, bool bIncludeComments, bool bIncludeEndOfLines, bool bIncludePragmas, bool bIncludeWhitespaces)
		{
			return CompilerProxy._Compiler.CreateMultiStringScanner(strlText, bIncludeComments, bIncludeEndOfLines, bIncludePragmas, bIncludeWhitespaces);
		}

		// Token: 0x06000CF5 RID: 3317 RVA: 0x000200CA File Offset: 0x0001F0CA
		internal static _IScanner CreateScanner()
		{
			return CompilerProxy._Compiler.CreateScanner();
		}

		// Token: 0x06000CF6 RID: 3318 RVA: 0x000200D6 File Offset: 0x0001F0D6
		internal static _IScanner CreateScanner(bool bCompilerVersionDepending)
		{
			return CompilerProxy._Compiler.CreateScanner(bCompilerVersionDepending);
		}

		// Token: 0x06000CF7 RID: 3319 RVA: 0x000200E3 File Offset: 0x0001F0E3
		internal static _IScanner CreateScanner(Version version)
		{
			return CompilerProxy.GetCompilerOrThrowException(version).CreateScanner(version);
		}

		// Token: 0x06000CF8 RID: 3320 RVA: 0x000200F4 File Offset: 0x0001F0F4
		private static ICompiler4 GetCompilerOrThrowException(Version version)
		{
			ICompiler4 compiler = VersionedCompilerFactory.GetCompilerOrNull(version) as ICompiler4;
			if (compiler == null)
			{
				throw new ArgumentException(string.Format("No compiler for version {0} available", version));
			}
			return compiler;
		}

		// Token: 0x06000CF9 RID: 3321 RVA: 0x00020122 File Offset: 0x0001F122
		internal static _IParser CreateParser(string stCode, bool bImplicit)
		{
			return CompilerProxy._Compiler.CreateParser(stCode, bImplicit);
		}

		// Token: 0x06000CFA RID: 3322 RVA: 0x00020130 File Offset: 0x0001F130
		internal static _IParser CreateParser(string stCode)
		{
			return CompilerProxy._Compiler.CreateParser(stCode);
		}

		// Token: 0x06000CFB RID: 3323 RVA: 0x0002013D File Offset: 0x0001F13D
		internal static _IParser CreateParser(IScanner scanner, bool bImplicit)
		{
			return CompilerProxy._Compiler.CreateParser(scanner, bImplicit);
		}

		// Token: 0x06000CFC RID: 3324 RVA: 0x0002014B File Offset: 0x0001F14B
		internal static _IParser CreateParser(IScanner scanner, bool bImplicit, Version version, ILMCompileOptions3 compileOptions)
		{
			return CompilerProxy.GetCompilerOrThrowException(version).CreateParser(scanner, bImplicit, version, compileOptions);
		}

		// Token: 0x06000CFD RID: 3325 RVA: 0x0002015C File Offset: 0x0001F15C
		internal static _IParser CreateParser(IScanner scanner)
		{
			return CompilerProxy._Compiler.CreateParser(scanner);
		}

		// Token: 0x06000CFE RID: 3326 RVA: 0x00020169 File Offset: 0x0001F169
		internal static IList<ICodePosition> FindCrossReferences(_IStatement state, int nSignatureId, int nVariableId)
		{
			return CompilerProxy._Compiler.FindCrossReferences(state, nSignatureId, nVariableId);
		}

		// Token: 0x06000CFF RID: 3327 RVA: 0x00020178 File Offset: 0x0001F178
		internal static IList<ICodePosition> FindCrossReferences2(_IExprement expr, int nSignatureId, int nVariableId, AccessFlag acc)
		{
			return CompilerProxy._Compiler.FindCrossReferences2(expr, nSignatureId, nVariableId, acc);
		}

		// Token: 0x06000D00 RID: 3328 RVA: 0x00020188 File Offset: 0x0001F188
		internal static void RemoveStatementComments(_IStatement state)
		{
			CompilerProxy._Compiler.RemoveStatementComments(state);
		}

		// Token: 0x06000D01 RID: 3329 RVA: 0x00020195 File Offset: 0x0001F195
		internal static void ObfuscateComments(_IStatement state)
		{
			CompilerProxy._Compiler.ObfuscateComments(state);
		}

		// Token: 0x06000D02 RID: 3330 RVA: 0x000201A2 File Offset: 0x0001F1A2
		internal static void DeobfuscateComments(_IStatement state)
		{
			CompilerProxy._Compiler.DeobfuscateComments(state);
		}

		// Token: 0x06000D03 RID: 3331 RVA: 0x000201AF File Offset: 0x0001F1AF
		internal static IExprementVisitor CreateFlowTraverser(IFlowPosVisitor fpvis, _IBreakpointList bpl)
		{
			return CompilerProxy._Compiler.CreateFlowTraverser(fpvis, bpl);
		}

		// Token: 0x06000D04 RID: 3332 RVA: 0x000201BD File Offset: 0x0001F1BD
		internal static IStandardTraverser CreateStandardTraverser()
		{
			return CompilerProxy._Compiler.CreateStandardTraverser();
		}

		// Token: 0x06000D05 RID: 3333 RVA: 0x000201C9 File Offset: 0x0001F1C9
		internal static IList<IPrecompilePositionInfo> FindPrecompileCrossReferences(int referencingSignatureId, int referencedSignatureId, int referencedVariableId)
		{
			return CompilerProxy._Compiler.FindPrecompileCrossReferences(referencingSignatureId, referencedSignatureId, referencedVariableId);
		}

		// Token: 0x06000D06 RID: 3334 RVA: 0x000201D8 File Offset: 0x0001F1D8
		internal static IList<IPrecompilePositionInfo> FindPrecompileCrossReferences(_IStatement statement, int referencingSignatureId, int referencedSignatureId, int referencedVariableId, bool ignoreCompoAccessLeftSideCalls)
		{
			return CompilerProxy._Compiler.FindPrecompileCrossReferences(statement, referencingSignatureId, referencedSignatureId, referencedVariableId, ignoreCompoAccessLeftSideCalls);
		}

		// Token: 0x06000D07 RID: 3335 RVA: 0x000201EA File Offset: 0x0001F1EA
		internal static IList<IPrecompilePositionInfo> FindPrecompileCrossReferences(int callerSignatureId, int calleeSignatureId, bool ignoreCompoAccessLeftSideCalls)
		{
			return CompilerProxy._Compiler.FindPrecompileCrossReferences(callerSignatureId, calleeSignatureId, ignoreCompoAccessLeftSideCalls);
		}

		// Token: 0x06000D08 RID: 3336 RVA: 0x000201F9 File Offset: 0x0001F1F9
		internal static void FindCrossReferencesPrecompile(IList<IAccessInfo> alPositions, _ICompiledPOU cpou, string stName, RefType reftype, int nProjectHandle, IPreCompileContext precom)
		{
			CompilerProxy._Compiler.FindCrossReferencesPrecompile(alPositions, cpou, stName, reftype, nProjectHandle, precom);
		}

		// Token: 0x06000D09 RID: 3337 RVA: 0x0002020D File Offset: 0x0001F20D
		internal static void FindCrossReferencesPrecompile(IList<IAccessInfo> alPositions, ISignature sign, string stName, RefType reftype, int nProjectHandle, IPreCompileContext precom)
		{
			CompilerProxy._Compiler.FindCrossReferencesPrecompile(alPositions, sign, stName, reftype, nProjectHandle, precom);
		}

		// Token: 0x06000D0A RID: 3338 RVA: 0x00020221 File Offset: 0x0001F221
		internal static void FindCrossReferencesPrecompile(IDictionary<string, IList<IAccessInfo>> htPositions, _ICompiledPOU cpou, Regex regex, RefType reftype, int nProjectHandle, IPreCompileContext precom)
		{
			CompilerProxy._Compiler.FindCrossReferencesPrecompile(htPositions, cpou, regex, reftype, nProjectHandle, precom);
		}

		// Token: 0x06000D0B RID: 3339 RVA: 0x00020235 File Offset: 0x0001F235
		internal static void FindCrossReferencesPrecompile(IDictionary<string, IList<IAccessInfo>> htPositions, ISignature sign, Regex regex, RefType reftype, int nProjectHandle, IPreCompileContext precom)
		{
			CompilerProxy._Compiler.FindCrossReferencesPrecompile(htPositions, sign, regex, reftype, nProjectHandle, precom);
		}

		// Token: 0x06000D0C RID: 3340 RVA: 0x00020249 File Offset: 0x0001F249
		internal static _IDataManager DuplicateDataManager(_IDataManager datmanSrc)
		{
			return CompilerProxy._Compiler.DuplicateDataManager(datmanSrc);
		}

		// Token: 0x06000D0D RID: 3341 RVA: 0x00020256 File Offset: 0x0001F256
		internal static bool ConfigureMemory(_IDataManager datman, _IMemorySettings memset, IList<_IArea> alAreas, int nFirstArea)
		{
			return CompilerProxy._Compiler.ConfigureMemory(datman, memset, alAreas, nFirstArea);
		}

		// Token: 0x06000D0E RID: 3342 RVA: 0x00020266 File Offset: 0x0001F266
		internal static bool IsEmptyArea(_IDataManager datman, int iAreaIndex)
		{
			return CompilerProxy._Compiler.IsEmptyArea(datman, iAreaIndex);
		}

		// Token: 0x06000D0F RID: 3343 RVA: 0x00020274 File Offset: 0x0001F274
		internal static bool Free(_IDataManager datman, IDataLocation datalocation, int nSize, DataSegmentFlags NOT_USED)
		{
			return CompilerProxy._Compiler.Free(datman, datalocation, nSize, NOT_USED);
		}

		// Token: 0x06000D10 RID: 3344 RVA: 0x00020284 File Offset: 0x0001F284
		internal static bool Allocate(_IDataManager datman, ushort usArea, int iAddress, int iSize, DataSegmentFlags NOT_USED)
		{
			return CompilerProxy._Compiler.Allocate(datman, usArea, iAddress, iSize, NOT_USED);
		}

		// Token: 0x06000D11 RID: 3345 RVA: 0x00020296 File Offset: 0x0001F296
		internal static bool Allocate(_IDataManager datman, ref ushort usArea, ref int iAddress, int iGranularity, int iSize, int iSegmentSize, DataSegmentFlags flags)
		{
			return CompilerProxy._Compiler.Allocate(datman, ref usArea, ref iAddress, iGranularity, iSize, iSegmentSize, flags);
		}

		// Token: 0x06000D12 RID: 3346 RVA: 0x000202AC File Offset: 0x0001F2AC
		internal static ICompiledType GetTypeOfLiteral(ILiteralExpression literal, bool bLRealSupported, bool bTreatLRealAsReal, bool bInt64Supported, bool bTreatInt64AsInt32, bool bUnicodeNotSupported)
		{
			return CompilerProxy._Helper.GetTypeOfLiteral(literal, bLRealSupported, bTreatLRealAsReal, bInt64Supported, bTreatInt64AsInt32, bUnicodeNotSupported);
		}

		// Token: 0x06000D13 RID: 3347 RVA: 0x000202C0 File Offset: 0x0001F2C0
		internal static _ISignature GetPrecompileSignature(_ICompileContext comcon, _ISignature sign, _IPreCompileContext precomp, _IPreCompileContext precompPool, out ISignature[] signSubs, out _IPreCompileContext precomFound)
		{
			return CompilerProxy._Helper.GetPrecompileSignature(comcon, sign, precomp, precompPool, out signSubs, out precomFound);
		}

		// Token: 0x06000D14 RID: 3348 RVA: 0x000202D4 File Offset: 0x0001F2D4
		internal static _ICompiledPOU GetPrecompiledPOU(_ICompileContext comcon, _ICompiledPOU cpou, _IPreCompileContext precomp, _IPreCompileContext precompPool)
		{
			return CompilerProxy._Helper.GetPrecompiledPOU(comcon, cpou, precomp, precompPool);
		}

		// Token: 0x06000D15 RID: 3349 RVA: 0x000202E4 File Offset: 0x0001F2E4
		internal static Version RuntimeVersion(ITargetSettings target)
		{
			return CompilerProxy._Helper.RuntimeVersion(target);
		}

		// Token: 0x06000D16 RID: 3350 RVA: 0x000202F1 File Offset: 0x0001F2F1
		internal static string GetLocalLibraryNamespaceRecursive(_ICompileContext comcon, _IPreCompileContext precom)
		{
			return CompilerProxy._Helper.GetLocalLibraryNamespaceRecursive(comcon, precom);
		}

		// Token: 0x06000D17 RID: 3351 RVA: 0x000202FF File Offset: 0x0001F2FF
		internal static IDataLocation LocateAddress(_ICompileContext comconNew, out IMessage message, out bool bError, ISourcePosition sp, IDirectVariable dirvar, IVariable2 var)
		{
			return CompilerProxy._Helper.LocateAddress(comconNew, out message, out bError, sp, dirvar, var);
		}

		// Token: 0x06000D18 RID: 3352 RVA: 0x00020313 File Offset: 0x0001F313
		internal static string GetDefaultInitializationCode(IVariable var, ISignature sign, IScope scope, string stInstancePath)
		{
			return CompilerProxy._Helper.GetDefaultInitializationCode(var, sign, scope, stInstancePath);
		}

		// Token: 0x06000D19 RID: 3353 RVA: 0x00020323 File Offset: 0x0001F323
		internal static string[] InstancePaths(_ICompileContext comcon, ISignature sign, out IVariable[] varInstances, out ISignature[] declaringSignatures, bool bWithNamespace)
		{
			return CompilerProxy._Helper.InstancePaths(comcon, sign, out varInstances, out declaringSignatures, bWithNamespace);
		}

		// Token: 0x06000D1A RID: 3354 RVA: 0x00020335 File Offset: 0x0001F335
		internal static string[] InstancePaths(_ICompileContext comcon, ISignature sign, out IVariable[] varInstances, out ISignature[] declaringSignatures, bool bWithNamespace, bool bWithStackVariables, bool bWithDerivedClasses)
		{
			return CompilerProxy._Helper.InstancePaths(comcon, sign, out varInstances, out declaringSignatures, bWithNamespace, bWithStackVariables, bWithDerivedClasses);
		}

		// Token: 0x06000D1B RID: 3355 RVA: 0x0002034B File Offset: 0x0001F34B
		internal static IEnumerable<IInstancePathInfo> InstancePaths(_ICompileContext comcon, ISignature sign, bool bWithDerivedClasses)
		{
			return CompilerProxy._Helper.InstancePaths(comcon, sign, bWithDerivedClasses);
		}

		// Token: 0x06000D1C RID: 3356 RVA: 0x0002035A File Offset: 0x0001F35A
		internal static string[] InstancePaths(_ICompileContext comcon, string stSignatureName)
		{
			return CompilerProxy._Helper.InstancePaths(comcon, stSignatureName);
		}

		// Token: 0x06000D1D RID: 3357 RVA: 0x00020368 File Offset: 0x0001F368
		internal static string[] InstancePathsWithPoolNamespace(_ICompileContext comcon, ISignature sign, out IVariable[] varInstances, out ISignature[] declaringSignatures, bool bWithNamespace, bool bWithStackVariables, bool bWithDerivedClasses)
		{
			ICompilerHelper6 compilerHelper = CompilerProxy._Helper as ICompilerHelper6;
			if (compilerHelper != null)
			{
				return compilerHelper.InstancePathsWithPoolNamespace(comcon, sign, out varInstances, out declaringSignatures, bWithNamespace, bWithStackVariables, bWithDerivedClasses);
			}
			return CompilerProxy._Helper.InstancePaths(comcon, sign, out varInstances, out declaringSignatures, bWithNamespace, bWithStackVariables, bWithDerivedClasses);
		}

		// Token: 0x06000D1E RID: 3358 RVA: 0x000203A8 File Offset: 0x0001F3A8
		internal static string ReplacePlaceholders(string stInput, _IExpression exp)
		{
			return CompilerProxy._Helper.ReplacePlaceholders(stInput, exp);
		}

		// Token: 0x06000D1F RID: 3359 RVA: 0x000203B6 File Offset: 0x0001F3B6
		internal static string GetParameterGetFunctionCall(IVariable var, int nBitNr)
		{
			return CompilerProxy._Helper.GetParameterGetFunctionCall(var, nBitNr);
		}

		// Token: 0x06000D20 RID: 3360 RVA: 0x000203C4 File Offset: 0x0001F3C4
		internal static string GetTextOfOperator(Operator op)
		{
			return CompilerProxy._Helper.GetTextOfOperator(op);
		}

		// Token: 0x06000D21 RID: 3361 RVA: 0x000203D1 File Offset: 0x0001F3D1
		internal static void UpdateScannerOperatorTable()
		{
			CompilerProxy._Helper.UpdateScannerOperatorTable();
		}

		// Token: 0x06000D22 RID: 3362 RVA: 0x000203DD File Offset: 0x0001F3DD
		internal static bool PrecompileChecksDone()
		{
			return CompilerProxy._Helper.PrecompileChecksDone();
		}

		// Token: 0x06000D23 RID: 3363 RVA: 0x000203E9 File Offset: 0x0001F3E9
		internal static void AddImplicitPrecompileSignatures(_ISignature sign, _IPreCompileContext precom)
		{
			CompilerProxy._Helper.AddImplicitPrecompileSignatures(sign, precom);
		}

		// Token: 0x06000D24 RID: 3364 RVA: 0x000203F7 File Offset: 0x0001F3F7
		internal static string VersionFreeLibraryPath(string stDisplayName)
		{
			return CompilerProxy._Helper.VersionFreeLibraryPath(stDisplayName);
		}

		// Token: 0x06000D25 RID: 3365 RVA: 0x00020404 File Offset: 0x0001F404
		internal static bool IsNewestLibrary(string stLibrary)
		{
			return CompilerProxy._Helper.IsNewestLibrary(stLibrary);
		}

		// Token: 0x06000D26 RID: 3366 RVA: 0x00020411 File Offset: 0x0001F411
		internal static bool ParseLibraryId(string stLibraryId, out string stTitle, out string stCompany, out Version v)
		{
			return CompilerProxy._Helper.ParseLibraryId(stLibraryId, out stTitle, out stCompany, out v);
		}

		// Token: 0x06000D27 RID: 3367 RVA: 0x00020421 File Offset: 0x0001F421
		internal static bool IsEqualLibraryNoVersion(string stLibraryId1, string stLibraryId2, out Version foundVersion)
		{
			return CompilerProxy._Helper.IsEqualLibraryNoVersion(stLibraryId1, stLibraryId2, out foundVersion);
		}

		// Token: 0x06000D28 RID: 3368 RVA: 0x00020430 File Offset: 0x0001F430
		internal static void HandleInstanceVars(_ICompileContext compileContext, _ISignature sign, _ISignature signOld, _ICompileContext comconOld)
		{
			CompilerProxy._Helper.HandleInstanceVars(compileContext, sign, signOld, comconOld);
		}

		// Token: 0x06000D29 RID: 3369 RVA: 0x00020440 File Offset: 0x0001F440
		internal static _ISourcePosition CreateSourcePosition(int nProjectHandle, Guid objectGuid, long nPosition, short sPositionOffset, short nLength)
		{
			return CompilerProxy._Helper.CreateSourcePosition(nProjectHandle, objectGuid, nPosition, sPositionOffset, nLength);
		}

		// Token: 0x06000D2A RID: 3370 RVA: 0x00020452 File Offset: 0x0001F452
		internal static byte[] GetInitializationBlob(IScope5 scope, bool isMotorolaByteOrder, IVariable var, out IRelocationList2 relocations)
		{
			return CompilerProxy._Helper.GetInitializationBlob(scope, isMotorolaByteOrder, var, out relocations);
		}

		// Token: 0x06000D2B RID: 3371 RVA: 0x00020462 File Offset: 0x0001F462
		internal static IExternalReference[] GetExternalReferences(_ICompileContext comcon, bool bCompactDownload)
		{
			return CompilerProxy._Compiler.GetExternalReferences(comcon, bCompactDownload);
		}

		// Token: 0x06000D2C RID: 3372 RVA: 0x00020470 File Offset: 0x0001F470
		internal static uint? CalculateOCRelevantPreComNamesHash(params _IPreCompileContext[] precoms)
		{
			ICompilerHelper2 compilerHelper = CompilerProxy._Helper as ICompilerHelper2;
			if (compilerHelper == null)
			{
				return null;
			}
			return new uint?(compilerHelper.CalculateOCRelevantPreComNamesHash(precoms));
		}

		// Token: 0x06000D2D RID: 3373 RVA: 0x000204A0 File Offset: 0x0001F4A0
		internal static IEnumerable<uint> CreateChecksumListByVariableOffsetsForSignature(ISignature sign, Guid appGuid)
		{
			ICompilerHelper4 compilerHelper = CompilerProxy._Helper as ICompilerHelper4;
			if (compilerHelper != null)
			{
				return compilerHelper.CreateChecksumListByVariableOffsetsForSignature(sign, appGuid);
			}
			return Array.Empty<uint>();
		}

		// Token: 0x0200029B RID: 667
		private static class DummyCompatibilityCompiler
		{
			// Token: 0x06002B56 RID: 11094 RVA: 0x00073274 File Offset: 0x00072274
			internal static void AddInstVarsToParentSign(_ISignature sign, _ICompileContext comconNew, _ICompileContext comconRef)
			{
				_ISignature isignature = null;
				_ISignature isignature2 = comconNew.GetSignatureById(sign.ParentSignatureId) as _ISignature;
				Debug.Assert(isignature2 != null);
				if (comconRef != null)
				{
					isignature = (comconRef.GetSignatureById(sign.ParentSignatureId) as _ISignature);
				}
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35700)
				{
					foreach (IVariable variable in sign.All)
					{
						if (variable.HasAttribute("instancevar"))
						{
							(variable as _IVariable).SetFlag(VarFlag.AllocateInInstance, true);
						}
					}
				}
				if (isignature2.POUType == Operator.Program && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351430)
				{
					foreach (IVariable variable2 in sign.InstanceLocals)
					{
						((_IVariable)variable2).SetFlag(VarFlag.Static, true);
						((_IVariable)variable2).SetFlag(VarFlag.Absolut, true);
						((_IVariable)variable2).SetFlag(VarFlag.AllocateInInstance, false);
						((_IVariable)variable2).RemoveAttribute(CompileAttributes.ATTRIBUTE_USELOCATION);
					}
					sign.SetFlagInternal(SignatureFlagInternal.ContainsInstanceVars, false);
					return;
				}
				foreach (IVariable variable3 in sign.InstanceLocals)
				{
					int num = -1;
					string text = string.Empty;
					if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351100)
					{
						text = IdentifierConstants.GetImplicitMethodInstVarName351100(isignature2.Name, sign.Name, variable3.Name);
					}
					else
					{
						text = IdentifierConstants.GetImplicitMethodInstVarName(sign.Name, variable3.Name);
					}
					if (isignature != null)
					{
						IVariable variable4 = isignature[text];
						if (variable4 != null)
						{
							num = variable4.Id;
						}
					}
					if (num == -1)
					{
						num = isignature2.NextId;
					}
					_IVariable ivariable = variable3.Duplicate() as _IVariable;
					ivariable.Id = num;
					ivariable.Name = text;
					ivariable.SetFlag(VarFlag.AllocateInInstance, false);
					ivariable.SetFlag(VarFlag.Local | VarFlag.Implicit, true);
					if (isignature2.POUType == Operator.Program)
					{
						ivariable.SetFlag(VarFlag.Absolut, true);
					}
					isignature2.AddVariable(ivariable);
					((_IVariable)variable3).AddAttribute(CompileAttributes.ATTRIBUTE_USELOCATION, ivariable.Name);
				}
			}
		}
	}
}

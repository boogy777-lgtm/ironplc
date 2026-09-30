using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.TargetSettings;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ICompiler
	{
		ITypeTable TypeTable { get; }

		bool MakeImplicitConversionIfNecessary(ICompiledType typeSource, ICompiledType typeDest, ref IExpression exprToConvert);

		void CheckSyntax(_ICompiledPOU cpou);

		bool CheckSignature(_ISignature sign, IScope5 scope, _ICompileContext comcon);

		bool TypifySignature(_ISignature sign, IScope5 scope, _ICompileContext comcon);

		bool Compile(Guid guidApplication, IProgressCallback callback, bool bCheckAll, bool bKeepCompileInformation);

		bool Compile(_IPreCompileContext comconPrecompiled, Guid guidApplication, bool bOnlineChange, bool bBootProject, bool bDoTypification, out bool bUpToDate, out _ICompileContext comconNew, IProgressCallback callback, bool bCheckAll, bool bKeepCompileInformation);

		bool GenerateCode(Guid guidApplication, bool bOnlineChange, bool bBootProject, bool bKeepCompileInformation, out IOnlineChangeDetails ocd, out IMessage[] errors, out IMessage[] warnings);

		ICodegenerator CreateCodegenerator(Guid guidDevice, Guid guidApplication, bool bSimulationMode, bool bKeepCompileInformation);

		bool GenerateCodeForSystemApp(Guid guidApplication, string stLibraryId);

		IDownloadInfo GetDownloadInfo(Guid guidApplication, bool bOnlineChange, bool bBootProject, bool bOfflineBootProject);

		IExternalReference[] GetExternalReferences(_ICompileContext comcon, bool bCompactDownload);

		void AddImplicitMethods(_ISignature sign, _ISignature signOld, _ICompileContext comconNew, _ICompileContext comconRef);

		void AddInterfaceUnion(_ICompileContext comconNew, _ISignature sign, _ICompileContext comconOld);

		void AddImplicitToStringFunction(_ICompileContext comconNew, _ISignature sign, _ICompileContext comconOld);

		IApplicationContent GetApplicationContent(_ICompileContext comcon);

		IApplicationContent BuildApplicationContentFromUpload(byte[] bytes, bool bIsMotorolaByteOrder, bool bByteSupport);

		IList<_ICompilerMessage> GetGlobalErrors(_ICompileContext comconNew);

		string GetStringOfMessageId(MessageId nId);

		bool MessageOutput(_ICompileContext comcon, IMessageStorage messagestorage, IMessageCategory cmc);

		void AddLateLanguageModelForPOU(_ICompileContext comconNew, ILMPOU lmpou, _ICompileContext comconRef, ref bool bRet, bool bTestOnly, bool bBootProject);

		void AddLateLanguageModelForGVL(_ICompileContext comconNew, ILMGlobVarlist lmgvl, _ICompileContext comconRef, ref bool bRet, bool bTestOnly, bool bBootProject);

		void AddLateLanguageModelForDUT(_ICompileContext comconNew, ILMDataType lmdut, _ICompileContext comconRef, ref bool bRet, bool bTestOnly, bool bBootProject);

		string DumpExprement(_IExprement expr);

		_ICompilerMessage[] GetExprementMessages(_IExprement expr);

		_ICompilerMessage[] GetPOUMessages(_ICompiledPOU cpou);

		_ICompilerMessage[] GetPOUMessages(_ICompiledPOU cpou, bool bIncludeErrorStatements);

		void TypifyExprement(IExprement expr, IScope scope, _ICompileContext comcon, ICompiledType ctypeExpected, bool bInterpretPragmas, bool bContributeToCompile, _ICompiledPOU cpou);

		void TypifyExprement(IExprement expr, IScope scope, _ICompileContext comcon, ICompiledType ctypeExpected, bool bInterpretPragmas, bool bContributeToCompile, bool bTreatReferenceAsPointer, _ICompiledPOU cpou);

		void TypifyPOU(ICompiledPOU cpou, IScope scope, _ICompileContext comcon, ICompiledType ctypeExpected, bool bInterpretPragmas, bool bContributeToCompile);

		void TypeCheckExprement(IExprement expr, IScope scope, _ICompileContext comcon);

		void TypeCheckExprement(IExprement expr, IScope scope, _ICompileContext comcon, bool bWriteConstants);

		void TypeCheckPOU(ICompiledPOU cpou, IScope scope, _ICompileContext comcon);

		_ICompilerMessage[] TypifyAndCheckExprement(_IExprement expr, IScope scope, _ICompileContext comcon);

		bool AddTemporaryVariableAfterCompile(_ICompileContext comcon, ISignature signToModify, IExprementPosition pos, string stVariableName, ICompiledType ctype);

		IExpressionTypifier CreateTypifier(IScope scope, _ICompileContext comcon, ICompiledType ctypeExpected, bool bInterpretPragmas, bool bContributeToCompile, bool bTreatReferenceAsPointer, _ICompiledPOU cpou);

		IExpressionTypifier CreateTypifier(int idLocalSignature, _ICompileContext comcon, ICompiledType ctypeExpected, bool bInterpretPragmas, bool bContributeToCompile, bool bTreatReferenceAsPointer, _ICompiledPOU cpou);

		ISymbolTables CreatePrecompileSymbolTables(_ICompileContext comcon);

		void InitCodegenerator(ICodegenerator codegen, _ICompileContext comcon, ITargetSettings targetSettings);

		void StartCompilation();

		IScope5 CreateGlobalScope(_ICompileContext comcon);

		IScope5 CreateScope(_ICompileContext comcon, int nIdLocal, int nIdMethod);

		IScope5 CreateScope(_ICompileContext comcon, int nIdLocal);

		IScope5 CreateScope(_ICompileContext comcon, int nIdLocal, bool bSearchLocal);

		IScope5 CreateOnlineExpressionScope(_ICompileContext comcon, int nIdLocal);

		_IParser CreateParser(string stCode, bool bImplicit);

		_IParser CreateParser(string st);

		_IParser CreateParser(IList<string> strl);

		_IParser CreateParser(IScanner scanner, bool bImplicit);

		_IParser CreateParser(IScanner scanner);

		_IScanner CreateScanner();

		_IScanner CreateScanner(bool bCompilerVersionDepending);

		_IScanner CreateMultiStringScanner(IList<string> strlText, bool bIncludeComments, bool bIncludeEndOfLines, bool bIncludePragmas, bool bIncludeWhitespaces);

		IList<ICodePosition> FindCrossReferences(_IStatement state, int nSignatureId, int nVariableId);

		IList<ICodePosition> FindCrossReferences2(_IExprement expr, int nSignatureId, int nVariableId, AccessFlag acc);

		IList<IPrecompilePositionInfo> FindPrecompileCrossReferences(int referencingSignatureId, int referencedSignatureId, int referencedVariableId);

		IList<IPrecompilePositionInfo> FindPrecompileCrossReferences(_IStatement statement, int referencingSignatureId, int referencedSignatureId, int referencedVariableId, bool ignoreCompoAccessLeftSideCalls);

		IList<IPrecompilePositionInfo> FindPrecompileCrossReferences(int callerSignatureId, int calleeSignatureId, bool ignoreCompoAccessLeftSideCalls);

		void FindCrossReferencesPrecompile(IList<IAccessInfo> alPositions, _ICompiledPOU cpou, string stName, RefType reftype, int nProjectHandle, IPreCompileContext precom);

		void FindCrossReferencesPrecompile(IList<IAccessInfo> alPositions, ISignature sign, string stName, RefType reftype, int nProjectHandle, IPreCompileContext precom);

		void FindCrossReferencesPrecompile(IDictionary<string, IList<IAccessInfo>> htPositions, _ICompiledPOU cpou, Regex regex, RefType reftype, int nProjectHandle, IPreCompileContext precom);

		void FindCrossReferencesPrecompile(IDictionary<string, IList<IAccessInfo>> htPositions, ISignature sign, Regex regex, RefType reftype, int nProjectHandle, IPreCompileContext precom);

		string LoadString(MessageId mid, params object[] args);

		void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args);

		void AddMessageST(_IExprement exp, Severity severity, MessageId nErrorId, params object[] args);

		void AddErrorST(_IExprement exp, IToken tokenPos, MessageId nErrorId, params object[] args);

		void AddMessageST(_IExprement exp, IToken tokenPos, Severity severity, MessageId nErrorId, params object[] args);

		void RemoveStatementComments(_IStatement parseTreeNoComments);

		void ObfuscateComments(_IStatement state);

		void DeobfuscateComments(_IStatement state);

		IExprementVisitor CreateFlowTraverser(IFlowPosVisitor fpvis, _IBreakpointList bpl);

		IStandardTraverser CreateStandardTraverser();

		_IDataManager DuplicateDataManager(_IDataManager datmansrc);

		bool ConfigureMemory(_IDataManager datman, _IMemorySettings memset, IList<_IArea> alAreas, int nFirstArea);

		bool IsEmptyArea(_IDataManager datman, int iAreaIndex);

		bool Free(_IDataManager datman, IDataLocation datalocation, int nSize, DataSegmentFlags NOT_USED);

		bool Allocate(_IDataManager datman, ushort usArea, int iAddress, int iSize, DataSegmentFlags NOT_USED);

		bool Allocate(_IDataManager datman, ref ushort usArea, ref int iAddress, int iGranularity, int iSize, int iSegmentSize, DataSegmentFlags flags);
	}
}

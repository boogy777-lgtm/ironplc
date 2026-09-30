using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.TargetSettings;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ICompilerHelper
	{
		_ISignature GetPrecompileSignature(_ICompileContext comcon, _ISignature sign, _IPreCompileContext precomp, _IPreCompileContext precompPool, out ISignature[] signSubs, out _IPreCompileContext precomFound);

		_ICompiledPOU GetPrecompiledPOU(_ICompileContext comcon, _ICompiledPOU cpou, _IPreCompileContext precomp, _IPreCompileContext precompPool);

		Version RuntimeVersion(ITargetSettings target);

		string GetLocalLibraryNamespaceRecursive(_ICompileContext comcon, _IPreCompileContext precom);

		string GetLocalLibraryNamespaceRecursive(_ICompileContext comcon, _IPreCompileContext precomToFind, _IPreCompileContext precomToSearchIn);

		void GetAllInterfaceIds(IScope scope, ISignature sign, IDictionary<int, int> iids, bool bWithInterfaceBases);

		IList<_ISignature> GetSortedBySlotAttribute(_ICompileContext comcon, string stAttribute, out IList<IExpression> qualified_callees);

		IDataLocation LocateAddress(_ICompileContext comconNew, out IMessage message, out bool bError, ISourcePosition sp, IDirectVariable dirvar, IVariable2 var);

		int GetGranularity(ICompiledType type, int iMinSize, IScope scope);

		string GetDefaultInitializationCode(IVariable var, ISignature sign, IScope scope, string stInstancePath);

		byte[] GetTaskIds(IVariable var, ISignature signDecl, bool bWriteOnly, ICompileContext comcon);

		IList<ITaskCrossref> GetTaskRefIds(IVariable var, ISignature signDecl, bool bWriteOnly, bool bDeclarationReferences, ICompileContext comcon);

		string[] InstancePaths(_ICompileContext comcon, ISignature sign, out IVariable[] varInstances, out ISignature[] declaringSignatures, bool bWithNamespace);

		string[] InstancePaths(_ICompileContext comcon, ISignature sign, out IVariable[] varInstances, out ISignature[] declaringSignatures);

		string[] InstancePaths(_ICompileContext comcon, ISignature sign, out IVariable[] varInstances, out ISignature[] declaringSignatures, bool bWithNamespace, bool bWithStackVariables, bool bWithDerivedClasses);

		IEnumerable<IInstancePathInfo> InstancePaths(_ICompileContext comcon, ISignature sign, bool bWithDerivedClasses);

		string[] InstancePaths(_ICompileContext comcon, string stSignatureName);

		string ReplacePlaceholders(string stInput, _IExpression exp);

		string GetParameterGetFunctionCall(IVariable var, int nBitNr);

		void UpdateScannerOperatorTable();

		ICompiledType GetTypeOfLiteral(ILiteralExpression literal, bool bLRealSupported, bool bTreatLRealAsReal, bool bInt64Supported, bool bTreatInt64AsInt32, bool bUnicodeNotSupported);

		ICheckSumVisitor CreateChecksumVisitor(bool bNoInit);

		_IVariableDeclarationChecksumGenerator CreateVariableDeclarationChecksumGenerator();

		IPrecompileChecker CreatePrecompileChecker(_ISignature signature, int nProjectHandle, _IPreCompileContext precomLocal, bool bTrackCrossReferences);

		IPrecompileScope CreatePrecompileScope(_IPreCompileContext precomLocal, _ISignature signature);

		_IPrecompileScope CreatePrecompileScope(int Pointersize, _ILibraryTable libtable, _ISignature sign, _IPreCompileContext precomLocal, _IPreCompileContext precomPool);

		_IPrecompileScope CreatePrecompileScope(_ISignature sign, _IPreCompileContext precomLocal, _IPreCompileContext precomPool);

		IIdentifierInfo[] FindSubelements(_IPreCompileContext precomLocal, Guid guidSignature, string stAccessPathOrType, FindSubelementsFlags flags, out bool bError);

		IExpressionInfo GetExpressionInfo(_IPreCompileContext precomLocal, Guid guidSignature, string stExpression);

		IIdentifierInfo[] GetIdentifierInfo(_IPreCompileContext precom, Guid guidSignature, string stAccessPath);

		IIdentifierInfo[] GetIdentifierInfoFast(_IPreCompileContext precom, Guid guidSignature, string stAccessPath);

		IEnumerable<IDeclarationInfo> ParseForUnknownIdentifiers(_IPreCompileContext precom, string stCode, string stPOUName, string stSubObjectName);

		_ISignature ParseLocalInterface(string stInterface, bool bImplicit);

		_ISignature ParseLocalInterface(string stInterface);

		_ISignature ParseGlobalInterface(string stName, string stInterface, bool bImplicit);

		ChecksumStream CreateOldChecksumStream();

		ChecksumStream CreateChecksumStream(bool bCompilerVersionDependent);

		_ICheckerThread GetCheckerThread();

		IIdentifierInfo[] GetIdentifierInfoAtSourcePosition(string stName, ISourcePosition sourcepos, WhatToFind what);

		IExprement FindExpressionAtSourcePosition(ISourcePosition sourcepos, WhatToFind what, out IPreCompileContext precom);

		string GetTextOfOperator(Operator op);

		bool IsPrefixOperator(Operator op);

		string GetImplicitInitFunctionName(_ISignature sign);

		bool PrecompileChecksDone();

		void AddImplicitPrecompileSignatures(_ISignature sign, _IPreCompileContext precom);

		[Obsolete("we need to know the Encoding for strings in order to do this use TODO instead")]
		byte[] GetStringBytes(string st, TypeClass tc, bool bMotorola, int nAlignment);

		string VersionFreeLibraryPath(string stDisplayName);

		bool IsNewestLibrary(string stLibrary);

		bool ParseLibraryId(string stLibraryId, out string stTitle, out string stCompany, out Version v);

		bool IsEqualLibraryNoVersion(string stLibraryId1, string stLibraryId2, out Version foundVersion);

		void HandleInstanceVars(_ICompileContext compileContext, _ISignature sign, _ISignature signOld, _ICompileContext comconOld);

		bool ParseAttributePragma(IPragmaStatement pragma, out string name, out string value);

		IEnumerable<KeyValuePair<string, string>> GetAttributesDefinedAtTopOfSequence(ISequenceStatement stmt);

		_ISourcePosition CreateSourcePosition(int nProjectHandle, Guid objectGuid, long nPosition, short sPositionOffset, short nLength);

		byte[] GetInitializationBlob(IScope5 scope, bool isMotorolaByteOrder, IVariable var, out IRelocationList2 relocations);

		IPrecompileScope CreatePrecompileScope(Guid applicationGuid, _IPreCompileContext precomLocal, _ISignature signature);
	}
}

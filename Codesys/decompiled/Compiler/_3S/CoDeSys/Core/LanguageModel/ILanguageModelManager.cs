using System;
using System.Collections;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager
	{
		IPreCompileContext[] PrecompileContexts { get; }

		IPreCompileContext[] LibraryContexts { get; }

		ICompileContext[] CompileContexts { get; }

		ICompileContext[] ReferenceContexts { get; }

		event CompileEventHandler BeforeCompile;

		event CompileEventHandler AfterCompile;

		event CompileEventHandler CodeChanged;

		event EventHandler BeforeClearAll;

		event AddLanguageModelEventHandler AddLateLanguageModel;

		void PutLanguageModel(ILanguageModelProvider lanmodprov, bool bShowSyntaxErrors);

		void RemoveLanguageModelOfObject(int nProjectHandle, Guid objectGuid);

		void RemoveLanguageModelOfProject(string stProjectId);

		bool IsUpToDate(Guid guidApplication, out bool bOnlineChangePossible);

		ISignature[] AllPrecompiledSignatures(bool bWithLibraries, bool bWithResources);

		IPreCompileContext[] AllPreCompileContexts(bool bWithDevices, bool bWithLibraries);

		IScope AllSignatures(bool bCompiled);

		IScope GlobalSignatures(bool bCompiled);

		IScope CreateScope(ISignature isign, ICollection Signatures, Guid guidApplication);

		IScope CreateScope(ISignature sign, Guid guidApplication);

		IAccessInfo[] GetVariableAccess(string stVariableName, bool bCompiled);

		IAccessInfo[] GetPOUAccess(string stPOUName, bool bCompiled);

		IAccessInfo[] GetDirectVariableAccess(IDirectVariable dirvar, bool bCompiled);

		IVariable GetVariable(string stVariableName, bool bCompiled);

		ISignature FindSignature(Guid guidObject, out IPreCompileContext precom);

		ISignature[] FindSignaturesByName(int nProjectHandle, Guid callingObjectGuid, string stName);

		[Obsolete("Use ICompileContext.BreakpointList")]
		IBreakPointTable GetBreakPointTable(string stPOUName);

		IMessage[] GetCompilerMessages(Guid guidApplication);

		void ClearAll();

		void SaveToProject(int nProjectHandle);

		void ForceRebuildAll(Guid guidApplication);

		void ClearDownloadContext(Guid guidApplication);

		void CompileAll();

		bool Compile(Guid guidApplication);

		void UpdateDownloadContext(Guid guidApplication);

		void GetCompiledIds(Guid guidApplication, out Guid guidCodeId, out Guid guidDataId);

		void GetDownloadIds(Guid guidApplication, out Guid guidCodeId, out Guid guidDataId);

		void GetLastDownloadIds(Guid guidApplication, out Guid guidCodeIdLast, out Guid guidDataIdLast);

		IDownloadInfo GetDownloadInfo(Guid guidApplication, bool bOnlineChange, bool bBootProject);

		bool GenerateCode(Guid guidApplication, bool bOnlineChange, bool bKeepCompileInformation);

		IVarRef GetVarReference(Guid guidApplication, string stPOUName, string stExpression);

		IVarRef GetVarReference(Guid guidApplication, string stExpression);

		IVarRef GetVarReference(string stExpression);

		IVarRef[] GetAllVarReferences(string stInstance, long[] alPositionsOfInterest);

		IConverterFromIEC GetConverterFromIEC();

		IConverterToIEC GetConverterToIEC(bool bOmitPrefixesWherePossible, bool bUseShortPrefixes, DisplayMode displayMode);

		bool CanConvertRaw(byte[] raw, IType typeVarRef, Guid guidApplication, ByteOrder byteOrder);

		object ConvertRaw(byte[] raw, IType typeVarRef, Guid guidApplication, ByteOrder byteOrder);

		byte[] ConvertToRaw(object value, IType typeVarRef, Guid guidApplication, ByteOrder byteOrder);

		IScanner CreateScanner(string stText, bool bIncludeComments, bool bIncludeEndOfLines, bool bIncludePragmas, bool bIncludeWhitespaces);

		IParser CreateParser(IScanner scanner);

		IExpressionTypifier CreateTypifier(Guid guidApplication, int idSignature);

		IPreCompileContext GetPrecompileContext(Guid guidApplication);

		ICompileContext GetCompileContext(Guid guidApplication);

		Guid GetCompileContextGuidByName(string stResourceName, string stApplicationName);

		IExprement FindExpressionAtSourcePosition(ISourcePosition sourcepos, WhatToFind whattofind, out IPreCompileContext precom);

		string GetApplicationNameByGuid(Guid guidApplication);

		Guid GetApplicationGuidByName(string stName);
	}
}

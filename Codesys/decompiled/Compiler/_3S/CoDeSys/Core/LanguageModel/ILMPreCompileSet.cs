using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMPreCompileSet : ILMPouSet
	{
		string Namespace { get; }

		string LibraryPath { get; }

		bool QualifiedAccessOnly { get; }

		bool Support32BitOnly { get; }

		bool DeviceApplication { get; }

		IEnumerable<ISignature4> AllSignaturesFlat { get; }

		bool IsEmpty();

		object GetParameterValue(string stLibraryId, string stIdentifier);

		Version CompilerVersionSavedWith();

		IEnumerable<ISignature> GetSubSignatureSet(Guid objectGuid);

		IEnumerable<ISignature> FindSubSignatureSet(string stName);

		ILibraryTable GetLibraryTable(Guid applicationGuid);

		bool IsDefined(string stDefineIdent);

		bool DefineHasValue(string stDefineIdent, string stValue);
	}
}

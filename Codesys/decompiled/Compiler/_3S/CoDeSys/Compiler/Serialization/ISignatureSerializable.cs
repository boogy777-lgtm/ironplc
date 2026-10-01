using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface ISignatureSerializable
	{
		string OrgName { get; }

		_IVirtualFunctionTable _VirtualFunctionTable { get; set; }

		int BaseSignatureId { get; set; }

		int[] InterfaceIds { get; set; }

		int[] DeclarerIds { get; set; }

		int[] ReferencerIds { get; set; }

		int[] CallerIds { get; set; }

		IList<uint> CalleeIdList { get; set; }

		IList<_IVariable> AllVariables { get; set; }

		string[] Attributes { get; }

		ISignature[] SubSignatures { get; }

		int SerializableVariableIdManagement { get; set; }

		uint ChecksumOptionalInputs { get; set; }

		byte[] TaskReferenceList { get; set; }

		long TimeStamp { get; set; }

		SignatureFlag Flags { get; set; }

		int Id { get; set; }

		int ParentSignatureId { get; set; }

		Guid ObjectGuid { get; set; }

		Guid MessageGuid { get; set; }

		Guid ParentObjectGuid { get; set; }

		string LibraryPath { get; set; }

		int Size { get; set; }

		int CalleeSize { get; set; }

		Operator POUType { get; set; }

		uint Checksum { get; set; }

		uint ChecksumNoInit { get; set; }

		int HighestUsedOffset { get; set; }

		_IExpression _NameExpression { get; set; }

		IDataLocation FPDataLocation { get; set; }

		void SetAttributes(IList<KeyValuePair<string, string>> attributes);

		void SetSubSignatures(IList<_ISignature> signatures);

		string GetAttributeValue(string stAttribute);
	}
}

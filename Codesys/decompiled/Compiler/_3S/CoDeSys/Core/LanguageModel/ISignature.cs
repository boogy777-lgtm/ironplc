using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ISignature
	{
		string Name { get; }

		string OrgName { get; }

		[Obsolete("QualifiedNameExpression is no longer used, function will return null. Use NameExpression instead")]
		IQualifiedNameExpression QualifiedName { get; }

		string LibraryPath { get; }

		Operator POUType { get; }

		int Id { get; }

		int ParentSignatureId { get; }

		Guid ObjectGuid { get; set; }

		Guid MessageGuid { get; }

		IMessage[] Messages { get; }

		IVariable this[string stName] { get; }

		IVariable this[int nId] { get; }

		IVariable[] Locals { get; }

		IVariable[] Inputs { get; }

		IVariable[] Outputs { get; }

		IVariable[] InOuts { get; }

		IVariable[] Externals { get; }

		IVariable[] Temps { get; }

		IVariable[] Statics { get; }

		IVariable[] Constant { get; }

		IVariable[] AllInputs { get; }

		IVariable[] AllOutputs { get; }

		ISignature[] SubSignatures { get; }

		IVariable[] All { get; }

		Guid ParentObjectGuid { get; }

		int Size { get; }

		int CalleeSize { get; }

		string[] Attributes { get; }

		[Obsolete("QualifiedNameExpression is no longer used, function will return null. Use BaseExpression instead")]
		IQualifiedNameExpression BaseSignature { get; }

		int BaseSignatureId { get; }

		[Obsolete("QualifiedNameExpression is no longer used, function will return null. Use InterfaceExpressions instead")]
		IQualifiedNameExpression[] Interfaces { get; }

		int[] InterfaceIds { get; }

		IVirtualFunctionTable VirtualFunctionTable { get; }

		IDataLocation FPDataLocation { get; }

		[Obsolete("The timestamp is still valid, but better use ISignature3.Checksum")]
		long TimeStamp { get; }

		int[] DeclarerIds { get; }

		int[] CallerIds { get; }

		int[] CalleeIds { get; }

		byte[] TaskReferenceList { get; }

		ISignature Duplicate();

		ISignature GetSubSignature(string stName);

		ISignature GetSubSignature(int nId);

		bool GetFlag(SignatureFlag sfFlag);

		bool HasAttribute(string stAttribute);

		string GetAttributeValue(string stAttribute);

		bool IsReferencedByTask(byte byTaskIndex);
	}
}

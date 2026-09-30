using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompileContextCommon
	{
		Guid ApplicationGuid { get; }

		ISignature[] GVLSignatures { get; }

		ISignature[] AllSignatures { get; }

		IPreCompileContext[] LibraryContexts { get; }

		ISignature GetSignature(string stName);

		ISignature GetSignature(Guid objectGuid);

		ICompiledPOU GetCompiledPOU(Guid objectGuid);

		ISignature[] GetSubSignatures(Guid objectGuid);
	}
}

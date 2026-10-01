using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMPouSet
	{
		Guid ApplicationGuid { get; }

		IEnumerable<ISignature> GVLSignatureSet { get; }

		IEnumerable<ISignature> SignatureSet { get; }

		IEnumerable<ITaskInfo> TaskSet { get; }

		IMemorySettings MemorySettings { get; }

		ISignature GetSignature(string stName);

		ISignature GetSignature(Guid objectGuid);

		ICompiledPOU GetCompiledPOU(Guid objectGuid);
	}
}

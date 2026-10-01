using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCompiledApplicationQuery4 : ILMCompiledApplicationQuery3, ILMCompiledApplicationQuery2, ILMCompiledApplicationQuery
	{
		ISignature FindPrecompileSignature(ISignature compiledSignature);

		IEnumerable<uint> CreateChecksumListByVariableOffsetsForSignature(ISignature sign);
	}
}

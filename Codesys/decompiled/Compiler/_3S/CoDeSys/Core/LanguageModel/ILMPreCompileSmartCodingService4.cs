using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMPreCompileSmartCodingService4 : ILMPreCompileSmartCodingService3, ILMPreCompileSmartCodingService2, ILMPreCompileSmartCodingService
	{
		IEnumerable<ISourcePosition> GetUnusedStatementPositions(Guid guidApplication, ISignature signature);
	}
}

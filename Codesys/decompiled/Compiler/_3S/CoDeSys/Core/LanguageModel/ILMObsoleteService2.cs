using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMObsoleteService2 : ILMObsoleteService
	{
		IExpressionTypifier CreateTypifier(Guid guidApplication, int idSignature, bool bContributeToCompile, bool bInterpretPragmas);
	}
}

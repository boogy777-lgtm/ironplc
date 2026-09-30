using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ICompilerHelper2 : ICompilerHelper
	{
		uint CalculateOCRelevantPreComNamesHash(params _IPreCompileContext[] precoms);

		ILMPreCompileTypifier CreatePreCompileTypifier(Guid guidApplication);
	}
}

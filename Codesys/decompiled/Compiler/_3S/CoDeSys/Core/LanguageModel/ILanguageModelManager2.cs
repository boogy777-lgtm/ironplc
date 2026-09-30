using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager2 : ILanguageModelManager
	{
		ICompileContext GetReferenceContextIfAvailable(Guid guidApplication);
	}
}

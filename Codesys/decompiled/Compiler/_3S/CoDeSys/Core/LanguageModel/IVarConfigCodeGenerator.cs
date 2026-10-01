using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IVarConfigCodeGenerator
	{
		void GenerateVarConfigCode(Guid guidApplication, ILanguageModelList lmlist);
	}
}

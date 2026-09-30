using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCompileOptions3 : _ICompileOptions2, _ICompileOptions, ILMCompileOptions, ILMCompileOptions2
	{
		string ProjectDefines { get; set; }

		bool ReportCompiledPousDuringIncrementalCompile { get; set; }
	}
}

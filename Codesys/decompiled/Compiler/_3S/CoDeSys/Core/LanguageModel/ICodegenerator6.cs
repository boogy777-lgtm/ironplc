using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICodegenerator6 : ICodegenerator5, ICodegenerator4, ICodegenerator3, ICodegenerator2, ICodegenerator
	{
		bool FPUSupport { get; }

		void GenerateTrySubroutine(ICompiledPOU cpou, ISequenceStatement seq);

		void GenerateFramePointer();
	}
}

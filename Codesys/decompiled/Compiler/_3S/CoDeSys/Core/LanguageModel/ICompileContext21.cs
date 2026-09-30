using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompileContext21 : ICompileContext20, ICompileContext19, ICompileContext18, ICompileContext17, ICompileContext16, ICompileContext15, ICompileContext14, ICompileContext13, ICompileContext12, ICompileContext11, ICompileContext10, ICompileContext9, ICompileContext8, ICompileContext7, ICompileContext6, ICompileContext5, ICompileContext4, ICompileContext3, ICompileContext2, ICompileContext, ICompileContextCommon
	{
		int VectorBlockSize { get; }

		int VectorAlignment { get; }
	}
}

using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ICompileContext2 : _ICompileContext, ICompileContext21, ICompileContext20, ICompileContext19, ICompileContext18, ICompileContext17, ICompileContext16, ICompileContext15, ICompileContext14, ICompileContext13, ICompileContext12, ICompileContext11, ICompileContext10, ICompileContext9, ICompileContext8, ICompileContext7, ICompileContext6, ICompileContext5, ICompileContext4, ICompileContext3, ICompileContext2, ICompileContext, ICompileContextCommon
	{
		uint PrecompileContextNamesChecksum { get; set; }

		IDownloadInfo7 StoredOnlineChangeDownloadInfo { get; set; }
	}
}

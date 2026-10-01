using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDownloadInfo5 : IDownloadInfo4, IDownloadInfo3, IDownloadInfo2, IDownloadInfo
	{
		IDataLocation OnlineChange1Concurrent { get; }

		IDataLocation OnlineChange2Repeatable { get; }
	}
}

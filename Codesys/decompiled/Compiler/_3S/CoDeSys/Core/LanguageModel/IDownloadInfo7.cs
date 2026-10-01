using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDownloadInfo7 : IDownloadInfo6, IDownloadInfo5, IDownloadInfo4, IDownloadInfo3, IDownloadInfo2, IDownloadInfo
	{
		IDataLocation OnlineChangeConcurrentBefore { get; }

		IDataLocation OnlineChangeConcurrentAfter { get; }

		IDataLocation CodeLocationInfo { get; }
	}
}

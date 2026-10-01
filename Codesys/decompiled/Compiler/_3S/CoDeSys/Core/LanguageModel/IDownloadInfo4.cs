using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDownloadInfo4 : IDownloadInfo3, IDownloadInfo2, IDownloadInfo
	{
		IDataLocation SegmentInfoLocation { get; }
	}
}

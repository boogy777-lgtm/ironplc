using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDownloadInfo2 : IDownloadInfo
	{
		IDataLocation TargetInformationLocation { get; }
	}
}

using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IVersionedLicensedSoftwareMetricCheckableByGeneratedCode : ILicensedSoftwareMetricCheckableByGeneratedCode
	{
		uint Version { get; }

		EMetricFlagsDuringDownload Flags { get; }
	}
}

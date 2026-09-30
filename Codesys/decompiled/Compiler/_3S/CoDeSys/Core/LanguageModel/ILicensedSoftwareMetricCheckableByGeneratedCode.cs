using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILicensedSoftwareMetricCheckableByGeneratedCode
	{
		int ProductCode { get; }

		long Value { get; }

		uint ConversionFactor { get; }
	}
}

using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILicensedSoftwareMetricInformationProvider2 : ILicensedSoftwareMetricInformationProvider
	{
		IEnumerable<ILicensedSoftwareMetricCheckableByGeneratedCode> GetMetricsForApplicationWithVersioned(int iProjectHandle, Guid gdApplication);
	}
}

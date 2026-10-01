using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILicensedSoftwareMetricInformationProvider
	{
		IEnumerable<ILicensedSoftwareMetricCheckableByGeneratedCode> GetMetricsForApplication(int iProjectHandle, Guid gdApplication);
	}
}

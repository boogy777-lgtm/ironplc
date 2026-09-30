using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Licensing
{
	[ReleasedInterface]
	public interface ILicensingInitializationReporter
	{
		bool CheckLicense(PlugInInformation plugInInfo);

		bool ReportMissingLicenses(IEnumerable<PlugInInformation> plugInInfos);
	}
}

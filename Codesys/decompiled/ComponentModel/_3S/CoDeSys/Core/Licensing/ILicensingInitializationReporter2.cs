using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Licensing
{
	[ReleasedInterface]
	public interface ILicensingInitializationReporter2 : ILicensingInitializationReporter
	{
		ReporterUsage Usage { get; set; }
	}
}

using _3S.CoDeSys.Core.Components;

namespace CODESYS.Parser
{
	[ReleasedInterface]
	public interface IScannerOptionsService
	{
		void GetScanningOptions(out bool bUnicodeIdentifiers, out bool bSupportNonCompliantIdentifiers);
	}
}

using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IAddressCalculation
	{
		IDataLocation CalculateAddress(IDirectVariable dirvar, out bool bError);
	}
}

using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IAddressInfo2 : IAddressInfo
	{
		int VariableID { get; }

		int SignatureID { get; }
	}
}

using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IAddressCrossReference
	{
		IAddressCodePosition[] Positions { get; }

		int CodeId { get; }
	}
}

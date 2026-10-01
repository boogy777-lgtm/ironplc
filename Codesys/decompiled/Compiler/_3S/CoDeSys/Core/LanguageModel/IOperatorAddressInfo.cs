using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IOperatorAddressInfo : IAddressInfo
	{
		IAddressInfo[] Operands { get; }

		Operator Operator { get; }
	}
}

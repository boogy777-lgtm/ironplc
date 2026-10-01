using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILiteralValue2 : ILiteralValue
	{
		long GetAnyLong(out bool bValid);

		bool IsValueEqual(ILiteralValue literalValue);
	}
}

using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ITypeInfo3 : ITypeInfo2, ITypeInfo
	{
		string GetIecName(TypeClass tc);

		ulong GetTypeRangeHigh(TypeClass tc);

		long GetTypeRangeLow(TypeClass tc);
	}
}

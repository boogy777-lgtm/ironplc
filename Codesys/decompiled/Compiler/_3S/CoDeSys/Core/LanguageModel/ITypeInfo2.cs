using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ITypeInfo2 : ITypeInfo
	{
		int GetSize(TypeClass tc, IScope scope);

		bool IsLInteger(TypeClass tc, IScope scope);
	}
}

using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ITypeInfo4 : ITypeInfo3, ITypeInfo2, ITypeInfo
	{
		bool IsResolvedXType(IType type);
	}
}

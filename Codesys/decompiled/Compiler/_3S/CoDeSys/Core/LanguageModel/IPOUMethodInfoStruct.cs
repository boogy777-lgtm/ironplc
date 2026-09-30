using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPOUMethodInfoStruct : IMethodInfoStruct, IPOUInfoStruct, ICompiledElementInfoStruct
	{
		Operator ParentPOUType { get; }
	}
}

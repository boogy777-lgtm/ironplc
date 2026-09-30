using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IMethodInfoStruct : IPOUInfoStruct, ICompiledElementInfoStruct
	{
		uint ParentPOUIndex { get; }
	}
}

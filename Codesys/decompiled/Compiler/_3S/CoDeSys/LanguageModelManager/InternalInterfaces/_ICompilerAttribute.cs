using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ICompilerAttribute
	{
		string Value { get; }

		string Name { get; }
	}
}

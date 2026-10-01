using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface IBreakpointListSerializable
	{
		int FirstIndex { get; set; }

		int Add(ref _IBreakpoint bp);

		void AfterDeserialize();
	}
}

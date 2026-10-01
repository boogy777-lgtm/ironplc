using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface IMemoryManagerSerializable
	{
		int Size { get; set; }

		int Base { get; set; }

		int Count { get; }

		_IMemManGap this[int i] { get; }

		void AddGap(int nAddress, int nSize);
	}
}

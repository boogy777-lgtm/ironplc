using System.IO;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface ICompiledCodeSerializable
	{
		CompiledCodeFlags Flags { get; set; }

		IDataLocation Location { get; set; }

		IRelocationList RelocationList { get; set; }

		Stream CodeBytes { get; set; }
	}
}

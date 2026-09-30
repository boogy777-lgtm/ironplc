using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface ICompiledCodeSerializable2 : ICompiledCodeSerializable
	{
		int NumCodeBytes { get; }

		void WriteCodeBytesToWriter(BinaryWriter bw);

		void SetCode(Stream stream);
	}
}

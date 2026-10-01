using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedClass]
	public abstract class ChecksumStream : Stream
	{
		public abstract uint Checksum { get; }
	}
}

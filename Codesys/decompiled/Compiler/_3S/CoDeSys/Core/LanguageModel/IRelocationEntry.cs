using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRelocationEntry
	{
		[Obsolete("remove this: we do not know something about offsets (could be more than one)")]
		int Offset { get; set; }

		void AddOffset(int nOffset);

		void Dump(BinaryWriter binwriter);
	}
}

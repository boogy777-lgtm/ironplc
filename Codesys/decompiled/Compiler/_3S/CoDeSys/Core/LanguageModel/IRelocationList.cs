using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRelocationList
	{
		IRelocationList Empty { get; }

		IRelocationAreaList[] RelocationAreaLists { get; }

		IRelocationList Duplicate();

		void Merge(IRelocationList rl);

		void AddOffset(int nOffset);

		void Dump(BinaryWriter bw);
	}
}

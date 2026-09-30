using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompilerVersionManager6 : ICompilerVersionManager5, ICompilerVersionManager4, ICompilerVersionManager3, ICompilerVersionManager2, ICompilerVersionManager
	{
		bool CompilerVersionGreaterEq(ushort Generation, ushort Version, ushort ServicePack, ushort Patch);
	}
}

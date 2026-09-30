using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IVirtualFunctionTable2 : IVirtualFunctionTable
	{
		int GetInterfaceOffsetInInstance(int nInterfaceId, ICompileContext comcon);
	}
}

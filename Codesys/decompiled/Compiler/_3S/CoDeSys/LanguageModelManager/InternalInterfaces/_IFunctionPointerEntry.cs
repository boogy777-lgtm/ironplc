using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IFunctionPointerEntry : IVFTableEntry2, IVFTableEntry
	{
		new string Name { get; set; }

		new int Id { get; set; }
	}
}

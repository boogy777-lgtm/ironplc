using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IMemorySettings2 : _IMemorySettings, IMemorySettings3, IMemorySettings2, IMemorySettings
	{
		bool LateRelocationForFixedAreas { get; set; }
	}
}

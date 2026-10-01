using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IMemManGap : IMemManGap
	{
		new int Offset { get; set; }

		new int Size { get; set; }
	}
}

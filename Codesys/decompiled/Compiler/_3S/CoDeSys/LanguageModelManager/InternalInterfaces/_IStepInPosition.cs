using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IStepInPosition : IStepInPosition2, IStepInPosition
	{
		new KindOfCall KindOfCall { get; set; }
	}
}

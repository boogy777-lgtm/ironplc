using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IDirectLocationInfo
	{
		IDataLocation DatLoc { get; set; }

		IMessage Message { get; set; }

		bool Error { get; set; }
	}
}

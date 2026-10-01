using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ISignatureSupportingFastOnlineChange
	{
		IDataLocation FPDataLocation { get; set; }

		_IVirtualFunctionTable _VirtualFunctionTable { get; set; }
	}
}

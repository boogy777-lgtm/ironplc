using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IDirvarLocationTable
	{
		bool TryGetLocationInfo(IDirectVariable _dirvar, IVariable2 _var, out _IDirectLocationInfo dirlocinfo);

		void AddLocationInfo(IDirectVariable _dirvar, IVariable2 _var, IDataLocation datloc, IMessage message, bool bError);
	}
}

using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IAddressCalculator
	{
		IDataLocation CalculateAddress(out IMessage message, out bool bHandled, out bool bError, ISourcePosition sp, ICompileContext compilecontext, IDirectVariable dirvar, IVariable2 var);
	}
}

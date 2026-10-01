using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IOnlineChangeDetails : IOnlineChangeDetails3, IOnlineChangeDetails2, IOnlineChangeDetails
	{
		new IDictionary<string, IVariableInfo> InterfacesToRelink { get; set; }

		new IDictionary<string, IVariableInfo> InstancesToMove { get; set; }

		new long TotalNumberOfRelinkTests { get; set; }

		IList<IVariable> VariablesWithOnlineChangeFlag { get; }

		void AddVariableInfo(IVariable var, ISignature sign);

		void ResetOnlineChangeFlags();

		void AddResetVariable(_IVariable var);
	}
}

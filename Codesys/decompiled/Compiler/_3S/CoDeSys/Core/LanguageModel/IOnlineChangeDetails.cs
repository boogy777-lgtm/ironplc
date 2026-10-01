using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IOnlineChangeDetails
	{
		bool InterfaceChanged { get; }

		bool CodeChanged { get; }

		List<IVariableInfo> VariablesAffected { get; }
	}
}

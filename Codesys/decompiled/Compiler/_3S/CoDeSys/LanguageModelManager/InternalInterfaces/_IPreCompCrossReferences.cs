using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IPreCompCrossReferences
	{
		IList<IAccessInfo> this[string stName] { get; }

		void Add(string stName, Guid objectGuid, Guid messageGuid);

		void Remove(Guid objectGuid);

		void GetCrossReferences(Regex regex, IDictionary<string, IList<IAccessInfo>> ht);

		IList<IDirectVariableAccess> AllAccesses();
	}
}

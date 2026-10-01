using System.Collections;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILibParameterTable
	{
		IDictionary ParameterTable { get; }

		void AddParameter(string stName, string stValue);
	}
}

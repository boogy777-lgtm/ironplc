using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IMonitoringUtilities
	{
		Guid GetApplicationGuid(string stDevice, string stApplication);

		ICompileContext GetReferenceContext(Guid guidApplication);

		void CheckValidAddress(string stExpression);

		void CheckValidValue(IVarRef varRef, string stValueText, Guid applicationGuid);

		void CheckValidValue(ICompiledType type, string stValueText, Guid applicationGuid);
	}
}

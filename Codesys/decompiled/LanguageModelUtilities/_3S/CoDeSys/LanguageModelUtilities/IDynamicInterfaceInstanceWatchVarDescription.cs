using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IDynamicInterfaceInstanceWatchVarDescription : IReferencedInstanceWatchVarDescription
	{
		void Initialize(Guid gdApplication, string stDevice, string stApplication, ulong ulAddressInstance, bool bAddressChanged, ISignature signInstance, string stInterfaceExpression);

		void Initialize(Guid gdApplication, string stQualifiedApplication, ulong ulAddressInstance, bool bAddressChanged, ISignature signInstance, string stInterfaceExpression);
	}
}

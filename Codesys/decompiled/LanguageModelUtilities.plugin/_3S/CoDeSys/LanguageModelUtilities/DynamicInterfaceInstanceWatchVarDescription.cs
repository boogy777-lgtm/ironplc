using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[TypeGuid("{5580A0BF-599D-4A42-948B-F5063481C693}")]
	public sealed class DynamicInterfaceInstanceWatchVarDescription : ReferencedInstanceWatchVarDescription, IDynamicInterfaceInstanceWatchVarDescription, IReferencedInstanceWatchVarDescription
	{
		private static int s_TempVarCounter;

		public void Initialize(Guid gdApplication, string stDevice, string stApplication, ulong ulAddressInstance, bool bAddressChanged, ISignature signInstance, string stInterfaceExpression)
		{
			Initialize(stDevice, stApplication, ulAddressInstance, signInstance);
			Initialize(gdApplication, bAddressChanged, signInstance, stInterfaceExpression);
		}

		public void Initialize(Guid gdApplication, string stQualifiedApplication, ulong ulAddressInstance, bool bAddressChanged, ISignature signInstance, string stInterfaceExpression)
		{
			Initialize(stQualifiedApplication, ulAddressInstance, signInstance);
			Initialize(gdApplication, bAddressChanged, signInstance, stInterfaceExpression);
		}

		private void Initialize(Guid gdApplication, bool bAddressChanged, ISignature signInstance, string stInterfaceExpression)
		{
			s_TempVarCounter++;
			Initialize(gdApplication, bAddressChanged, signInstance, stInterfaceExpression, s_TempVarCounter);
		}
	}
}

using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[TypeGuid("{C6622B0C-7F4D-4866-99D6-09BA0187394E}")]
	public sealed class PointerInstanceWatchVarDescription : ReferencedInstanceWatchVarDescription, IPointerInstanceWatchVarDescription, IReferencedInstanceWatchVarDescription
	{
		private string _stDereferencedPointer;

		private static int s_TempVarCounter;

		public string DereferencedPointer => _stDereferencedPointer;

		public void Initialize(Guid gdApplication, string stDevice, string stApplication, ulong ulAddressInstance, bool bAddressChanged, ISignature signInstance, string stInstancePath, string stPointerExpression)
		{
			Initialize(stDevice, stApplication, ulAddressInstance, signInstance);
			_stDereferencedPointer = DetermineDereferencedPointerExpression(stPointerExpression, stInstancePath);
			s_TempVarCounter++;
			Initialize(gdApplication, bAddressChanged, signInstance, stPointerExpression, s_TempVarCounter);
		}
	}
}

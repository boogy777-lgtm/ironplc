using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[TypeGuid("{F8538370-5F9E-4799-9494-D28D1EA6CD90}")]
	public sealed class AddressWatchVarDescription : ReferencedInstanceWatchVarDescription, IAddressWatchVarDescription, IReferencedInstanceWatchVarDescription
	{
		private ICompiledType _type;

		private string _stDereferencedPointer;

		private static int s_TempVarCounter;

		public string DereferencedPointer => _stDereferencedPointer;

		public override string InstanceType
		{
			get
			{
				if (_type is IPointerType pointerType)
				{
					return pointerType.Base.ToString();
				}
				return string.Empty;
			}
		}

		public void Initialize(Guid gdApplication, string stDevice, string stApplication, ulong ulAddress, ICompiledType type, string stInstancePath, string stPointerExpression)
		{
			Initialize(stDevice, stApplication, ulAddress, null);
			_type = type;
			_stDereferencedPointer = DetermineDereferencedPointerExpression(stPointerExpression, stInstancePath);
			s_TempVarCounter++;
			Initialize(gdApplication, bAddressChanged: false, type, stPointerExpression, s_TempVarCounter);
		}
	}
}

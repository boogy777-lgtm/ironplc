using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal sealed class DeRefAccessVarRef : IVarRef
	{
		private Guid _guidApplication;

		private DeRefAccessInfo _addressInfo;

		private IExpression _watchExpression;

		public IAddressInfo AddressInfo => _addressInfo;

		public Guid ApplicationGuid => _guidApplication;

		public object ConstantValue => null;

		public ISourcePosition Position => null;

		public IExpression WatchExpression => _watchExpression;

		internal DeRefAccessVarRef(Guid guidApplication, IAddressInfo baseAddressInfo, string stPointerVar, int iSize, ICompiledType pointerType, ICompiledType baseType)
		{
			_guidApplication = guidApplication;
			_addressInfo = new DeRefAccessInfo(baseAddressInfo, iSize, baseType);
			ILanguageModelBuilder9 languageModelBuilder = APEnvironmentFacade.Instance.LMServiceProvider.CreatorService.CreateLanguageModelBuilder() as ILanguageModelBuilder9;
			if (string.IsNullOrEmpty(stPointerVar))
			{
				stPointerVar = "Dummy";
			}
			IExpression expression = languageModelBuilder.CreateVariableExpression(null, stPointerVar);
			languageModelBuilder.SetType(expression, baseType);
			_watchExpression = languageModelBuilder.CreateDeRefAccessExpression(null, expression);
			languageModelBuilder.SetType(_watchExpression, pointerType);
		}

		public bool GetFlag(VarRefFlag vrflag)
		{
			return false;
		}
	}
}

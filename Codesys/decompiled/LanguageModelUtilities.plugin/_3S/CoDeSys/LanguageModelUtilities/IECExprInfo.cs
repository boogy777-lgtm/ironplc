using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class IECExprInfo : IIECExprInfo
	{
		public IType Type { get; }

		public IVariable Var { get; }

		public bool IsBitAccess { get; }

		public bool IsProperty { get; }

		public string PropertyMonitor { get; }

		public IECExprInfo(IType t, IVariable var, bool bBit, bool bProperty, string stPropertyMonitor)
		{
			Type = t;
			Var = var;
			IsBitAccess = bBit;
			IsProperty = bProperty;
			PropertyMonitor = stPropertyMonitor;
		}
	}
}

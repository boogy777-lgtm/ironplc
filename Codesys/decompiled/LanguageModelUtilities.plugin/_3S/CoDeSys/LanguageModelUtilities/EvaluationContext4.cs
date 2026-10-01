using System;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	public class EvaluationContext4 : EvaluationContext3, IEvaluationContext4, IEvaluationContext3, IEvaluationContext2, IEvaluationContext
	{
		private Guid _gdLocalScope;

		public Guid LocalScopeIdentification => _gdLocalScope;

		public EvaluationContext4(int nProj, int nAttrProj, Guid gdScope, Guid gdLocalScope, IGetLibInformation libInfo)
			: base(nProj, nAttrProj, gdScope, libInfo)
		{
			_gdLocalScope = gdLocalScope;
		}

		public EvaluationContext4(int nProj, int nAttrProj, Guid gdApp, Guid gdScope, Guid gdLocalScope, IGetLibInformation libInfo)
			: base(nProj, nAttrProj, gdApp, gdScope, libInfo)
		{
			_gdLocalScope = gdLocalScope;
		}
	}
}

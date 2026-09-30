using System;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	public class EvaluationContext3 : EvaluationContext2, IEvaluationContext3, IEvaluationContext2, IEvaluationContext
	{
		private IGetLibInformation _LibInfo;

		public IGetLibInformation LibInfo
		{
			get
			{
				return _LibInfo;
			}
			set
			{
				_LibInfo = value;
			}
		}

		public EvaluationContext3(int nProj, int nAttrProj, Guid gdScope, IGetLibInformation libInfo)
			: base(nProj, nAttrProj, gdScope)
		{
			_LibInfo = libInfo;
		}

		public EvaluationContext3(int nProj, int nAttrProj, Guid gdApp, Guid gdScope, IGetLibInformation libInfo)
			: base(nProj, nAttrProj, gdApp, gdScope)
		{
			_LibInfo = libInfo;
		}
	}
}

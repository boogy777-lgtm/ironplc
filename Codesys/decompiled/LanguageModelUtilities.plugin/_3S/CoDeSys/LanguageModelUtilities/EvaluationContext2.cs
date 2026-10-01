using System;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	public class EvaluationContext2 : EvaluationContext, IEvaluationContext2, IEvaluationContext
	{
		private int _nProj;

		private int _nAttrProj;

		public int ProjectHandle => _nProj;

		public int AttractingProjectHandle => _nAttrProj;

		public EvaluationContext2(int nProj, int nAttrProj, Guid gdScope)
			: base(Guid.Empty, gdScope)
		{
			_nProj = nProj;
			_nAttrProj = nAttrProj;
		}

		public EvaluationContext2(int nProj, int nAttrProj, Guid gdApp, Guid gdScope)
			: base(gdApp, gdScope)
		{
			_nProj = nProj;
			_nAttrProj = nAttrProj;
		}
	}
}

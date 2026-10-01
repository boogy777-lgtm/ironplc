using System;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	public class EvaluationContext : IEvaluationContext
	{
		private Guid _gdApplication;

		private Guid _gdScope;

		public Guid ScopeIdentification => _gdScope;

		public Guid ApplicationGuid => _gdApplication;

		public EvaluationContext(Guid gdApplication, Guid gdScope)
		{
			_gdApplication = gdApplication;
			_gdScope = gdScope;
		}
	}
}

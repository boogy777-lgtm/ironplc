using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedClass]
	public class ConstantEvaluationException : LanguageModelUtilitiesException
	{
		public ConstantEvaluationException(string stMessage)
			: base(stMessage)
		{
		}
	}
}

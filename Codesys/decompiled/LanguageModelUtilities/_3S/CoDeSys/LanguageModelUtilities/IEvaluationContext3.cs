using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IEvaluationContext3 : IEvaluationContext2, IEvaluationContext
	{
		IGetLibInformation LibInfo { get; set; }
	}
}

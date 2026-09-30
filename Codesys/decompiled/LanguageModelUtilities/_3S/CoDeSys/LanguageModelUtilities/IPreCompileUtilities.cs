using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IPreCompileUtilities
	{
		IEvaluationContext CreateContext(Guid gdApplication, Guid gdScope);

		IConstantEvaluator CreateConstantEvaluator(IEvaluationContext context);

		IArrayDimensionEvaluator CreateArrayDimensionEvaluator(IEvaluationContext context);

		Guid GetDeclaringSignature(IEvaluationContext context, string stAccessPath);
	}
}

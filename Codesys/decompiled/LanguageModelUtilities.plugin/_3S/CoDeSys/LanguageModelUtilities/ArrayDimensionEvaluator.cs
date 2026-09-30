using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	public class ArrayDimensionEvaluator : IArrayDimensionEvaluator
	{
		private IEvaluationContext _context;

		public ArrayDimensionEvaluator(IEvaluationContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			_context = context;
		}

		public void EvaluateArrayDimension(IArrayDimension dim, out int iLowerBorder, out int iUpperBorder)
		{
			if (dim == null)
			{
				throw new ArgumentNullException("dim");
			}
			try
			{
				ConstantEvaluator constantEvaluator = new ConstantEvaluator(_context);
				ILiteralValue literalValue = constantEvaluator.Evaluate(dim.LowerBorder);
				ILiteralValue literalValue2 = constantEvaluator.Evaluate(dim.UpperBorder);
				iLowerBorder = literalValue.GetInt(out bool bValid);
				iUpperBorder = literalValue2.GetInt(out bValid);
			}
			catch (LanguageModelUtilitiesException)
			{
				throw;
			}
			catch (Exception exReason)
			{
				throw new LanguageModelUtilitiesException(exReason);
			}
		}
	}
}

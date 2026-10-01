using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities.MonitoringByteCode
{
	internal static class OperatorMapping
	{
		private static readonly LDictionary<Operator, OperatorCodes> _dictOperator2OperatorCode;

		static OperatorMapping()
		{
			_dictOperator2OperatorCode = new LDictionary<Operator, OperatorCodes>();
			_dictOperator2OperatorCode.Add(Operator.Plus, OperatorCodes.Addition);
			_dictOperator2OperatorCode.Add(Operator.Minus, OperatorCodes.Subtraction);
			_dictOperator2OperatorCode.Add(Operator.Times, OperatorCodes.Multiplication);
			_dictOperator2OperatorCode.Add(Operator.Divide, OperatorCodes.Division);
			_dictOperator2OperatorCode.Add(Operator.Mod, OperatorCodes.Modulo);
			_dictOperator2OperatorCode.Add(Operator.__Cast, OperatorCodes.CastUnsignedToSigned);
		}

		public static OperatorCodes GetOperatorCode(Operator op)
		{
			OperatorCodes result = default(OperatorCodes);
			if (!_dictOperator2OperatorCode.TryGetValue(op, ref result))
			{
				return OperatorCodes.None;
			}
			return result;
		}
	}
}

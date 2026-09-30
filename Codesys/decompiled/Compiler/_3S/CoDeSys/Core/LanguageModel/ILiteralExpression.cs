using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILiteralExpression : IExpression2, IExpression, IExprement
	{
		long LongValue { get; }

		ulong ULongValue { get; }

		string StringValue { get; }

		double RealValue { get; }

		bool Negative { get; }

		TypeClass ConstantType { get; }

		ILiteralValue LiteralValue { get; }
	}
}

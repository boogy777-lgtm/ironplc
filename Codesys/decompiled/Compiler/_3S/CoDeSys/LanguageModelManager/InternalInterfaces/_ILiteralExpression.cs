using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ILiteralExpression : _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ILiteralExpression2, ILiteralExpression
	{
		new long LongValue { get; set; }

		new ulong ULongValue { get; }

		new string StringValue { get; set; }

		new double RealValue { get; set; }

		new bool Negative { get; set; }

		new TypeClass ConstantType { get; set; }

		int Base { get; }

		bool LiteralValueEquals(_ILiteralExpression other);
	}
}

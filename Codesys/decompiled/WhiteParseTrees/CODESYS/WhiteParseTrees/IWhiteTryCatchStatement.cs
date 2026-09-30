using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteTryCatchStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		ITryToken __Try { get; set; }

		IWhiteSequenceStatement TrySequence { get; set; }

		ICatchToken __Catch { get; set; }

		[Nullable(2)]
		IParenthesizedExpression ExceptionExpression
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}

		IWhiteSequenceStatement CatchSequence { get; set; }

		[Nullable(2)]
		IFinallyToken __Finally
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}

		[Nullable(2)]
		IWhiteSequenceStatement FinallySequence
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}

		IEndTryToken __EndTry { get; set; }
	}
}

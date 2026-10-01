using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteTryCatchStatement : WhiteStatement, IWhiteTryCatchStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		public ITryToken __Try { get; set; }

		public IWhiteSequenceStatement TrySequence { get; set; }

		public ICatchToken __Catch { get; set; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public IParenthesizedExpression ExceptionExpression
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		public IWhiteSequenceStatement CatchSequence { get; set; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public IFinallyToken __Finally
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public IWhiteSequenceStatement FinallySequence
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		public IEndTryToken __EndTry { get; set; }

		public WhiteTryCatchStatement(ITryToken @try, IWhiteSequenceStatement trySequence, ICatchToken @catch, [System.Runtime.CompilerServices.Nullable(2)] IParenthesizedExpression exceptionExpression, IWhiteSequenceStatement catchSequence, [System.Runtime.CompilerServices.Nullable(2)] IFinallyToken @finally, [System.Runtime.CompilerServices.Nullable(2)] IWhiteSequenceStatement finallySequence, IEndTryToken endtry)
		{
			__Try = @try;
			TrySequence = trySequence;
			__Catch = @catch;
			ExceptionExpression = exceptionExpression;
			CatchSequence = catchSequence;
			__Finally = @finally;
			FinallySequence = finallySequence;
			__EndTry = endtry;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return __Try;
			yield return TrySequence;
			yield return __Catch;
			if (ExceptionExpression != null)
			{
				yield return ExceptionExpression;
			}
			yield return CatchSequence;
			if (__Finally != null)
			{
				yield return __Finally;
			}
			if (FinallySequence != null)
			{
				yield return FinallySequence;
			}
			yield return __EndTry;
		}

		public override void Accept(IStatementSyntax.IStatementVisitor visitor)
		{
			visitor.visit(this);
		}

		public override T Accept<[System.Runtime.CompilerServices.Nullable(2)] T>(IStatementSyntax.IStatementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		public override T Accept<[System.Runtime.CompilerServices.Nullable(2)] T, [System.Runtime.CompilerServices.Nullable(2)] TContext>(IStatementSyntax.IStatementVisitor<T, TContext> visitor, TContext context)
		{
			return visitor.visit(this, context);
		}
	}
}

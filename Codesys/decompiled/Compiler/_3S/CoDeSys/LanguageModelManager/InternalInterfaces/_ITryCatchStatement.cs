using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ITryCatchStatement : _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement
	{
		ISequenceStatement3 Try { get; }

		ISequenceStatement3 Catch { get; }

		ISequenceStatement3 Finally { get; }

		_ISequenceStatement _Try { get; set; }

		_ISequenceStatement _Catch { get; set; }

		_ISequenceStatement _Finally { get; set; }

		_ISequenceStatement _ReplacedSequence { get; set; }

		_IExpression _Exception { get; set; }

		_ISubRoutineStatement Subroutine { get; set; }

		int Index { get; set; }

		void DefaultTraverse(IExprementVisitor visitor);
	}
}

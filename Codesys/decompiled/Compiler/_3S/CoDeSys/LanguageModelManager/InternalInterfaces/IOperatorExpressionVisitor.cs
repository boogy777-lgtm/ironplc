using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IOperatorExpressionVisitor
	{
		void visitTestAndSet(_IOperatorExpression op);

		void visitMove(_IOperatorExpression op);

		void visitAbs(_IOperatorExpression op);

		void visitTrunc(_IOperatorExpression op);

		void visitTruncInt(_IOperatorExpression op);

		void visitIni(_IOperatorExpression op);

		void visitThrow(_IOperatorExpression op);

		void visitBitOffset(_IOperatorExpression op);

		void visitBitAdr(_IOperatorExpression op);

		void visitReloc(_IOperatorExpression op);

		void visitMemorySet(_IOperatorExpression op);

		void visitGetLTick(_IOperatorExpression op);

		void visitMaxOffset(_IOperatorExpression op);

		void visitLocalOffset(_IOperatorExpression op);

		void visitCRC(_IOperatorExpression op);

		void visitIsValidRef(_IOperatorExpression op);

		void visitDelete(_IOperatorExpression op);

		void visitQueryPointer(_IOperatorExpression op);

		void visitQueryInterface(_IOperatorExpression op);

		void visitPropertyInfo(_IOperatorExpression op);

		void visitFCall(_IOperatorExpression op);

		void visitTypeOf(_IOperatorExpression op);

		void visitInit(_IOperatorExpression op);

		void visitVarInfo(_IOperatorExpression op);

		void visitRefAdr(_IOperatorExpression op);

		void visitAdr(_IOperatorExpression op);

		void visitSizeOf(_IOperatorExpression op);

		void visitAdrInst(_IOperatorExpression op);

		void visitIndexOf(_IOperatorExpression op);

		void visitTime(_IOperatorExpression op);

		void visitLTime(_IOperatorExpression op);

		void visitShiftOps(_IOperatorExpression op);

		void visitTrigonometrics(_IOperatorExpression op);

		void visitComparisons(_IOperatorExpression op);

		void visitBoolOps(_IOperatorExpression op);

		void visitArithmetics(_IOperatorExpression op);

		void visitSelection(_IOperatorExpression op);

		void visitCheckLicense(_IOperatorExpression op);

		void visitCallInitFunction(_IOperatorExpression op);

		void visitLateCompiledExpr(_IOperatorExpression operatorExpression);

		void visitLowerUpperBound(_IOperatorExpression operatorExpression);

		void visitCurrentTask(_IOperatorExpression operatorExpression);
	}
}

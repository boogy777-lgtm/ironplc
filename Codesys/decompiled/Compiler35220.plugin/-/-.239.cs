using System;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0006
{
	// Token: 0x0200029A RID: 666
	internal static class \u0008
	{
		// Token: 0x06002A29 RID: 10793 RVA: 0x000933B8 File Offset: 0x000915B8
		public static \u0001 \u0001<\u0001>(this _IStatement \u0002, IStatementVisitor<\u0001> \u0003)
		{
			_IWhileStatement iwhileStatement = \u0002 as _IWhileStatement;
			if (iwhileStatement != null)
			{
				return \u0003.visit(iwhileStatement);
			}
			_IRepeatStatement irepeatStatement = \u0002 as _IRepeatStatement;
			if (irepeatStatement != null)
			{
				return \u0003.visit(irepeatStatement);
			}
			_IForStatement iforStatement = \u0002 as _IForStatement;
			if (iforStatement != null)
			{
				return \u0003.visit(iforStatement);
			}
			_IIfStatement iifStatement = \u0002 as _IIfStatement;
			if (iifStatement != null)
			{
				return \u0003.visit(iifStatement);
			}
			_IExpressionStatement iexpressionStatement = \u0002 as _IExpressionStatement;
			if (iexpressionStatement != null)
			{
				return \u0003.visit(iexpressionStatement);
			}
			_ICaseStatement icaseStatement = \u0002 as _ICaseStatement;
			if (icaseStatement != null)
			{
				return \u0003.visit(icaseStatement);
			}
			_ISequenceStatement isequenceStatement = \u0002 as _ISequenceStatement;
			if (isequenceStatement != null)
			{
				return \u0003.visit(isequenceStatement);
			}
			_IPragmaStatement ipragmaStatement = \u0002 as _IPragmaStatement;
			if (ipragmaStatement != null)
			{
				return \u0003.visit(ipragmaStatement);
			}
			_IReturnStatement ireturnStatement = \u0002 as _IReturnStatement;
			if (ireturnStatement != null)
			{
				return \u0003.visit(ireturnStatement);
			}
			_ITryCatchStatement itryCatchStatement = \u0002 as _ITryCatchStatement;
			if (itryCatchStatement == null)
			{
				return \u0003.visitGeneric(\u0002);
			}
			return \u0003.visit(itryCatchStatement);
		}
	}
}

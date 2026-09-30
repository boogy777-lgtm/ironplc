using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x02000279 RID: 633
	public abstract class AbstractStatementReplacer : IStatementVisitor<_IStatement>
	{
		// Token: 0x0600280F RID: 10255 RVA: 0x0008B834 File Offset: 0x00089A34
		public virtual _IStatement visit(_IWhileStatement whilst)
		{
			return null;
		}

		// Token: 0x06002810 RID: 10256 RVA: 0x0008B838 File Offset: 0x00089A38
		public virtual _IStatement visit(_IRepeatStatement repeat)
		{
			return null;
		}

		// Token: 0x06002811 RID: 10257 RVA: 0x0008B83C File Offset: 0x00089A3C
		public virtual _IStatement visit(_IForStatement forloop)
		{
			return null;
		}

		// Token: 0x06002812 RID: 10258 RVA: 0x0008B840 File Offset: 0x00089A40
		public virtual _IStatement visit(_IIfStatement ifst)
		{
			return null;
		}

		// Token: 0x06002813 RID: 10259 RVA: 0x0008B844 File Offset: 0x00089A44
		public virtual _IStatement visit(_IExpressionStatement expstat)
		{
			return null;
		}

		// Token: 0x06002814 RID: 10260 RVA: 0x0008B848 File Offset: 0x00089A48
		public virtual _IStatement visit(_ICaseStatement caseStatement)
		{
			return null;
		}

		// Token: 0x06002815 RID: 10261 RVA: 0x0008B84C File Offset: 0x00089A4C
		public virtual _IStatement visit(_ISequenceStatement sequenceStatement)
		{
			return null;
		}

		// Token: 0x06002816 RID: 10262 RVA: 0x0008B850 File Offset: 0x00089A50
		public virtual _IStatement visit(_IPragmaStatement pragmaStatement)
		{
			return null;
		}

		// Token: 0x06002817 RID: 10263 RVA: 0x0008B854 File Offset: 0x00089A54
		public virtual _IStatement visit(_IReturnStatement returnStatement)
		{
			return null;
		}

		// Token: 0x06002818 RID: 10264 RVA: 0x0008B858 File Offset: 0x00089A58
		public virtual _IStatement visit(_ITryCatchStatement tryCatchStatement)
		{
			return null;
		}

		// Token: 0x06002819 RID: 10265 RVA: 0x0008B85C File Offset: 0x00089A5C
		public virtual _IStatement visitGeneric(_IStatement statement)
		{
			return null;
		}
	}
}

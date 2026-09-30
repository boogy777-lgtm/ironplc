using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u000E;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0084
{
	// Token: 0x0200024B RID: 587
	internal sealed class \u0016 : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor, IExprementVisitor2
	{
		// Token: 0x06002668 RID: 9832 RVA: 0x00086234 File Offset: 0x00084434
		private \u0016()
		{
		}

		// Token: 0x17000706 RID: 1798
		// (get) Token: 0x06002669 RID: 9833 RVA: 0x00086248 File Offset: 0x00084448
		// (set) Token: 0x0600266A RID: 9834 RVA: 0x00086250 File Offset: 0x00084450
		private bool ErrorOccured { get; set; }

		// Token: 0x0600266B RID: 9835 RVA: 0x0008625C File Offset: 0x0008445C
		internal static void \u0001(_ISequenceStatement \u0002, out bool \u0003)
		{
			\u0003 = false;
			if (\u0002 == null)
			{
				return;
			}
			\u0016 u = new \u0016();
			u.\u0001(false, false, \u0016.\u0001.\u0001);
			u.\u0001(\u0002);
			u.\u0001();
			if (u.ErrorOccured)
			{
				\u0003 = true;
			}
		}

		// Token: 0x0600266C RID: 9836 RVA: 0x0008628C File Offset: 0x0008448C
		internal static void \u0001(_ICompiledPOU \u0002, out bool \u0003)
		{
			\u0003 = false;
			\u0016 u = new \u0016();
			u.\u0001(\u0002);
			if (u.ErrorOccured)
			{
				\u0003 = true;
			}
		}

		// Token: 0x0600266D RID: 9837 RVA: 0x000862A8 File Offset: 0x000844A8
		internal static void \u0001(_ICompileContext \u0002, out bool \u0003)
		{
			\u0003 = false;
			foreach (_ICompiledPOU icompiledPOU in \u0002.GetAllCompiledPOUsEx().OfType<_ICompiledPOU>())
			{
				if (icompiledPOU.GetFlagInternal(InternalCompiledPOUFlags.ContainsTryCatch))
				{
					bool flag;
					\u0016.\u0001(icompiledPOU, out flag);
					\u0003 = (\u0003 || flag);
				}
			}
		}

		// Token: 0x0600266E RID: 9838 RVA: 0x00086310 File Offset: 0x00084510
		private void \u0001(bool \u0002, bool \u0003, \u0016.\u0001 \u0004)
		{
			\u0016.\u0002 item = new \u0016.\u0002
			{
				CheckForContinueExit = \u0003,
				CheckForReturn = \u0002,
				Context = \u0004
			};
			this.\u0001.Push(item);
		}

		// Token: 0x0600266F RID: 9839 RVA: 0x00086344 File Offset: 0x00084544
		private void \u0001()
		{
			this.\u0001.Pop();
		}

		// Token: 0x06002670 RID: 9840 RVA: 0x00086354 File Offset: 0x00084554
		public void \u0001(_ICompiledPOU \u0002)
		{
			_ISequenceStatement isequenceStatement = \u0002.ParseTree as _ISequenceStatement;
			if (isequenceStatement != null)
			{
				this.\u0001(false, false, \u0016.\u0001.\u0001);
				isequenceStatement.Accept(this);
				this.\u0001();
			}
		}

		// Token: 0x06002671 RID: 9841 RVA: 0x00086388 File Offset: 0x00084588
		public void \u0001(_ITryCatchStatement \u0002)
		{
			this.\u0001(false, true, \u0016.\u0001.\u0002);
			if (\u0002._Try != null)
			{
				\u0002._Try.Accept(this);
			}
			this.\u0001();
			this.\u0001(false, true, \u0016.\u0001.\u0003);
			if (\u0002._Catch != null)
			{
				\u0002._Catch.Accept(this);
			}
			this.\u0001();
			this.\u0001(true, true, \u0016.\u0001.\u0004);
			if (\u0002._Finally != null)
			{
				\u0002._Finally.Accept(this);
			}
			this.\u0001();
		}

		// Token: 0x06002672 RID: 9842 RVA: 0x00086400 File Offset: 0x00084600
		public void \u0001(_ISequenceStatement \u0002)
		{
			foreach (_IStatement istatement in \u0002.StatementList.OfType<_IStatement>())
			{
				istatement.Accept(this);
			}
		}

		// Token: 0x06002673 RID: 9843 RVA: 0x00086450 File Offset: 0x00084650
		public void \u0001(_IRepeatStatement \u0002)
		{
			\u0016.\u0002 u = this.\u0001.Peek();
			this.\u0001(u.CheckForReturn, false, u.Context);
			\u0002._Controlled.Accept(this);
			this.\u0001();
		}

		// Token: 0x06002674 RID: 9844 RVA: 0x00086490 File Offset: 0x00084690
		public void \u0001(_IForStatement \u0002)
		{
			\u0016.\u0002 u = this.\u0001.Peek();
			this.\u0001(u.CheckForReturn, false, u.Context);
			\u0002._Controlled.Accept(this);
			this.\u0001();
		}

		// Token: 0x06002675 RID: 9845 RVA: 0x000864D0 File Offset: 0x000846D0
		public void \u0001(_IWhileStatement \u0002)
		{
			\u0016.\u0002 u = this.\u0001.Peek();
			this.\u0001(u.CheckForReturn, false, u.Context);
			\u0002._Controlled.Accept(this);
			this.\u0001();
		}

		// Token: 0x06002676 RID: 9846 RVA: 0x00086510 File Offset: 0x00084710
		public void \u0001(_ICaseStatement \u0002)
		{
			foreach (_ICase icase in \u0002._Cases)
			{
				icase._Controlled.Accept(this);
			}
		}

		// Token: 0x06002677 RID: 9847 RVA: 0x00086560 File Offset: 0x00084760
		public void \u0001(_IIfStatement \u0002)
		{
			\u0002._IfThen.Accept(this);
			if (\u0002._IfElse != null)
			{
				\u0002._IfElse.Accept(this);
			}
		}

		// Token: 0x06002678 RID: 9848 RVA: 0x00086584 File Offset: 0x00084784
		public void \u0001(_IExitStatement \u0002)
		{
			\u0016.\u0002 u = this.\u0001.Peek();
			if (u.CheckForContinueExit)
			{
				switch (u.Context)
				{
				case \u0016.\u0001.\u0002:
					\u0002.AddError(\u0018.\u0001(MessageId.Err_NoExitOrContinueOutOfTry), MessageId.Err_NoExitOrContinueOutOfTry);
					this.ErrorOccured = true;
					return;
				case \u0016.\u0001.\u0003:
					\u0002.AddError(\u0018.\u0001(MessageId.Err_NoExitOrContinueOutOfCatch), MessageId.Err_NoExitOrContinueOutOfCatch);
					this.ErrorOccured = true;
					return;
				case \u0016.\u0001.\u0004:
					\u0002.AddError(\u0018.\u0001(MessageId.Err_NoBranchOutOfFinally), MessageId.Err_NoBranchOutOfFinally);
					this.ErrorOccured = true;
					return;
				default:
					Debug.\u0001(false);
					break;
				}
			}
		}

		// Token: 0x06002679 RID: 9849 RVA: 0x00086620 File Offset: 0x00084820
		public void \u0001(_IReturnStatement \u0002)
		{
			\u0016.\u0002 u = this.\u0001.Peek();
			if (u.CheckForReturn)
			{
				switch (u.Context)
				{
				case \u0016.\u0001.\u0002:
					\u0002.AddError(\u0018.\u0001(MessageId.Err_NoExitOrContinueOutOfTry), MessageId.Err_NoExitOrContinueOutOfTry);
					this.ErrorOccured = true;
					return;
				case \u0016.\u0001.\u0003:
					\u0002.AddError(\u0018.\u0001(MessageId.Err_NoExitOrContinueOutOfCatch), MessageId.Err_NoExitOrContinueOutOfCatch);
					this.ErrorOccured = true;
					return;
				case \u0016.\u0001.\u0004:
					\u0002.AddError(\u0018.\u0001(MessageId.Err_NoBranchOutOfFinally), MessageId.Err_NoBranchOutOfFinally);
					this.ErrorOccured = true;
					return;
				default:
					Debug.\u0001(false);
					break;
				}
			}
		}

		// Token: 0x0600267A RID: 9850 RVA: 0x000866BC File Offset: 0x000848BC
		public void \u0001(_IContinueStatement \u0002)
		{
			\u0016.\u0002 u = this.\u0001.Peek();
			if (u.CheckForContinueExit)
			{
				switch (u.Context)
				{
				case \u0016.\u0001.\u0002:
					\u0002.AddError(\u0018.\u0001(MessageId.Err_NoExitOrContinueOutOfTry), MessageId.Err_NoExitOrContinueOutOfTry);
					this.ErrorOccured = true;
					return;
				case \u0016.\u0001.\u0003:
					\u0002.AddError(\u0018.\u0001(MessageId.Err_NoExitOrContinueOutOfCatch), MessageId.Err_NoExitOrContinueOutOfCatch);
					this.ErrorOccured = true;
					return;
				case \u0016.\u0001.\u0004:
					\u0002.AddError(\u0018.\u0001(MessageId.Err_NoBranchOutOfFinally), MessageId.Err_NoBranchOutOfFinally);
					this.ErrorOccured = true;
					return;
				default:
					Debug.\u0001(false);
					break;
				}
			}
		}

		// Token: 0x0600267B RID: 9851 RVA: 0x00086758 File Offset: 0x00084958
		public void \u0001(_IPragmaIfStatement \u0002)
		{
			\u0002.IfThen.Accept(this);
			if (\u0002.IfElse != null)
			{
				\u0002.IfElse.Accept(this);
			}
		}

		// Token: 0x0600267C RID: 9852 RVA: 0x0008677C File Offset: 0x0008497C
		public void \u0001(_IJumpStatement \u0002)
		{
		}

		// Token: 0x0600267D RID: 9853 RVA: 0x00086780 File Offset: 0x00084980
		public void \u0001(_ICaseLabelStatement \u0002)
		{
		}

		// Token: 0x0600267E RID: 9854 RVA: 0x00086784 File Offset: 0x00084984
		public void \u0001(_ICommentStatement \u0002)
		{
		}

		// Token: 0x0600267F RID: 9855 RVA: 0x00086788 File Offset: 0x00084988
		public void \u0001(_IExpressionStatement \u0002)
		{
		}

		// Token: 0x06002680 RID: 9856 RVA: 0x0008678C File Offset: 0x0008498C
		public void \u0001(_IErrorStatement \u0002)
		{
		}

		// Token: 0x06002681 RID: 9857 RVA: 0x00086790 File Offset: 0x00084990
		public void \u0001(_INullStatement \u0002)
		{
		}

		// Token: 0x06002682 RID: 9858 RVA: 0x00086794 File Offset: 0x00084994
		public void \u0001(_IEmptyStatement \u0002)
		{
		}

		// Token: 0x06002683 RID: 9859 RVA: 0x00086798 File Offset: 0x00084998
		public void \u0001(_IPragmaStatement \u0002)
		{
		}

		// Token: 0x06002684 RID: 9860 RVA: 0x0008679C File Offset: 0x0008499C
		public void \u0001(_ILabelStatement \u0002)
		{
		}

		// Token: 0x06002685 RID: 9861 RVA: 0x000867A0 File Offset: 0x000849A0
		public void \u0001(_IBreakPointStatement \u0002)
		{
		}

		// Token: 0x06002686 RID: 9862 RVA: 0x000867A4 File Offset: 0x000849A4
		public void \u0001(_IDefineStatement \u0002)
		{
		}

		// Token: 0x06002687 RID: 9863 RVA: 0x000867A8 File Offset: 0x000849A8
		public void \u0001(_ICurrentTaskExpression \u0002)
		{
		}

		// Token: 0x06002688 RID: 9864 RVA: 0x000867AC File Offset: 0x000849AC
		public void \u0001(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002689 RID: 9865 RVA: 0x000867B0 File Offset: 0x000849B0
		public void \u0001(_IThisExpression \u0002)
		{
		}

		// Token: 0x0600268A RID: 9866 RVA: 0x000867B4 File Offset: 0x000849B4
		public void \u0001(_ILiteralExpression \u0002)
		{
		}

		// Token: 0x0600268B RID: 9867 RVA: 0x000867B8 File Offset: 0x000849B8
		public void \u0001(_IVariableExpression \u0002)
		{
		}

		// Token: 0x0600268C RID: 9868 RVA: 0x000867BC File Offset: 0x000849BC
		public void \u0001(_ICompoAccessExpression \u0002)
		{
		}

		// Token: 0x0600268D RID: 9869 RVA: 0x000867C0 File Offset: 0x000849C0
		public void \u0001(_ICopyScopeExpression \u0002)
		{
		}

		// Token: 0x0600268E RID: 9870 RVA: 0x000867C4 File Offset: 0x000849C4
		public void \u0001(_ISystemScopeExpression \u0002)
		{
		}

		// Token: 0x0600268F RID: 9871 RVA: 0x000867C8 File Offset: 0x000849C8
		public void \u0001(_ICaseRangeExpression \u0002)
		{
		}

		// Token: 0x06002690 RID: 9872 RVA: 0x000867CC File Offset: 0x000849CC
		public void \u0001(_IVariableReference \u0002)
		{
		}

		// Token: 0x06002691 RID: 9873 RVA: 0x000867D0 File Offset: 0x000849D0
		public void \u0001(_IPouReference \u0002)
		{
		}

		// Token: 0x06002692 RID: 9874 RVA: 0x000867D4 File Offset: 0x000849D4
		public void \u0001(_IResourceReference \u0002)
		{
		}

		// Token: 0x06002693 RID: 9875 RVA: 0x000867D8 File Offset: 0x000849D8
		public void \u0001(_IPragmaOperatorExpression \u0002)
		{
		}

		// Token: 0x06002694 RID: 9876 RVA: 0x000867DC File Offset: 0x000849DC
		public void \u0001(_IXRefExpression \u0002)
		{
		}

		// Token: 0x06002695 RID: 9877 RVA: 0x000867E0 File Offset: 0x000849E0
		public void \u0001(_IIsEnumTypeExpression \u0002)
		{
		}

		// Token: 0x06002696 RID: 9878 RVA: 0x000867E4 File Offset: 0x000849E4
		public void \u0001(_IHasValueExpression \u0002)
		{
		}

		// Token: 0x06002697 RID: 9879 RVA: 0x000867E8 File Offset: 0x000849E8
		public void \u0001(_IPragmaAssertion \u0002)
		{
		}

		// Token: 0x06002698 RID: 9880 RVA: 0x000867EC File Offset: 0x000849EC
		public void \u0001(_ICastExpression \u0002)
		{
		}

		// Token: 0x06002699 RID: 9881 RVA: 0x000867F0 File Offset: 0x000849F0
		public void \u0001(_ITypeExpression \u0002)
		{
		}

		// Token: 0x0600269A RID: 9882 RVA: 0x000867F4 File Offset: 0x000849F4
		public void \u0001(_INewExpression \u0002)
		{
		}

		// Token: 0x0600269B RID: 9883 RVA: 0x000867F8 File Offset: 0x000849F8
		public void \u0001(_ICompilerVersionExpression \u0002)
		{
		}

		// Token: 0x0600269C RID: 9884 RVA: 0x000867FC File Offset: 0x000849FC
		public void \u0001(_IHasConstantValueExpression \u0002)
		{
		}

		// Token: 0x0600269D RID: 9885 RVA: 0x00086800 File Offset: 0x00084A00
		public void \u0001(_IHasConstantTypeExpression \u0002)
		{
		}

		// Token: 0x0600269E RID: 9886 RVA: 0x00086804 File Offset: 0x00084A04
		public void \u0001(_IHasAttributeExpression \u0002)
		{
		}

		// Token: 0x0600269F RID: 9887 RVA: 0x00086808 File Offset: 0x00084A08
		public void \u0001(_IHasTypeExpression \u0002)
		{
		}

		// Token: 0x060026A0 RID: 9888 RVA: 0x0008680C File Offset: 0x00084A0C
		public void \u0001(_IDefinedExpression \u0002)
		{
		}

		// Token: 0x060026A1 RID: 9889 RVA: 0x00086810 File Offset: 0x00084A10
		public void \u0001(_ITaskReference \u0002)
		{
		}

		// Token: 0x060026A2 RID: 9890 RVA: 0x00086814 File Offset: 0x00084A14
		public void \u0001(_ITypeReference \u0002)
		{
		}

		// Token: 0x060026A3 RID: 9891 RVA: 0x00086818 File Offset: 0x00084A18
		public void \u0001(_IDefineReference \u0002)
		{
		}

		// Token: 0x060026A4 RID: 9892 RVA: 0x0008681C File Offset: 0x00084A1C
		public void \u0001(_IQualifiedNameExpression \u0002)
		{
		}

		// Token: 0x060026A5 RID: 9893 RVA: 0x00086820 File Offset: 0x00084A20
		public void \u0001(_INullExpression \u0002)
		{
		}

		// Token: 0x060026A6 RID: 9894 RVA: 0x00086824 File Offset: 0x00084A24
		public void \u0001(_IErrorExpression \u0002)
		{
		}

		// Token: 0x060026A7 RID: 9895 RVA: 0x00086828 File Offset: 0x00084A28
		public void \u0001(_IGlobalScopeExpression \u0002)
		{
		}

		// Token: 0x060026A8 RID: 9896 RVA: 0x0008682C File Offset: 0x00084A2C
		public void \u0001(_IDeRefAccessExpression \u0002)
		{
		}

		// Token: 0x060026A9 RID: 9897 RVA: 0x00086830 File Offset: 0x00084A30
		public void \u0001(_IIndexAccessExpression \u0002)
		{
		}

		// Token: 0x060026AA RID: 9898 RVA: 0x00086834 File Offset: 0x00084A34
		public void \u0001(_IAddressExpression \u0002)
		{
		}

		// Token: 0x060026AB RID: 9899 RVA: 0x00086838 File Offset: 0x00084A38
		public void \u0001(_IBaseExpression \u0002)
		{
		}

		// Token: 0x060026AC RID: 9900 RVA: 0x0008683C File Offset: 0x00084A3C
		public void \u0001(_IConversionExpression \u0002)
		{
		}

		// Token: 0x060026AD RID: 9901 RVA: 0x00086840 File Offset: 0x00084A40
		public void \u0001(_ICallExpression \u0002)
		{
		}

		// Token: 0x060026AE RID: 9902 RVA: 0x00086844 File Offset: 0x00084A44
		public void \u0001(_IAssignmentExpression \u0002)
		{
		}

		// Token: 0x060026AF RID: 9903 RVA: 0x00086848 File Offset: 0x00084A48
		public void \u0001(_IPoolScopeExpression \u0002)
		{
		}

		// Token: 0x060026B0 RID: 9904 RVA: 0x0008684C File Offset: 0x00084A4C
		public void \u0001(_IRuntimeVersionExpression \u0002)
		{
		}

		// Token: 0x060026B1 RID: 9905 RVA: 0x00086850 File Offset: 0x00084A50
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
		}

		// Token: 0x060026B2 RID: 9906 RVA: 0x00086854 File Offset: 0x00084A54
		public void \u0001(_IPartialAccessExpression \u0002)
		{
		}

		// Token: 0x060026B3 RID: 9907 RVA: 0x00086858 File Offset: 0x00084A58
		public void \u0001(_IProjectDefinedExpression \u0002)
		{
		}

		// Token: 0x060026B4 RID: 9908 RVA: 0x0008685C File Offset: 0x00084A5C
		public void \u0001(_IVariableDeclarationStatement \u0002)
		{
		}

		// Token: 0x060026B5 RID: 9909 RVA: 0x00086860 File Offset: 0x00084A60
		public void \u0001(_IPOUDeclarationStatement \u0002)
		{
		}

		// Token: 0x060026B6 RID: 9910 RVA: 0x00086864 File Offset: 0x00084A64
		public void \u0001(_IEnumDeclarationStatement \u0002)
		{
		}

		// Token: 0x060026B7 RID: 9911 RVA: 0x00086868 File Offset: 0x00084A68
		public void \u0001(_IMultipleIndexInitialization \u0002)
		{
		}

		// Token: 0x060026B8 RID: 9912 RVA: 0x0008686C File Offset: 0x00084A6C
		public void \u0001(_IStructureInitialization \u0002)
		{
		}

		// Token: 0x060026B9 RID: 9913 RVA: 0x00086870 File Offset: 0x00084A70
		public void \u0001(_IArrayInitialization \u0002)
		{
		}

		// Token: 0x060026BA RID: 9914 RVA: 0x00086874 File Offset: 0x00084A74
		public void \u0001(_IEnumDeclarationListStatement \u0002)
		{
		}

		// Token: 0x060026BB RID: 9915 RVA: 0x00086878 File Offset: 0x00084A78
		public void \u0001(_ITypeDeclarationStatement \u0002)
		{
		}

		// Token: 0x060026BC RID: 9916 RVA: 0x0008687C File Offset: 0x00084A7C
		public void \u0001(_IVariableDeclarationListStatement \u0002)
		{
		}

		// Token: 0x0400070A RID: 1802
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x0400070B RID: 1803
		private readonly Stack<\u0016.\u0002> \u0001 = new Stack<\u0016.\u0002>();

		// Token: 0x0200024C RID: 588
		private enum \u0001
		{
			// Token: 0x0400070D RID: 1805
			\u0001,
			// Token: 0x0400070E RID: 1806
			\u0002,
			// Token: 0x0400070F RID: 1807
			\u0003,
			// Token: 0x04000710 RID: 1808
			\u0004
		}

		// Token: 0x0200024D RID: 589
		private sealed class \u0002
		{
			// Token: 0x17000707 RID: 1799
			// (get) Token: 0x060026BD RID: 9917 RVA: 0x00086880 File Offset: 0x00084A80
			// (set) Token: 0x060026BE RID: 9918 RVA: 0x00086888 File Offset: 0x00084A88
			public \u0016.\u0001 Context { get; set; }

			// Token: 0x17000708 RID: 1800
			// (get) Token: 0x060026BF RID: 9919 RVA: 0x00086894 File Offset: 0x00084A94
			// (set) Token: 0x060026C0 RID: 9920 RVA: 0x0008689C File Offset: 0x00084A9C
			public bool CheckForContinueExit { get; set; }

			// Token: 0x17000709 RID: 1801
			// (get) Token: 0x060026C1 RID: 9921 RVA: 0x000868A8 File Offset: 0x00084AA8
			// (set) Token: 0x060026C2 RID: 9922 RVA: 0x000868B0 File Offset: 0x00084AB0
			public bool CheckForReturn { get; set; }

			// Token: 0x04000711 RID: 1809
			[CompilerGenerated]
			private \u0016.\u0001 \u0001;

			// Token: 0x04000712 RID: 1810
			[CompilerGenerated]
			private bool \u0001;

			// Token: 0x04000713 RID: 1811
			[CompilerGenerated]
			private bool \u0002;
		}
	}
}

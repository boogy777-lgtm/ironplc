using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using \u000F;
using \u0013;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001E
{
	// Token: 0x02000090 RID: 144
	internal sealed class \u0002 : \u0013.\u0002
	{
		// Token: 0x17000423 RID: 1059
		// (get) Token: 0x06000C36 RID: 3126 RVA: 0x0001D4DC File Offset: 0x0001B6DC
		// (set) Token: 0x06000C37 RID: 3127 RVA: 0x0001D4E4 File Offset: 0x0001B6E4
		private IDictionary<_IExprement, int> ExprementTable { get; set; }

		// Token: 0x06000C38 RID: 3128 RVA: 0x0001D4F0 File Offset: 0x0001B6F0
		protected \u0002(BinaryWriter \u009C\u0002, IDictionary<_IExprement, int> \u009D\u0002) : base(\u009C\u0002)
		{
			this.ExprementTable = \u009D\u0002;
		}

		// Token: 0x06000C39 RID: 3129 RVA: 0x0001D500 File Offset: 0x0001B700
		public static void \u0001(BinaryWriter \u0002, IDictionary<_IExprement, int> \u0003, _IExprement \u0004)
		{
			\u001E.\u0002 ivisit = new \u001E.\u0002(\u0002, \u0003);
			\u0004.Accept(ivisit);
		}

		// Token: 0x06000C3A RID: 3130 RVA: 0x0001D51C File Offset: 0x0001B71C
		public static void \u0001(BinaryWriter \u0002, IDictionary<_IExprement, int> \u0003, _IExprement \u0004, ICompactedParseTreeInformation \u0005)
		{
			\u001E.\u0002 ivisit = new \u001E.\u0002(\u0002, \u0003);
			\u0004.Accept(ivisit);
			\u0013.\u0002.\u0001(\u0002, \u0005);
		}

		// Token: 0x06000C3B RID: 3131 RVA: 0x0001D540 File Offset: 0x0001B740
		public static void \u0001(BinaryWriter \u0002, IGreenTreeTables \u0003, IDictionary<_IExprement, int> \u0004)
		{
			object tableLock = \u0003.TableLock;
			lock (tableLock)
			{
				int num = 0;
				int num2 = \u0003.VarTable.Values.Count + \u0003.IntegerTable.Values.Count + \u0003.ExprTable.Values.Count;
				\u0002.Write(num2);
				foreach (_IVariableExpression ivariableExpression in \u0003.VarTable.Values)
				{
					global::\u000F.\u0004.\u0001(\u0002, ivariableExpression);
					\u0004.Add(ivariableExpression, num++);
				}
				foreach (_ILiteralExpression iliteralExpression in \u0003.IntegerTable.Values)
				{
					global::\u000F.\u0004.\u0001(\u0002, iliteralExpression);
					\u0004.Add(iliteralExpression, num++);
				}
				foreach (_IExprement iexprement in \u0003.ExprTable.Values)
				{
					global::\u000F.\u0004.\u0001(\u0002, iexprement);
					\u0004.Add(iexprement, num++);
				}
				Debug.\u0001(num == num2);
			}
		}

		// Token: 0x06000C3C RID: 3132 RVA: 0x0001D6F0 File Offset: 0x0001B8F0
		private bool \u0001(_IExprement \u0002)
		{
			int value;
			if (this.ExprementTable.TryGetValue(\u0002, out value))
			{
				base.Writer.Write(78U);
				base.Writer.Write(value);
				return true;
			}
			return false;
		}

		// Token: 0x06000C3D RID: 3133 RVA: 0x0001D72C File Offset: 0x0001B92C
		public override void \u0001(_IAssignmentExpression \u0002)
		{
			if (!this.\u0001(\u0002))
			{
				base.\u0001(\u0002);
			}
		}

		// Token: 0x06000C3E RID: 3134 RVA: 0x0001D740 File Offset: 0x0001B940
		public override void \u0001(_ICompoAccessExpression \u0002)
		{
			if (!this.\u0001(\u0002))
			{
				base.\u0001(\u0002);
			}
		}

		// Token: 0x06000C3F RID: 3135 RVA: 0x0001D754 File Offset: 0x0001B954
		public override void \u0001(_IDeRefAccessExpression \u0002)
		{
			if (!this.\u0001(\u0002))
			{
				base.\u0001(\u0002);
			}
		}

		// Token: 0x06000C40 RID: 3136 RVA: 0x0001D768 File Offset: 0x0001B968
		public override void \u0001(_IIndexAccessExpression \u0002)
		{
			if (!this.\u0001(\u0002))
			{
				base.\u0001(\u0002);
			}
		}

		// Token: 0x06000C41 RID: 3137 RVA: 0x0001D77C File Offset: 0x0001B97C
		public override void \u0001(_ILiteralExpression \u0002)
		{
			if (!this.\u0001(\u0002))
			{
				base.\u0001(\u0002);
			}
		}

		// Token: 0x06000C42 RID: 3138 RVA: 0x0001D790 File Offset: 0x0001B990
		public override void \u0001(_IOperatorExpression \u0002)
		{
			if (!this.\u0001(\u0002))
			{
				base.\u0001(\u0002);
			}
		}

		// Token: 0x06000C43 RID: 3139 RVA: 0x0001D7A4 File Offset: 0x0001B9A4
		public override void \u0001(_IVariableExpression \u0002)
		{
			if (!this.\u0001(\u0002))
			{
				base.\u0001(\u0002);
			}
		}

		// Token: 0x04000210 RID: 528
		[CompilerGenerated]
		private new IDictionary<_IExprement, int> \u0001;
	}
}

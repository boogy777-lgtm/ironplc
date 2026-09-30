using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using \u0011;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0083;
using \u0084;

namespace \u0017
{
	// Token: 0x02000199 RID: 409
	internal sealed class \u000F : \u0083.\u0003, IExprVisitor
	{
		// Token: 0x170005CD RID: 1485
		// (get) Token: 0x06001D79 RID: 7545 RVA: 0x0005F56C File Offset: 0x0005D76C
		private string QualificationNamespace { get; }

		// Token: 0x170005CE RID: 1486
		// (get) Token: 0x06001D7A RID: 7546 RVA: 0x0005F574 File Offset: 0x0005D774
		private StringBuilder Builder { get; }

		// Token: 0x170005CF RID: 1487
		// (get) Token: 0x06001D7B RID: 7547 RVA: 0x0005F57C File Offset: 0x0005D77C
		private IScanner Scanner { get; }

		// Token: 0x06001D7C RID: 7548 RVA: 0x0005F584 File Offset: 0x0005D784
		private \u000F(string \u0095\u0005)
		{
			this.QualificationNamespace = \u0095\u0005;
			this.Builder = new StringBuilder();
			this.Scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner("", false, false, false, false);
		}

		// Token: 0x06001D7D RID: 7549 RVA: 0x0005F5BC File Offset: 0x0005D7BC
		internal static string \u0001(IExpression \u0002, ILMPreCompileSet \u0003, ILMPreCompileSet \u0004)
		{
			IPreCompileContext9 preCompileContext = \u0004 as IPreCompileContext9;
			if (preCompileContext != null)
			{
				ILibraryTable4 libraryTable = preCompileContext.LibraryTable as ILibraryTable4;
				if (libraryTable != null)
				{
					string localLibraryNamespaceRecursive = libraryTable.GetLocalLibraryNamespaceRecursive(preCompileContext, \u0003.LibraryPath);
					return global::\u0017.\u000F.\u0001(\u0002, localLibraryNamespaceRecursive);
				}
			}
			if (\u0002 == null)
			{
				return null;
			}
			return \u0002.ToString();
		}

		// Token: 0x06001D7E RID: 7550 RVA: 0x0005F604 File Offset: 0x0005D804
		internal static string \u0001(IExpression \u0002, string \u0003)
		{
			global::\u0017.\u000F u000F = new global::\u0017.\u000F(\u0003);
			try
			{
				\u0002.AcceptVisitor(u000F);
			}
			catch (NotSupportedException)
			{
				return string.Format(global::\u0011.\u0001.UnexpectedExpression, \u0002.ToString());
			}
			return u000F.Builder.ToString();
		}

		// Token: 0x06001D7F RID: 7551 RVA: 0x0005F654 File Offset: 0x0005D854
		public void \u0001(IOperatorExpression \u0002)
		{
			if (\u0084.\u0002.\u0001(\u0002.Code))
			{
				this.Builder.Append(this.Scanner.GetOperatorText(\u0002.Code));
				this.Builder.Append("(");
				this.\u0001(", ", \u0002.Operands);
				this.Builder.Append(")");
				return;
			}
			string operatorText = this.Scanner.GetOperatorText(\u0002.Code);
			this.Builder.Append("(");
			this.\u0001(" " + operatorText + " ", \u0002.Operands);
			this.Builder.Append(")");
		}

		// Token: 0x06001D80 RID: 7552 RVA: 0x0005F710 File Offset: 0x0005D910
		public void \u0001(IConversionExpression \u0002)
		{
			string value = global::\u0017.\u000F.\u0001(\u0002.From);
			string value2 = global::\u0017.\u000F.\u0001(\u0002.To);
			this.Builder.Append(value);
			this.Builder.Append("_TO_");
			this.Builder.Append(value2);
			this.Builder.Append("(");
			\u0002.Exp.AcceptVisitor(this);
			this.Builder.Append(")");
		}

		// Token: 0x06001D81 RID: 7553 RVA: 0x0005F790 File Offset: 0x0005D990
		public void \u0001(IThisExpression \u0002)
		{
			this.Builder.Append(\u0002.ToString());
		}

		// Token: 0x06001D82 RID: 7554 RVA: 0x0005F7A4 File Offset: 0x0005D9A4
		public void \u0001(IBaseExpression \u0002)
		{
			this.Builder.Append(\u0002.ToString());
		}

		// Token: 0x06001D83 RID: 7555 RVA: 0x0005F7B8 File Offset: 0x0005D9B8
		public void \u0001(ILiteralExpression \u0002)
		{
			this.Builder.Append(\u0002.ToString());
		}

		// Token: 0x06001D84 RID: 7556 RVA: 0x0005F7CC File Offset: 0x0005D9CC
		public void \u0001(IAddressExpression \u0002)
		{
			this.Builder.Append(\u0002.ToString());
		}

		// Token: 0x06001D85 RID: 7557 RVA: 0x0005F7E0 File Offset: 0x0005D9E0
		public void \u0001(IVariableExpression \u0002)
		{
			this.Builder.Append(this.QualificationNamespace + "." + \u0002.ToString());
		}

		// Token: 0x06001D86 RID: 7558 RVA: 0x0005F804 File Offset: 0x0005DA04
		public void \u0001(IIndexAccessExpression \u0002)
		{
			\u0002.Var.AcceptVisitor(this);
			this.Builder.Append("[");
			this.\u0001(", ", \u0002.Accesses);
			this.Builder.Append("]");
		}

		// Token: 0x06001D87 RID: 7559 RVA: 0x0005F850 File Offset: 0x0005DA50
		public void \u0001(ICompoAccessExpression \u0002)
		{
			\u0002.Left.AcceptVisitor(this);
			this.Builder.Append("." + \u0002.Right.ToString());
		}

		// Token: 0x06001D88 RID: 7560 RVA: 0x0005F880 File Offset: 0x0005DA80
		public void \u0001(IDeRefAccessExpression \u0002)
		{
			\u0002.Base.AcceptVisitor(this);
			this.Builder.Append("^");
		}

		// Token: 0x06001D89 RID: 7561 RVA: 0x0005F8A0 File Offset: 0x0005DAA0
		public void \u0001(IGlobalScopeExpression \u0002)
		{
			\u0002.Base.AcceptVisitor(this);
		}

		// Token: 0x06001D8A RID: 7562 RVA: 0x0005F8B0 File Offset: 0x0005DAB0
		private void \u0001(string \u0002, IEnumerable<IExprement> \u0003)
		{
			bool flag = true;
			foreach (IExprement exprement in \u0003.OfType<_IExprement>())
			{
				if (flag)
				{
					flag = false;
				}
				else
				{
					this.Builder.Append(\u0002);
				}
				exprement.AcceptVisitor(this);
			}
		}

		// Token: 0x06001D8B RID: 7563 RVA: 0x0005F914 File Offset: 0x0005DB14
		private static string \u0001(TypeClass \u0002)
		{
			if (\u0002 <= TypeClass.TimeOfDay)
			{
				if (\u0002 == TypeClass.DateAndTime)
				{
					return "DT";
				}
				if (\u0002 == TypeClass.TimeOfDay)
				{
					return "TOD";
				}
			}
			else
			{
				switch (\u0002)
				{
				case TypeClass.UXInt:
					return "__UXINT";
				case TypeClass.XWord:
					return "__XWORD";
				case TypeClass.XInt:
					return "__XINT";
				default:
					if (\u0002 == TypeClass.LTimeOfDay)
					{
						return "LTOD";
					}
					break;
				}
			}
			return \u0002.ToString().ToUpperInvariant();
		}

		// Token: 0x040004D3 RID: 1235
		[CompilerGenerated]
		private readonly string \u0001;

		// Token: 0x040004D4 RID: 1236
		[CompilerGenerated]
		private readonly StringBuilder \u0001;

		// Token: 0x040004D5 RID: 1237
		[CompilerGenerated]
		private readonly IScanner \u0001;
	}
}

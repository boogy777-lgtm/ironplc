using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using \u0008;
using \u000F;
using \u0012;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0080;
using \u0082;
using \u0083;
using \u0084;

namespace \u0016
{
	// Token: 0x020001E1 RID: 481
	internal class \u000E : \u0084.\u000F
	{
		// Token: 0x06002131 RID: 8497 RVA: 0x000713B4 File Offset: 0x0006F5B4
		public \u000E(int \u009F\u0003, IScope \u009B\u0002, LList<IVariable> \u0001\u0004, LStringBuilder \u0002\u0004)
		{
			this.CharacterLimit = \u009F\u0003;
			this.Vars = \u0001\u0004;
			this.Scope = \u009B\u0002;
			this.StrBuilder = \u0002\u0004;
		}

		// Token: 0x1700062E RID: 1582
		// (get) Token: 0x06002132 RID: 8498 RVA: 0x000713DC File Offset: 0x0006F5DC
		// (set) Token: 0x06002133 RID: 8499 RVA: 0x000713E4 File Offset: 0x0006F5E4
		public int CharacterLimit { get; set; }

		// Token: 0x1700062F RID: 1583
		// (get) Token: 0x06002134 RID: 8500 RVA: 0x000713F0 File Offset: 0x0006F5F0
		// (set) Token: 0x06002135 RID: 8501 RVA: 0x000713F8 File Offset: 0x0006F5F8
		public LList<IVariable> Vars { get; set; }

		// Token: 0x17000630 RID: 1584
		// (get) Token: 0x06002136 RID: 8502 RVA: 0x00071404 File Offset: 0x0006F604
		public virtual string MaxBufferSize
		{
			get
			{
				return "MAXBUFFER";
			}
		}

		// Token: 0x17000631 RID: 1585
		// (get) Token: 0x06002137 RID: 8503 RVA: 0x0007140C File Offset: 0x0006F60C
		// (set) Token: 0x06002138 RID: 8504 RVA: 0x00071414 File Offset: 0x0006F614
		public IScope Scope { get; set; }

		// Token: 0x17000632 RID: 1586
		// (get) Token: 0x06002139 RID: 8505 RVA: 0x00071420 File Offset: 0x0006F620
		// (set) Token: 0x0600213A RID: 8506 RVA: 0x00071428 File Offset: 0x0006F628
		public LStringBuilder StrBuilder { get; set; }

		// Token: 0x17000633 RID: 1587
		// (get) Token: 0x0600213B RID: 8507 RVA: 0x00071434 File Offset: 0x0006F634
		// (set) Token: 0x0600213C RID: 8508 RVA: 0x0007143C File Offset: 0x0006F63C
		public int NumArrayIndexVariables { get; set; }

		// Token: 0x17000634 RID: 1588
		// (get) Token: 0x0600213D RID: 8509 RVA: 0x00071448 File Offset: 0x0006F648
		// (set) Token: 0x0600213E RID: 8510 RVA: 0x00071450 File Offset: 0x0006F650
		public int MaxNumOfCharactersInConcatenatedString { get; set; }

		// Token: 0x17000635 RID: 1589
		// (get) Token: 0x0600213F RID: 8511 RVA: 0x0007145C File Offset: 0x0006F65C
		// (set) Token: 0x06002140 RID: 8512 RVA: 0x00071464 File Offset: 0x0006F664
		public int CurrentNumOfCharactersInConcatenatedString { get; set; }

		// Token: 0x06002141 RID: 8513 RVA: 0x00071470 File Offset: 0x0006F670
		public void \u0001(LList<global::\u0008.\u0008> \u0002)
		{
			foreach (global::\u0008.\u0008 u in \u0002)
			{
				u.\u0001(this);
			}
		}

		// Token: 0x06002142 RID: 8514 RVA: 0x000714B8 File Offset: 0x0006F6B8
		public virtual void \u0001(\u0083.\u0004 \u0002)
		{
			string text = \u0002.RValue;
			if (\u0002.RValue.Length > this.CharacterLimit)
			{
				text = text.Substring(text.Length - this.CharacterLimit + 2);
				text = text.Insert(0, "..");
			}
			if (APEnvironmentFacade.Instance.CompileOptions.UnicodeIdentifiers)
			{
				byte[] bytes = Encoding.UTF8.GetBytes(text);
				text = Encoding.UTF8.GetString(bytes);
			}
			this.StrBuilder.AppendLine(\u0002.LValue + " := '" + text + "';");
			if (this.Vars.Count > 0)
			{
				string text2 = \u0002.LValue.Substring(0, \u0002.LValue.LastIndexOf('.') + 1);
				foreach (IVariable variable in this.Vars)
				{
					this.StrBuilder.AppendLine(string.Concat(new string[]
					{
						text2,
						variable.OrgName,
						" := ",
						\u0002.LValue,
						";"
					}));
				}
			}
		}

		// Token: 0x06002143 RID: 8515 RVA: 0x000715F4 File Offset: 0x0006F7F4
		public virtual void \u0001(\u0080.\u0010 \u0002)
		{
			this.StrBuilder.AppendLine("pConcatenationBuffer := ADR(ConcatenationBuffer);");
			this.StrBuilder.AppendLine("pConcatenationBuffer^ := '';");
			this.CurrentNumOfCharactersInConcatenatedString = 0;
		}

		// Token: 0x06002144 RID: 8516 RVA: 0x00071620 File Offset: 0x0006F820
		public virtual void \u0001(global::\u000F.\u0008 \u0002)
		{
			this.StrBuilder.AppendLine(string.Format("__StringCopyChecked(pSource := pConcatenationBuffer, pDest := ADR({0}), nCharacterLimit:={1}, maxbuffersize:={2});", \u0002.LValue, this.CharacterLimit, this.MaxBufferSize));
			if (this.CurrentNumOfCharactersInConcatenatedString > this.MaxNumOfCharactersInConcatenatedString)
			{
				this.MaxNumOfCharactersInConcatenatedString = this.CurrentNumOfCharactersInConcatenatedString;
			}
		}

		// Token: 0x06002145 RID: 8517 RVA: 0x00071674 File Offset: 0x0006F874
		public virtual void \u0001(global::\u0012.\u000F \u0002)
		{
			string text = \u0002.Literal;
			if (APEnvironmentFacade.Instance.CompileOptions.UnicodeIdentifiers)
			{
				byte[] bytes = Encoding.UTF8.GetBytes(text);
				text = Encoding.UTF8.GetString(bytes);
			}
			this.StrBuilder.AppendLine(string.Format("__StringAppend(STR1 := pConcatenationBuffer, STR2 := '{0}', maxbuffersize := {1});", text, this.MaxBufferSize));
			this.CurrentNumOfCharactersInConcatenatedString += text.Length;
		}

		// Token: 0x06002146 RID: 8518 RVA: 0x000716E4 File Offset: 0x0006F8E4
		protected virtual int \u0001(_IArrayType \u0002)
		{
			int num = 2;
			int num2 = \u0002.Dimensions.Count<IArrayDimension>() - 1;
			int num3 = num2;
			int num4 = 0;
			bool flag = false;
			foreach (IArrayDimension arrayDimension in \u0002.Dimensions)
			{
				string text = arrayDimension.LowerBorderInt(out flag, this.Scope).ToString();
				string text2 = arrayDimension.UpperBorderInt(out flag, this.Scope).ToString();
				num4 += Math.Max(text.Length, text2.Length);
			}
			int num5 = num + num2 + num3 + num4;
			if (\u0002._Base.Class == TypeClass.Array)
			{
				num5 += this.\u0001(\u0002._Base as _IArrayType);
			}
			return num5;
		}

		// Token: 0x06002147 RID: 8519 RVA: 0x000717A0 File Offset: 0x0006F9A0
		protected virtual void \u0001(_IArrayType \u0002, int \u0003)
		{
			this.StrBuilder.AppendLine(string.Format("__StringAppend(pConcatenationBuffer, '[', maxbuffersize:={0});", this.MaxBufferSize));
			this.StrBuilder.AppendLine(string.Format("__APPEND_INT_TO_STRING(pConcatenationBuffer, Index_{0}, maxbuffersize:={1});", \u0003, this.MaxBufferSize));
			this.\u0005(\u0003);
			for (int i = 1; i < \u0002.Dimensions.Count<IArrayDimension>(); i++)
			{
				this.StrBuilder.AppendLine(string.Format("__StringAppend(pConcatenationBuffer, ', ', maxbuffersize:={0});", this.MaxBufferSize));
				this.StrBuilder.AppendLine(string.Format("__APPEND_INT_TO_STRING(pConcatenationBuffer, Index_{0}, maxbuffersize:={1});", \u0003 + i, this.MaxBufferSize));
				this.\u0005(\u0003 + i);
			}
			this.StrBuilder.AppendLine(string.Format("__StringAppend(pConcatenationBuffer, ']', maxbuffersize:={0});", this.MaxBufferSize));
			int u = \u0003 + \u0002.Dimensions.Count<IArrayDimension>();
			if (\u0002._Base.Class == TypeClass.Array)
			{
				this.\u0001(\u0002._Base as _IArrayType, u);
			}
		}

		// Token: 0x06002148 RID: 8520 RVA: 0x0007189C File Offset: 0x0006FA9C
		public void \u0001(\u0082.\u0008 \u0002)
		{
			this.\u0001(\u0002.ArrayType, \u0002.StartIndex);
			this.CurrentNumOfCharactersInConcatenatedString += this.\u0001(\u0002.ArrayType);
		}

		// Token: 0x06002149 RID: 8521 RVA: 0x000718CC File Offset: 0x0006FACC
		public void \u0005(int \u0002)
		{
			if (\u0002 + 1 > this.NumArrayIndexVariables)
			{
				this.NumArrayIndexVariables = \u0002 + 1;
			}
		}

		// Token: 0x0600214A RID: 8522 RVA: 0x000718E4 File Offset: 0x0006FAE4
		private static bool \u0001(global::\u0008.\u0008 \u0002)
		{
			\u0082.\u000E u000E = \u0002 as \u0082.\u000E;
			return u000E != null && u000E.Controlled.All(new Func<global::\u0008.\u0008, bool>(global::\u0016.\u000E.\u0001));
		}

		// Token: 0x0600214B RID: 8523 RVA: 0x00071914 File Offset: 0x0006FB14
		public void \u0001(\u0082.\u000E \u0002)
		{
			if (global::\u0016.\u000E.\u0001(\u0002))
			{
				return;
			}
			_IArrayType iarrayType = \u0002.ArrayType;
			int num = \u0002.ArrayIndex;
			int num2 = 0;
			while (iarrayType != null)
			{
				int i = 0;
				while (i < iarrayType._Dimensions.Count<_IArrayDimension>())
				{
					bool flag;
					this.StrBuilder.AppendLine(string.Format("FOR Index_{0} := {1} TO {2} DO", num, iarrayType._Dimensions[i].LowerBorderInt(out flag, this.Scope), iarrayType._Dimensions[i].UpperBorderInt(out flag, this.Scope)));
					this.StrBuilder.AppendLine();
					this.\u0005(num);
					i++;
					num2++;
					num++;
				}
				iarrayType = (iarrayType._Base as _IArrayType);
			}
			this.\u0001(\u0002.Controlled);
			for (int j = 0; j < num2; j++)
			{
				this.StrBuilder.AppendLine("END_FOR");
			}
		}

		// Token: 0x04000584 RID: 1412
		[CompilerGenerated]
		private int \u0001;

		// Token: 0x04000585 RID: 1413
		[CompilerGenerated]
		private LList<IVariable> \u0001;

		// Token: 0x04000586 RID: 1414
		[CompilerGenerated]
		private IScope \u0001;

		// Token: 0x04000587 RID: 1415
		[CompilerGenerated]
		private LStringBuilder \u0001;

		// Token: 0x04000588 RID: 1416
		[CompilerGenerated]
		private int \u0002;

		// Token: 0x04000589 RID: 1417
		[CompilerGenerated]
		private int \u0003;

		// Token: 0x0400058A RID: 1418
		[CompilerGenerated]
		private int \u0004;
	}
}

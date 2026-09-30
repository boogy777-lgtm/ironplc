using System;
using System.Linq;
using \u000F;
using \u0012;
using \u0016;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0080;
using \u0083;

namespace \u0019
{
	// Token: 0x020001E2 RID: 482
	internal sealed class \u0004 : global::\u0016.\u000E
	{
		// Token: 0x0600214C RID: 8524 RVA: 0x00071A10 File Offset: 0x0006FC10
		public \u0004(int \u009F\u0003, IScope \u009B\u0002, LList<IVariable> \u0001\u0004, LStringBuilder \u0002\u0004) : base(\u009F\u0003, \u009B\u0002, \u0001\u0004, \u0002\u0004)
		{
		}

		// Token: 0x17000636 RID: 1590
		// (get) Token: 0x0600214D RID: 8525 RVA: 0x00071A20 File Offset: 0x0006FC20
		public override string MaxBufferSize
		{
			get
			{
				return "MAXWBUFFER";
			}
		}

		// Token: 0x0600214E RID: 8526 RVA: 0x00071A28 File Offset: 0x0006FC28
		public override void \u0001(\u0083.\u0004 \u0002)
		{
			string text = \u0002.RValue;
			if (\u0002.RValue.Length > base.CharacterLimit)
			{
				text = text.Substring(text.Length - base.CharacterLimit + 2);
				text = text.Insert(0, "..");
			}
			base.StrBuilder.AppendLine(\u0002.LValue + " := \"" + text + "\";");
			if (base.Vars.Count > 0)
			{
				string text2 = \u0002.LValue.Substring(0, \u0002.LValue.LastIndexOf('.') + 1);
				foreach (IVariable variable in base.Vars)
				{
					base.StrBuilder.AppendLine(string.Concat(new string[]
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

		// Token: 0x0600214F RID: 8527 RVA: 0x00071B38 File Offset: 0x0006FD38
		public override void \u0001(\u0080.\u0010 \u0002)
		{
			base.StrBuilder.AppendLine("pWConcatenationBuffer := ADR(WConcatenationBuffer);");
			base.StrBuilder.AppendLine("pWConcatenationBuffer^ := \"\";");
			base.CurrentNumOfCharactersInConcatenatedString = 0;
		}

		// Token: 0x06002150 RID: 8528 RVA: 0x00071B64 File Offset: 0x0006FD64
		public override void \u0001(global::\u000F.\u0008 \u0002)
		{
			base.StrBuilder.AppendLine(string.Format("__WStringCopyChecked(pSource := pWConcatenationBuffer, pDest := ADR({0}), nCharacterLimit:={1}, maxbuffersize:={2});", \u0002.LValue, base.CharacterLimit, this.MaxBufferSize));
			base.MaxNumOfCharactersInConcatenatedString = Math.Max(base.CurrentNumOfCharactersInConcatenatedString, base.MaxNumOfCharactersInConcatenatedString);
		}

		// Token: 0x06002151 RID: 8529 RVA: 0x00071BB8 File Offset: 0x0006FDB8
		public override void \u0001(global::\u0012.\u000F \u0002)
		{
			string text = \u0002.Literal;
			base.StrBuilder.AppendLine(string.Format("__WStringAppend(STR1 := pWConcatenationBuffer, STR2 := \"{0}\", maxbuffersize := {1});", text, this.MaxBufferSize));
			base.CurrentNumOfCharactersInConcatenatedString += text.Length;
		}

		// Token: 0x06002152 RID: 8530 RVA: 0x00071BFC File Offset: 0x0006FDFC
		protected override void \u0001(_IArrayType \u0002, int \u0003)
		{
			base.StrBuilder.AppendLine(string.Format("__WStringAppend(pWConcatenationBuffer, \"[\", maxbuffersize := {0});", this.MaxBufferSize));
			base.StrBuilder.AppendLine(string.Format("__APPEND_INT_TO_WSTRING(pWConcatenationBuffer, Index_{0}, maxbuffersize := {1});", \u0003, this.MaxBufferSize));
			base.\u0005(\u0003);
			for (int i = 1; i < \u0002.Dimensions.Count<IArrayDimension>(); i++)
			{
				base.StrBuilder.AppendLine(string.Format("__WStringAppend(pWConcatenationBuffer, \", \", maxbuffersize := {0});", this.MaxBufferSize));
				base.StrBuilder.AppendLine(string.Format("__APPEND_INT_TO_WSTRING(pWConcatenationBuffer, Index_{0}, maxbuffersize := {1});", \u0003 + i, this.MaxBufferSize));
				base.\u0005(\u0003 + i);
			}
			base.StrBuilder.AppendLine(string.Format("__WStringAppend(pWConcatenationBuffer, \"]\", maxbuffersize := {0});", this.MaxBufferSize));
			int u = \u0003 + \u0002.Dimensions.Count<IArrayDimension>();
			if (\u0002._Base.Class == TypeClass.Array)
			{
				this.\u0001(\u0002._Base as _IArrayType, u);
			}
		}
	}
}

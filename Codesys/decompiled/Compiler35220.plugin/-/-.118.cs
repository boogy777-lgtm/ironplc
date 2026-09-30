using System;
using System.Collections.Generic;
using \u000E;
using \u0011;
using \u0019;
using CODESYS.Parser;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0003
{
	// Token: 0x02000162 RID: 354
	internal sealed class \u0006 : IErrorHandler2, IErrorHandler
	{
		// Token: 0x06001842 RID: 6210 RVA: 0x0004BC9C File Offset: 0x00049E9C
		public \u0006(global::\u0011.\u0006 \u0007\u0006)
		{
			this.\u0001 = \u0007\u0006;
		}

		// Token: 0x06001843 RID: 6211 RVA: 0x0004BCAC File Offset: 0x00049EAC
		public void \u0001(_IExprement \u0002, MessageId \u0003, params object[] \u0004)
		{
			global::\u0003.\u0006.\u0002(\u0002, \u0003, \u0004);
		}

		// Token: 0x06001844 RID: 6212 RVA: 0x0004BCB8 File Offset: 0x00049EB8
		public void \u0001(_IExprement \u0002, IToken \u0003, MessageId \u0004, params object[] \u0005)
		{
			global::\u0003.\u0006.\u0001(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x06001845 RID: 6213 RVA: 0x0004BCC4 File Offset: 0x00049EC4
		public void \u0002(_IExprement \u0002, IToken \u0003, MessageId \u0004, params object[] \u0005)
		{
			global::\u0003.\u0006.\u0001(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x06001846 RID: 6214 RVA: 0x0004BCD0 File Offset: 0x00049ED0
		public void \u0001(_IExprement \u0002, Severity \u0003, MessageId \u0004, params object[] \u0005)
		{
			global::\u0003.\u0006.\u0001(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x06001847 RID: 6215 RVA: 0x0004BCDC File Offset: 0x00049EDC
		public void \u0001(_IExprement \u0002, IToken \u0003, Severity \u0004, MessageId \u0005, params object[] \u0006)
		{
			global::\u0003.\u0006.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006);
		}

		// Token: 0x06001848 RID: 6216 RVA: 0x0004BCEC File Offset: 0x00049EEC
		public void \u0002(_IExprement \u0002, MessageId \u0003, params object[] \u0004)
		{
			global::\u0003.\u0006.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x06001849 RID: 6217 RVA: 0x0004BCF8 File Offset: 0x00049EF8
		public string \u0001(MessageId \u0002, params object[] \u0003)
		{
			return global::\u0003.\u0006.\u0001(\u0002, \u0003);
		}

		// Token: 0x0600184A RID: 6218 RVA: 0x0004BD04 File Offset: 0x00049F04
		public void \u0001(IToken \u0002, MessageId \u0003, params object[] \u0004)
		{
			string format = \u0018.\u0001(\u0003);
			if (this.\u0001.Signature == null)
			{
				if (this.\u0001.FirstMessage == null)
				{
					this.\u0001.FirstMessage = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(-1, Guid.Empty, \u0002.Position, \u0002.PositionOffset, (short)\u0002.Length), string.Format(format, \u0004), Severity.Error, \u0003);
				}
				return;
			}
			if (this.\u0001.MessageGuid != Guid.Empty)
			{
				this.\u0001.Signature.AddError(\u0002, string.Format(format, \u0004), this.\u0001.MessageGuid, \u0003);
				return;
			}
			this.\u0001.Signature.AddError(\u0002, string.Format(format, \u0004), \u0003);
		}

		// Token: 0x0600184B RID: 6219 RVA: 0x0004BDC0 File Offset: 0x00049FC0
		private static IMinimalPosition \u0001(IToken \u0002, out short \u0003)
		{
			\u0003 = (short)\u0002.Length;
			return \u0019.\u0003.\u0001(\u0002.Position, \u0002.PositionOffset);
		}

		// Token: 0x0600184C RID: 6220 RVA: 0x0004BDDC File Offset: 0x00049FDC
		private static void \u0001(_IExprement \u0002, IToken \u0003, Severity \u0004, MessageId \u0005, params object[] \u0006)
		{
			string stMessage = string.Format(\u0018.\u0001(\u0005), \u0006);
			short sLength;
			IMinimalPosition sourcepos = global::\u0003.\u0006.\u0001(\u0003, out sLength);
			\u0002.AddMessage(stMessage, sourcepos, \u0004, sLength, \u0005);
		}

		// Token: 0x0600184D RID: 6221 RVA: 0x0004BE0C File Offset: 0x0004A00C
		internal static string \u0001(MessageId \u0002, params object[] \u0003)
		{
			return string.Format(\u0018.\u0001(\u0002), \u0003);
		}

		// Token: 0x0600184E RID: 6222 RVA: 0x0004BE1C File Offset: 0x0004A01C
		internal static void \u0001(_IExprement \u0002, MessageId \u0003, params object[] \u0004)
		{
			string stWarning = string.Format(\u0018.\u0001(\u0003), \u0004);
			\u0002.AddWarning(stWarning, \u0003);
		}

		// Token: 0x0600184F RID: 6223 RVA: 0x0004BE40 File Offset: 0x0004A040
		internal static void \u0001(_IExprement \u0002, Severity \u0003, MessageId \u0004, params object[] \u0005)
		{
			string stMessage = string.Format(\u0018.\u0001(\u0004), \u0005);
			\u0002.AddMessage(stMessage, \u0002._Position, \u0003, \u0002.PositionLength, \u0004);
		}

		// Token: 0x06001850 RID: 6224 RVA: 0x0004BE70 File Offset: 0x0004A070
		internal static void \u0002(_IExprement \u0002, MessageId \u0003, params object[] \u0004)
		{
			string stError = string.Format(\u0018.\u0001(\u0003), \u0004);
			\u0002.AddError(stError, \u0003);
		}

		// Token: 0x06001851 RID: 6225 RVA: 0x0004BE94 File Offset: 0x0004A094
		internal static void \u0001(_IExprement \u0002, IToken \u0003, MessageId \u0004, params object[] \u0005)
		{
			string stError = string.Format(\u0018.\u0001(\u0004), \u0005);
			\u0002.AddError(stError, \u0003, \u0004);
		}

		// Token: 0x06001852 RID: 6226 RVA: 0x0004BEB8 File Offset: 0x0004A0B8
		private static IEnumerable<IMessage> \u0001(_ISequenceStatement \u0002)
		{
			if (\u0002 == null)
			{
				return Array.Empty<IMessage>();
			}
			ErrorVisitor errorVisitor = new ErrorVisitor();
			\u0002.Accept(errorVisitor);
			return errorVisitor.Messages;
		}

		// Token: 0x06001853 RID: 6227 RVA: 0x0004BEE4 File Offset: 0x0004A0E4
		private static IEnumerable<IMessage> \u0001(IPOUSyntax \u0002)
		{
			List<IMessage> list = new List<IMessage>();
			list.AddRange(global::\u0003.\u0006.\u0001(\u0002.Declaration));
			list.AddRange(global::\u0003.\u0006.\u0001(\u0002.Implementation));
			foreach (IPOUSyntax u in \u0002.SubPOUs)
			{
				list.AddRange(global::\u0003.\u0006.\u0001(u));
			}
			return list;
		}

		// Token: 0x06001854 RID: 6228 RVA: 0x0004BF60 File Offset: 0x0004A160
		public IEnumerable<IMessage> \u0001(IEnumerable<IPOUSyntax> \u0002)
		{
			List<IMessage> list = new List<IMessage>();
			foreach (IPOUSyntax u in \u0002)
			{
				list.AddRange(global::\u0003.\u0006.\u0001(u));
			}
			return list;
		}

		// Token: 0x0400044A RID: 1098
		private readonly global::\u0011.\u0006 \u0001;
	}
}

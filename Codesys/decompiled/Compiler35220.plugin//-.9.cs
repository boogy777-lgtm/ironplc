using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u000E;
using \u0019;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0084
{
	// Token: 0x020001A8 RID: 424
	internal sealed class \u000E
	{
		// Token: 0x06001EB2 RID: 7858 RVA: 0x00062E68 File Offset: 0x00061068
		private \u000E(_ISignature \u0094\u0003, IScope2 \u009B\u0002)
		{
			this.\u0001 = \u009B\u0002;
			this.\u0001 = new Dictionary<int, \u0084.\u000E.\u0001>();
			this.\u0001 = new Stack<_ISignature>();
		}

		// Token: 0x06001EB3 RID: 7859 RVA: 0x00062E90 File Offset: 0x00061090
		internal static bool \u0001(_ISignature \u0002, IScope2 \u0003)
		{
			\u0084.\u000E u000E = new \u0084.\u000E(\u0002, \u0003);
			u000E.\u0001(\u0002, true);
			return u000E.RecursionDectected;
		}

		// Token: 0x06001EB4 RID: 7860 RVA: 0x00062EA8 File Offset: 0x000610A8
		private void \u0001(_ISignature \u0002, bool \u0003)
		{
			this.\u0001.Push(\u0002);
			if (\u0003)
			{
				this.\u0001.Add(\u0002.Id, \u0084.\u000E.\u0001.\u0002);
			}
			else
			{
				\u0084.\u000E.\u0001 u = \u0084.\u000E.\u0001.\u0001;
				if (this.\u0001.TryGetValue(\u0002.Id, out u))
				{
					this.RecursionDectected = (\u0084.\u000E.\u0001.\u0002 == u);
				}
				else
				{
					this.\u0001.Add(\u0002.Id, \u0084.\u000E.\u0001.\u0002);
				}
			}
			if (this.RecursionDectected)
			{
				ISourcePosition sourcePosition = null;
				if (sourcePosition == null && \u0002.InterfaceExpressions != null && \u0002.InterfaceExpressions.Length != 0)
				{
					sourcePosition = \u0002.InterfaceExpressions[0].Position;
				}
				if (sourcePosition == null)
				{
					IExpression baseExpression = \u0002.BaseExpression;
					sourcePosition = ((baseExpression != null) ? baseExpression.Position : null);
				}
				LStringBuilder lstringBuilder = new LStringBuilder();
				_ISignature[] array = this.\u0001.ToArray();
				bool flag = false;
				for (int i = array.Length - 1; i >= 0; i--)
				{
					if (flag)
					{
						lstringBuilder.Append(" -> ");
					}
					lstringBuilder.Append(array[i].OrgName);
					flag = true;
				}
				\u0002.AddError(\u0019.\u0003.\u0001(sourcePosition, \u0018.\u0001(MessageId.Err_SelfInheritance, new object[]
				{
					lstringBuilder.ToString()
				}), Severity.FatalError, MessageId.Err_SelfInheritance));
				Debug.\u0004(true);
				return;
			}
			if (\u0002.BaseSignatureId != Helper.InvalidId)
			{
				ISignature signature = this.\u0001[\u0002.BaseSignatureId];
				this.\u0001((_ISignature)signature, false);
			}
			foreach (int nId in \u0002.InterfaceIds)
			{
				if (!this.RecursionDectected)
				{
					ISignature signature2 = this.\u0001[nId];
					this.\u0001((_ISignature)signature2, false);
				}
			}
			if (!this.RecursionDectected)
			{
				this.\u0001.Pop();
			}
			this.\u0001[\u0002.Id] = \u0084.\u000E.\u0001.\u0003;
		}

		// Token: 0x170005E3 RID: 1507
		// (get) Token: 0x06001EB5 RID: 7861 RVA: 0x00063068 File Offset: 0x00061268
		// (set) Token: 0x06001EB6 RID: 7862 RVA: 0x00063070 File Offset: 0x00061270
		private bool RecursionDectected { get; set; }

		// Token: 0x040004EE RID: 1262
		private Dictionary<int, \u0084.\u000E.\u0001> \u0001;

		// Token: 0x040004EF RID: 1263
		private Stack<_ISignature> \u0001;

		// Token: 0x040004F0 RID: 1264
		private IScope2 \u0001;

		// Token: 0x040004F1 RID: 1265
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x020001A9 RID: 425
		private enum \u0001
		{
			// Token: 0x040004F3 RID: 1267
			\u0001,
			// Token: 0x040004F4 RID: 1268
			\u0002,
			// Token: 0x040004F5 RID: 1269
			\u0003
		}
	}
}

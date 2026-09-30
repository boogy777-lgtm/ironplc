using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0015
{
	// Token: 0x020000D2 RID: 210
	internal sealed class \u0001
	{
		// Token: 0x06000EE3 RID: 3811 RVA: 0x00029230 File Offset: 0x00027430
		public \u0001(IgnoreAttributes \u0007\u0002)
		{
			this.\u0001 = \u0007\u0002;
		}

		// Token: 0x06000EE4 RID: 3812 RVA: 0x00029240 File Offset: 0x00027440
		public bool \u0001(IDictionary<string, string> \u0002, IDictionary<string, string> \u0003)
		{
			int num = this.\u0001(\u0002);
			int num2 = this.\u0001(\u0003);
			if (num != num2)
			{
				return false;
			}
			if (num == 0)
			{
				return true;
			}
			foreach (KeyValuePair<string, string> keyValuePair in \u0002)
			{
				if (!this.\u0001(keyValuePair.Key))
				{
					if (!\u0003.ContainsKey(keyValuePair.Key))
					{
						return false;
					}
					if (\u0003[keyValuePair.Key] != keyValuePair.Value)
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06000EE5 RID: 3813 RVA: 0x000292E4 File Offset: 0x000274E4
		private int \u0001(IDictionary<string, string> \u0002)
		{
			int num = 0;
			if (\u0002 != null)
			{
				foreach (string u in \u0002.Keys)
				{
					if (!this.\u0001(u))
					{
						num++;
					}
				}
			}
			return num;
		}

		// Token: 0x06000EE6 RID: 3814 RVA: 0x00029340 File Offset: 0x00027540
		private bool \u0001(string \u0002)
		{
			return ((this.\u0001 & IgnoreAttributes.Comment) != (IgnoreAttributes)0U && \u0002 == CompileAttributes.ATTRIBUTE_COMMENT) || ((this.\u0001 & IgnoreAttributes.DocuComment) != (IgnoreAttributes)0U && \u0002 == CompileAttributes.ATTRIBUTE_DOCUCOMMENT) || ((this.\u0001 & IgnoreAttributes.MessageGuid) != (IgnoreAttributes)0U && \u0002 == CompileAttributes.ATTRIBUTE_MESSAGE_GUID) || ((this.\u0001 & IgnoreAttributes.SignatureCRC) != (IgnoreAttributes)0U && \u0002 == CompileAttributes.ATTRIBUTE_SIGNATURE_CRC) || ((this.\u0001 & IgnoreAttributes.VarLenArrayOriginalScope) != (IgnoreAttributes)0U && \u0002 == "variable_length_array_original_scope");
		}

		// Token: 0x0400029F RID: 671
		private readonly IgnoreAttributes \u0001;
	}
}

using System;
using \u0003;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration
{
	// Token: 0x02000240 RID: 576
	public class BlobInitException : Exception
	{
		// Token: 0x060025C3 RID: 9667 RVA: 0x0008307C File Offset: 0x0008127C
		public BlobInitException(_IExprement exp, _IVariable var)
		{
			this.\u0001 = exp;
			this.\u0001 = var;
		}

		// Token: 0x170006F7 RID: 1783
		// (get) Token: 0x060025C4 RID: 9668 RVA: 0x00083094 File Offset: 0x00081294
		public _IExprement _IExprement
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x170006F8 RID: 1784
		// (get) Token: 0x060025C5 RID: 9669 RVA: 0x0008309C File Offset: 0x0008129C
		internal MessageId MessageId
		{
			get
			{
				return MessageId.Err_InvalidInitialisationForArray;
			}
		}

		// Token: 0x170006F9 RID: 1785
		// (get) Token: 0x060025C6 RID: 9670 RVA: 0x000830A4 File Offset: 0x000812A4
		public override string Message
		{
			get
			{
				if (this.\u0001 == null)
				{
					return \u0006.\u0001(MessageId.Err_InvalidInitialisationForArray, new object[]
					{
						"???",
						this.\u0001.Name
					});
				}
				return \u0006.\u0001(MessageId.Err_InvalidInitialisationForArray, new object[]
				{
					this.\u0001.ToString(),
					this.\u0001.Name
				});
			}
		}

		// Token: 0x040006E2 RID: 1762
		private readonly _IExprement \u0001;

		// Token: 0x040006E3 RID: 1763
		private readonly _IVariable \u0001;
	}
}

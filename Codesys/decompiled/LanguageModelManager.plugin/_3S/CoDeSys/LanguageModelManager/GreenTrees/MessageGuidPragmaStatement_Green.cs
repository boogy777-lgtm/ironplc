using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001F0 RID: 496
	internal class MessageGuidPragmaStatement_Green : PragmaStatement_Green, _IMessageGuidPragmaStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement
	{
		// Token: 0x06002259 RID: 8793 RVA: 0x0005AC0F File Offset: 0x00059C0F
		public MessageGuidPragmaStatement_Green(Guid messageGuid, string stText) : base(stText)
		{
			this.m_messageGuid = messageGuid;
		}

		// Token: 0x1700098A RID: 2442
		// (get) Token: 0x0600225A RID: 8794 RVA: 0x0005AC1F File Offset: 0x00059C1F
		// (set) Token: 0x0600225B RID: 8795 RVA: 0x0000677E File Offset: 0x0000577E
		public Guid MessageGuid
		{
			get
			{
				return this.m_messageGuid;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x04000698 RID: 1688
		private Guid m_messageGuid;
	}
}

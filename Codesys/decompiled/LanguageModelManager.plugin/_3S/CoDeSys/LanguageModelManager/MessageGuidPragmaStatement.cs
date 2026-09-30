using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000096 RID: 150
	[TypeGuid("{a31824c6-40b4-4be7-bc88-33c49a0bd4f7}")]
	[StorageVersion("3.3.0.0")]
	public class MessageGuidPragmaStatement : PragmaStatement, _IMessageGuidPragmaStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement
	{
		// Token: 0x0600092D RID: 2349 RVA: 0x0001576B File Offset: 0x0001476B
		public MessageGuidPragmaStatement()
		{
		}

		// Token: 0x0600092E RID: 2350 RVA: 0x0001577E File Offset: 0x0001477E
		public MessageGuidPragmaStatement(IToken token, Guid guid) : base(token)
		{
			this.m_messageGuid = guid;
		}

		// Token: 0x17000217 RID: 535
		// (get) Token: 0x0600092F RID: 2351 RVA: 0x00015799 File Offset: 0x00014799
		// (set) Token: 0x06000930 RID: 2352 RVA: 0x000157A1 File Offset: 0x000147A1
		public Guid MessageGuid
		{
			get
			{
				return this.m_messageGuid;
			}
			set
			{
				this.m_messageGuid = value;
			}
		}

		// Token: 0x06000931 RID: 2353 RVA: 0x000157AC File Offset: 0x000147AC
		public override _IExprement Duplicate()
		{
			MessageGuidPragmaStatement messageGuidPragmaStatement = new MessageGuidPragmaStatement();
			this.DuplicateCommon(messageGuidPragmaStatement);
			messageGuidPragmaStatement.m_stText = this.m_stText;
			messageGuidPragmaStatement.m_messageGuid = this.m_messageGuid;
			return messageGuidPragmaStatement;
		}

		// Token: 0x04000140 RID: 320
		[DefaultSerialization("Guid")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Guid m_messageGuid = Guid.Empty;
	}
}

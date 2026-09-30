using System;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000095 RID: 149
	[TypeGuid("28bfcfa9-e407-4eb9-bb19-d41917848448")]
	[StorageVersion("3.5.21.0")]
	[SuppressMessage("Major Code Smell", "S110: Depth of inheritance", Justification = "hard to change")]
	[SuppressMessage("Major Code Smell", "S1939: double inheritance", Justification = "false positive")]
	public class LocalSignatureIdPragma : PragmaStatement, _ILocalSignatureIdPragma, _IPragmaStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IPragmaStatement
	{
		// Token: 0x06000928 RID: 2344 RVA: 0x000156FF File Offset: 0x000146FF
		public LocalSignatureIdPragma()
		{
			this.LocalSignatureId = -1;
		}

		// Token: 0x06000929 RID: 2345 RVA: 0x0001570E File Offset: 0x0001470E
		public LocalSignatureIdPragma(IToken token, string stText, int nId) : base(token)
		{
			this.LocalSignatureId = nId;
			base.Text = stText;
		}

		// Token: 0x17000216 RID: 534
		// (get) Token: 0x0600092A RID: 2346 RVA: 0x00015725 File Offset: 0x00014725
		// (set) Token: 0x0600092B RID: 2347 RVA: 0x0001572D File Offset: 0x0001472D
		public int LocalSignatureId { get; set; }

		// Token: 0x0600092C RID: 2348 RVA: 0x00015738 File Offset: 0x00014738
		public override _IExprement Duplicate()
		{
			LocalSignatureIdPragma localSignatureIdPragma = new LocalSignatureIdPragma();
			this.DuplicateCommon(localSignatureIdPragma);
			localSignatureIdPragma.m_stText = this.m_stText;
			localSignatureIdPragma.LocalSignatureId = this.LocalSignatureId;
			return localSignatureIdPragma;
		}
	}
}

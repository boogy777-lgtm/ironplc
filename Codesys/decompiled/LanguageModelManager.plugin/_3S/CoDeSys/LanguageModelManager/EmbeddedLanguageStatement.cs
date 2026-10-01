using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000089 RID: 137
	[TypeGuid("{25BCF8C7-55BC-4E03-99AE-380493834FE5}")]
	[StorageVersion("3.5.22.0")]
	public class EmbeddedLanguageStatement : PositionStatement, _IEmbeddedLanguageStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement
	{
		// Token: 0x0600089C RID: 2204 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void Accept(IExprementVisitor visitor)
		{
		}

		// Token: 0x0600089D RID: 2205 RVA: 0x00014C50 File Offset: 0x00013C50
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return default(T);
		}

		// Token: 0x0600089E RID: 2206 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x0600089F RID: 2207 RVA: 0x00014C68 File Offset: 0x00013C68
		public override _IExprement Duplicate()
		{
			EmbeddedLanguageStatement embeddedLanguageStatement = new EmbeddedLanguageStatement();
			this.DuplicateCommon(embeddedLanguageStatement);
			embeddedLanguageStatement.Kind = this.Kind;
			embeddedLanguageStatement.RawSegment = this.RawSegment;
			return embeddedLanguageStatement;
		}

		// Token: 0x170001EE RID: 494
		// (get) Token: 0x060008A0 RID: 2208 RVA: 0x00014C9B File Offset: 0x00013C9B
		// (set) Token: 0x060008A1 RID: 2209 RVA: 0x00014CA3 File Offset: 0x00013CA3
		public string Kind { get; set; }

		// Token: 0x170001EF RID: 495
		// (get) Token: 0x060008A2 RID: 2210 RVA: 0x00014CAC File Offset: 0x00013CAC
		// (set) Token: 0x060008A3 RID: 2211 RVA: 0x00014CB4 File Offset: 0x00013CB4
		public ArraySegment<char> RawSegment { get; set; }
	}
}

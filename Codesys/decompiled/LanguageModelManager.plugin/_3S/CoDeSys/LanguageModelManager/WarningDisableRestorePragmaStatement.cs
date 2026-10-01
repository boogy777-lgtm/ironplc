using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000A8 RID: 168
	[TypeGuid("{419E40AE-741C-4142-9B23-ADFC94EECA1A}")]
	[StorageVersion("3.5.0.0")]
	public class WarningDisableRestorePragmaStatement : PragmaStatement, _IWarningDisableRestorePragmaStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IWarningDisableRestorePragmaStatement
	{
		// Token: 0x06000A27 RID: 2599 RVA: 0x000171BB File Offset: 0x000161BB
		public WarningDisableRestorePragmaStatement()
		{
		}

		// Token: 0x06000A28 RID: 2600 RVA: 0x000171CE File Offset: 0x000161CE
		public WarningDisableRestorePragmaStatement(IToken token, bool bRestore, string stId) : base(token)
		{
			this._bRestore = bRestore;
			this._stId = stId;
		}

		// Token: 0x17000269 RID: 617
		// (get) Token: 0x06000A29 RID: 2601 RVA: 0x000171F0 File Offset: 0x000161F0
		// (set) Token: 0x06000A2A RID: 2602 RVA: 0x000171F8 File Offset: 0x000161F8
		public bool Restore
		{
			get
			{
				return this._bRestore;
			}
			set
			{
				this._bRestore = value;
			}
		}

		// Token: 0x1700026A RID: 618
		// (get) Token: 0x06000A2B RID: 2603 RVA: 0x00017201 File Offset: 0x00016201
		// (set) Token: 0x06000A2C RID: 2604 RVA: 0x00017209 File Offset: 0x00016209
		public string Id
		{
			get
			{
				return this._stId;
			}
			set
			{
				this._stId = value;
			}
		}

		// Token: 0x06000A2D RID: 2605 RVA: 0x00017214 File Offset: 0x00016214
		public override _IExprement Duplicate()
		{
			WarningDisableRestorePragmaStatement warningDisableRestorePragmaStatement = new WarningDisableRestorePragmaStatement();
			this.DuplicateCommon(warningDisableRestorePragmaStatement);
			warningDisableRestorePragmaStatement._bRestore = this._bRestore;
			warningDisableRestorePragmaStatement._stId = this._stId;
			return warningDisableRestorePragmaStatement;
		}

		// Token: 0x04000177 RID: 375
		[DefaultSerialization("restore")]
		[StorageVersion("3.5.0.0")]
		[Obfuscation(Feature = "rename")]
		private bool _bRestore;

		// Token: 0x04000178 RID: 376
		[DefaultSerialization("id")]
		[StorageVersion("3.5.0.0")]
		[Obfuscation(Feature = "rename")]
		private string _stId = string.Empty;
	}
}

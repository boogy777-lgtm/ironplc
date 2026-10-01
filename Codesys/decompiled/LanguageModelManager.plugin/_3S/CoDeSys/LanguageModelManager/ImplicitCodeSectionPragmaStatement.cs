using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000092 RID: 146
	[TypeGuid("3e31bd6e-8751-4287-bae7-6af8d4a73819")]
	[StorageVersion("3.5.21.0")]
	[SuppressMessage("Major Code Smell", "S110: Depth of inheritance", Justification = "hard to change")]
	[SuppressMessage("Major Code Smell", "S1939: double inheritance", Justification = "false positive")]
	public class ImplicitCodeSectionPragmaStatement : PragmaStatement, _IImplicitCodeSectionPragma, _IPragmaStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IPragmaStatement
	{
		// Token: 0x0600090D RID: 2317 RVA: 0x000154E0 File Offset: 0x000144E0
		public ImplicitCodeSectionPragmaStatement()
		{
			this._bOn = false;
		}

		// Token: 0x0600090E RID: 2318 RVA: 0x000154EF File Offset: 0x000144EF
		public ImplicitCodeSectionPragmaStatement(IToken token, string stText, bool bOn) : base(token)
		{
			this._bOn = bOn;
			base.Text = stText;
		}

		// Token: 0x17000210 RID: 528
		// (get) Token: 0x0600090F RID: 2319 RVA: 0x00015506 File Offset: 0x00014506
		// (set) Token: 0x06000910 RID: 2320 RVA: 0x0001550E File Offset: 0x0001450E
		public bool ImplicitOn
		{
			get
			{
				return this._bOn;
			}
			set
			{
				this._bOn = value;
			}
		}

		// Token: 0x06000911 RID: 2321 RVA: 0x00015518 File Offset: 0x00014518
		public override _IExprement Duplicate()
		{
			ImplicitCodeSectionPragmaStatement implicitCodeSectionPragmaStatement = new ImplicitCodeSectionPragmaStatement();
			this.DuplicateCommon(implicitCodeSectionPragmaStatement);
			implicitCodeSectionPragmaStatement.m_stText = this.m_stText;
			implicitCodeSectionPragmaStatement.ImplicitOn = this.ImplicitOn;
			return implicitCodeSectionPragmaStatement;
		}

		// Token: 0x0400013B RID: 315
		[DefaultSerialization("bOn")]
		[StorageVersion("3.5.21.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private bool _bOn;
	}
}

using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000094 RID: 148
	[TypeGuid("{b8592538-d1b7-4a14-9075-014be38262fe}")]
	[StorageVersion("3.3.0.0")]
	public class LabelStatement : PositionStatement, _ILabelStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, ILabelStatement2, ILabelStatement
	{
		// Token: 0x0600091E RID: 2334 RVA: 0x0001563D File Offset: 0x0001463D
		public LabelStatement()
		{
		}

		// Token: 0x0600091F RID: 2335 RVA: 0x00015650 File Offset: 0x00014650
		public LabelStatement(string stLabel)
		{
			this.m_stLabel = stLabel;
		}

		// Token: 0x06000920 RID: 2336 RVA: 0x0001566A File Offset: 0x0001466A
		public LabelStatement(string stLabel, IToken token) : base(token)
		{
			this.m_stLabel = stLabel;
		}

		// Token: 0x17000214 RID: 532
		// (get) Token: 0x06000921 RID: 2337 RVA: 0x00015685 File Offset: 0x00014685
		// (set) Token: 0x06000922 RID: 2338 RVA: 0x000156AA File Offset: 0x000146AA
		public string Text
		{
			get
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3211)
				{
					return this.m_stLabel.ToUpperInvariant();
				}
				return this.m_stLabel;
			}
			set
			{
				this.m_stLabel = value;
			}
		}

		// Token: 0x17000215 RID: 533
		// (get) Token: 0x06000923 RID: 2339 RVA: 0x000156B3 File Offset: 0x000146B3
		public string OrgText
		{
			get
			{
				return this.m_stLabel;
			}
		}

		// Token: 0x06000924 RID: 2340 RVA: 0x000156BB File Offset: 0x000146BB
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000925 RID: 2341 RVA: 0x000156C4 File Offset: 0x000146C4
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000926 RID: 2342 RVA: 0x000156CD File Offset: 0x000146CD
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000927 RID: 2343 RVA: 0x000156D8 File Offset: 0x000146D8
		public override _IExprement Duplicate()
		{
			LabelStatement labelStatement = new LabelStatement();
			this.DuplicateCommon(labelStatement);
			labelStatement.m_stLabel = this.m_stLabel;
			return labelStatement;
		}

		// Token: 0x0400013E RID: 318
		[DefaultSerialization("Label")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stLabel = string.Empty;
	}
}

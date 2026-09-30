using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200004E RID: 78
	[TypeGuid("{aef7a177-3956-4c97-8460-6228ab3f3401}")]
	[StorageVersion("3.3.0.0")]
	public class DeRefAccessExpression : Expression, _IDeRefAccessExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IDeRefAccessExpression, ILengthExprement
	{
		// Token: 0x0600047B RID: 1147 RVA: 0x0000C3D6 File Offset: 0x0000B3D6
		public DeRefAccessExpression()
		{
		}

		// Token: 0x0600047C RID: 1148 RVA: 0x0000E3CF File Offset: 0x0000D3CF
		internal DeRefAccessExpression(_IExpression exp, IToken token) : base(token)
		{
			this.m_exp = exp;
		}

		// Token: 0x0600047D RID: 1149 RVA: 0x0000E3DF File Offset: 0x0000D3DF
		internal DeRefAccessExpression(_IExpression exp)
		{
			this.m_exp = exp;
		}

		// Token: 0x0600047E RID: 1150 RVA: 0x00005E58 File Offset: 0x00004E58
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return true;
		}

		// Token: 0x0600047F RID: 1151 RVA: 0x0000E3EE File Offset: 0x0000D3EE
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000480 RID: 1152 RVA: 0x0000E3F7 File Offset: 0x0000D3F7
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000481 RID: 1153 RVA: 0x0000E400 File Offset: 0x0000D400
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000482 RID: 1154 RVA: 0x0000E40C File Offset: 0x0000D40C
		public override ISourcePosition GetPosition()
		{
			ISourcePosition position = this.m_exp.GetPosition();
			if (position != null)
			{
				(position as SourcePosition).Length = this.m_sLength;
			}
			return position;
		}

		// Token: 0x170000E9 RID: 233
		// (get) Token: 0x06000483 RID: 1155 RVA: 0x0000E43A File Offset: 0x0000D43A
		// (set) Token: 0x06000484 RID: 1156 RVA: 0x0000E447 File Offset: 0x0000D447
		[Obfuscation(Feature = "rename")]
		public override IMinimalPosition PositionIntern
		{
			get
			{
				return this.m_exp.PositionIntern;
			}
			set
			{
				if (this.m_exp != null)
				{
					this.m_exp.PositionIntern = value;
				}
			}
		}

		// Token: 0x06000485 RID: 1157 RVA: 0x0000E45D File Offset: 0x0000D45D
		[Obfuscation(Feature = "rename")]
		public override void SetPositionIntern(IMinimalPosition minpos)
		{
			this.m_exp.PositionIntern = minpos;
		}

		// Token: 0x170000EA RID: 234
		// (get) Token: 0x06000486 RID: 1158 RVA: 0x0000E46B File Offset: 0x0000D46B
		// (set) Token: 0x06000487 RID: 1159 RVA: 0x0000E473 File Offset: 0x0000D473
		[Obfuscation(Feature = "rename")]
		public override short LengthIntern
		{
			get
			{
				return this.m_sLength;
			}
			set
			{
				this.m_sLength = value;
			}
		}

		// Token: 0x170000EB RID: 235
		// (get) Token: 0x06000488 RID: 1160 RVA: 0x0000E47C File Offset: 0x0000D47C
		public IExpression Base
		{
			get
			{
				return this._Base;
			}
		}

		// Token: 0x170000EC RID: 236
		// (get) Token: 0x06000489 RID: 1161 RVA: 0x0000E484 File Offset: 0x0000D484
		// (set) Token: 0x0600048A RID: 1162 RVA: 0x0000E49A File Offset: 0x0000D49A
		public _IExpression _Base
		{
			get
			{
				if (this.m_exp == null)
				{
					return new NullExpression();
				}
				return this.m_exp;
			}
			set
			{
				this.m_exp = value;
			}
		}

		// Token: 0x0600048B RID: 1163 RVA: 0x0000E4A3 File Offset: 0x0000D4A3
		public override IVariable GetVariable(IScope scope)
		{
			return this._Base.GetVariable(scope);
		}

		// Token: 0x0600048C RID: 1164 RVA: 0x0000E4B1 File Offset: 0x0000D4B1
		public override IVariable GetVariable(IPrecompileScope scope)
		{
			return this._Base.GetVariable(scope);
		}

		// Token: 0x0600048D RID: 1165 RVA: 0x0000E4BF File Offset: 0x0000D4BF
		public override ISignature GetSignature(IScope scope)
		{
			return this._Base.GetSignature(scope);
		}

		// Token: 0x0600048E RID: 1166 RVA: 0x0000E4D0 File Offset: 0x0000D4D0
		public override _IExprement Duplicate()
		{
			DeRefAccessExpression deRefAccessExpression = new DeRefAccessExpression();
			if (this.m_exp != null)
			{
				deRefAccessExpression.m_exp = (this.m_exp.Duplicate() as Expression);
			}
			this.DuplicateCommon(deRefAccessExpression);
			return deRefAccessExpression;
		}

		// Token: 0x170000ED RID: 237
		// (get) Token: 0x0600048F RID: 1167 RVA: 0x0000E509 File Offset: 0x0000D509
		public override IExprInfo Info
		{
			get
			{
				return this.DeRefInfo;
			}
		}

		// Token: 0x170000EE RID: 238
		// (get) Token: 0x06000490 RID: 1168 RVA: 0x0000E511 File Offset: 0x0000D511
		// (set) Token: 0x06000491 RID: 1169 RVA: 0x0000E519 File Offset: 0x0000D519
		public IDeRefExprInfo DeRefInfo
		{
			get
			{
				return this.m_expInfo;
			}
			set
			{
				this.m_expInfo = value;
			}
		}

		// Token: 0x170000EF RID: 239
		// (get) Token: 0x06000492 RID: 1170 RVA: 0x0000E522 File Offset: 0x0000D522
		// (set) Token: 0x06000493 RID: 1171 RVA: 0x0000E52A File Offset: 0x0000D52A
		[DefaultSerialization("Type")]
		[StorageVersion("3.3.0.10")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		public override ICompiledType _CompiledType
		{
			get
			{
				return this.m_ctype;
			}
			set
			{
				this.m_ctype = value;
			}
		}

		// Token: 0x040000AE RID: 174
		[Obfuscation(Feature = "rename")]
		protected short m_sLength;

		// Token: 0x040000AF RID: 175
		[DefaultSerialization("compiledtype")]
		[StorageVersion("3.3.0.0")]
		protected ICompiledType m_ctype;

		// Token: 0x040000B0 RID: 176
		[Obfuscation(Feature = "rename")]
		private IDeRefExprInfo m_expInfo;

		// Token: 0x040000B1 RID: 177
		[DefaultSerialization("Expression")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		protected _IExpression m_exp;
	}
}

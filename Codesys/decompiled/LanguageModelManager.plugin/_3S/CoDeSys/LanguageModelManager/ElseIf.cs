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
	// Token: 0x02000088 RID: 136
	[TypeGuid("{a3e3d3f5-e25d-4c94-b086-e8ff7573110b}")]
	[StorageVersion("3.3.0.0")]
	public class ElseIf : Exprement, _IElseIf, _IExprement, IExprement3, IExprement2, IExprement, IElseIf2, IElseIf
	{
		// Token: 0x06000887 RID: 2183 RVA: 0x00014AE7 File Offset: 0x00013AE7
		public ElseIf(_IExpression expCondition, _IStatement stControlled)
		{
			this.m_expCondition = expCondition;
			this.m_stControlled = stControlled;
		}

		// Token: 0x06000888 RID: 2184 RVA: 0x0000E573 File Offset: 0x0000D573
		public ElseIf()
		{
		}

		// Token: 0x170001E7 RID: 487
		// (get) Token: 0x06000889 RID: 2185 RVA: 0x00014AFD File Offset: 0x00013AFD
		// (set) Token: 0x0600088A RID: 2186 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override IMinimalPosition _Position
		{
			get
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35800)
				{
					return new MinimalPositionBase(this.m_expCondition._Position.EditorPosition, 0);
				}
				return this.m_expCondition._Position;
			}
			set
			{
			}
		}

		// Token: 0x0600088B RID: 2187 RVA: 0x00014B37 File Offset: 0x00013B37
		public override IBreakpoint CreateBreakpoint(int nOffset, byte bySize)
		{
			return new Breakpoint(nOffset, this.m_expCondition._Position, this.m_expCondition.PositionLength);
		}

		// Token: 0x0600088C RID: 2188 RVA: 0x00014B55 File Offset: 0x00013B55
		public override ISourcePosition GetPosition()
		{
			return this.m_expCondition.GetPosition();
		}

		// Token: 0x170001E8 RID: 488
		// (get) Token: 0x0600088D RID: 2189 RVA: 0x00014B62 File Offset: 0x00013B62
		public IExpression Condition
		{
			get
			{
				return this._Condition;
			}
		}

		// Token: 0x170001E9 RID: 489
		// (get) Token: 0x0600088E RID: 2190 RVA: 0x00014B6A File Offset: 0x00013B6A
		// (set) Token: 0x0600088F RID: 2191 RVA: 0x00014B80 File Offset: 0x00013B80
		public _IExpression _Condition
		{
			get
			{
				if (this.m_expCondition == null)
				{
					return new NullExpression();
				}
				return this.m_expCondition;
			}
			set
			{
				this.m_expCondition = value;
			}
		}

		// Token: 0x170001EA RID: 490
		// (get) Token: 0x06000890 RID: 2192 RVA: 0x00014B89 File Offset: 0x00013B89
		public IStatement Controlled
		{
			get
			{
				return this._Controlled;
			}
		}

		// Token: 0x170001EB RID: 491
		// (get) Token: 0x06000891 RID: 2193 RVA: 0x00014B91 File Offset: 0x00013B91
		// (set) Token: 0x06000892 RID: 2194 RVA: 0x00014BA7 File Offset: 0x00013BA7
		public _IStatement _Controlled
		{
			get
			{
				if (this.m_stControlled == null)
				{
					return new NullStatement();
				}
				return this.m_stControlled;
			}
			set
			{
				this.m_stControlled = value;
			}
		}

		// Token: 0x06000893 RID: 2195 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void Accept(IExprementVisitor visitor)
		{
		}

		// Token: 0x06000894 RID: 2196 RVA: 0x00014BB0 File Offset: 0x00013BB0
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000895 RID: 2197 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x06000896 RID: 2198 RVA: 0x00014BBC File Offset: 0x00013BBC
		public override _IExprement Duplicate()
		{
			ElseIf elseIf = new ElseIf();
			if (this.m_expCondition != null)
			{
				elseIf.m_expCondition = (this.m_expCondition.Duplicate() as _IExpression);
			}
			if (this.m_stControlled != null)
			{
				elseIf.m_stControlled = (this.m_stControlled.Duplicate() as _IStatement);
			}
			return elseIf;
		}

		// Token: 0x170001EC RID: 492
		// (get) Token: 0x06000897 RID: 2199 RVA: 0x00014C0C File Offset: 0x00013C0C
		// (set) Token: 0x06000898 RID: 2200 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[Obfuscation(Feature = "rename")]
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override IMinimalPosition PositionIntern
		{
			get
			{
				return (this.m_expCondition as Expression).PositionIntern;
			}
			set
			{
			}
		}

		// Token: 0x06000899 RID: 2201 RVA: 0x00014C1E File Offset: 0x00013C1E
		[Obfuscation(Feature = "rename")]
		public override void SetPositionIntern(IMinimalPosition minpos)
		{
			(this.m_expCondition as Expression).PositionIntern = minpos;
		}

		// Token: 0x170001ED RID: 493
		// (get) Token: 0x0600089A RID: 2202 RVA: 0x00014C31 File Offset: 0x00013C31
		// (set) Token: 0x0600089B RID: 2203 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[Obfuscation(Feature = "rename")]
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override short LengthIntern
		{
			get
			{
				if (this.m_expCondition == null)
				{
					return 0;
				}
				return (this.m_expCondition as Expression).LengthIntern;
			}
			set
			{
			}
		}

		// Token: 0x04000126 RID: 294
		[DefaultSerialization("Condition")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expCondition;

		// Token: 0x04000127 RID: 295
		[DefaultSerialization("Controlled")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IStatement m_stControlled;
	}
}

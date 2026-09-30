using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000043 RID: 67
	[TypeGuid("{927de619-fa4d-4984-a44d-64277741232d}")]
	[StorageVersion("3.3.0.0")]
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "Class cannot be divided into subclasses because of released interfaces")]
	public class CallExpression : Expression, _ICallExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ICallExpression4, ICallExpression3, ICallExpression2, ICallExpression, ILengthExprement
	{
		// Token: 0x0600035E RID: 862 RVA: 0x0000BB18 File Offset: 0x0000AB18
		public CallExpression()
		{
		}

		// Token: 0x0600035F RID: 863 RVA: 0x0000BB27 File Offset: 0x0000AB27
		internal CallExpression(_IExpression expCallee)
		{
			this.m_expCallee = expCallee;
		}

		// Token: 0x06000360 RID: 864 RVA: 0x0000BB3D File Offset: 0x0000AB3D
		internal CallExpression(_IExpression expCallee, IToken token) : base(token)
		{
			this.m_expCallee = expCallee;
		}

		// Token: 0x06000361 RID: 865 RVA: 0x0000BB54 File Offset: 0x0000AB54
		public IBreakpoint SetCallBreakpoint(int nOffset, byte bySize)
		{
			this.m_BreakpointCall = new Breakpoint(nOffset, this._Position, this.m_sLength);
			return this.m_BreakpointCall;
		}

		// Token: 0x17000099 RID: 153
		// (get) Token: 0x06000362 RID: 866 RVA: 0x0000BB74 File Offset: 0x0000AB74
		public IBreakpoint CallBreakpoint
		{
			get
			{
				return this.m_BreakpointCall;
			}
		}

		// Token: 0x06000363 RID: 867 RVA: 0x0000BB7C File Offset: 0x0000AB7C
		public IBreakpoint SetBeforeCallBreakpoint(int nOffset, byte bySize)
		{
			this.m_BreakpointBeforeCall = new Breakpoint(nOffset, this._Position, this.m_sLength);
			return this.m_BreakpointBeforeCall;
		}

		// Token: 0x1700009A RID: 154
		// (get) Token: 0x06000364 RID: 868 RVA: 0x0000BB9C File Offset: 0x0000AB9C
		public IBreakpoint BeforeCallBreakpoint
		{
			get
			{
				return this.m_BreakpointBeforeCall;
			}
		}

		// Token: 0x1700009B RID: 155
		// (get) Token: 0x06000365 RID: 869 RVA: 0x0000BBA4 File Offset: 0x0000ABA4
		public KindOfCall KindOfCall
		{
			get
			{
				if (this.CallInfo != null)
				{
					return this.CallInfo.KindOfCall;
				}
				return KindOfCall.None;
			}
		}

		// Token: 0x06000366 RID: 870 RVA: 0x0000BBBB File Offset: 0x0000ABBB
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351000 && base.Type != null && base.Type.Class == TypeClass.Reference;
		}

		// Token: 0x1700009C RID: 156
		// (get) Token: 0x06000367 RID: 871 RVA: 0x0000BBE8 File Offset: 0x0000ABE8
		public IExpression Callee
		{
			get
			{
				return this._Callee;
			}
		}

		// Token: 0x1700009D RID: 157
		// (get) Token: 0x06000368 RID: 872 RVA: 0x0000BBF0 File Offset: 0x0000ABF0
		// (set) Token: 0x06000369 RID: 873 RVA: 0x0000BC06 File Offset: 0x0000AC06
		public _IExpression _Callee
		{
			get
			{
				if (this.m_expCallee == null)
				{
					return new NullExpression();
				}
				return this.m_expCallee;
			}
			set
			{
				this.m_expCallee = value;
			}
		}

		// Token: 0x1700009E RID: 158
		// (get) Token: 0x0600036A RID: 874 RVA: 0x0000BC0F File Offset: 0x0000AC0F
		public IList<_IExpression> Inputs
		{
			get
			{
				if (this.m_alexpFormalParams == null)
				{
					return Array.Empty<Expression>();
				}
				return this.m_alexpFormalParams;
			}
		}

		// Token: 0x1700009F RID: 159
		// (get) Token: 0x0600036B RID: 875 RVA: 0x0000BC28 File Offset: 0x0000AC28
		public IAssignmentExpression[] InputAssigns
		{
			get
			{
				return Enumerable.ToLList<_IAssignmentExpression>(this._InputAssigns).ToArray();
			}
		}

		// Token: 0x170000A0 RID: 160
		// (get) Token: 0x0600036C RID: 876 RVA: 0x0000BC48 File Offset: 0x0000AC48
		public IAssignmentExpression[] OutputAssigns
		{
			get
			{
				return Enumerable.ToLList<_IAssignmentExpression>(this._OutputAssigns).ToArray();
			}
		}

		// Token: 0x170000A1 RID: 161
		// (get) Token: 0x0600036D RID: 877 RVA: 0x0000BC67 File Offset: 0x0000AC67
		public IList<_IExpression> EmptyAssigns
		{
			get
			{
				if (this.m_alEmptyAssigns == null)
				{
					return Array.Empty<_IExpression>();
				}
				return this.m_alEmptyAssigns;
			}
		}

		// Token: 0x170000A2 RID: 162
		// (get) Token: 0x0600036E RID: 878 RVA: 0x0000BC80 File Offset: 0x0000AC80
		public IList<_IAssignmentExpression> _InputAssigns
		{
			get
			{
				if (this.m_alexpActualParams == null)
				{
					return Array.Empty<_IAssignmentExpression>();
				}
				_IAssignmentExpression[] array = new AssignmentExpression[this.m_alexpActualParams.Count];
				_IAssignmentExpression[] array2 = array;
				for (int i = 0; i < this.m_alexpActualParams.Count; i++)
				{
					array2[i] = new AssignmentExpression(this.m_alexpFormalParams[i]);
					array2[i]._RValue = this.m_alexpActualParams[i];
				}
				return array2;
			}
		}

		// Token: 0x0600036F RID: 879 RVA: 0x0000BCED File Offset: 0x0000ACED
		public void RemoveInputAt(int i)
		{
			Debug.Assert(i >= 0 && i < this.m_alexpActualParams.Count);
			this.m_alexpActualParams.RemoveAt(i);
			this.m_alexpFormalParams.RemoveAt(i);
		}

		// Token: 0x170000A3 RID: 163
		// (get) Token: 0x06000370 RID: 880 RVA: 0x0000BD24 File Offset: 0x0000AD24
		public IList<_IAssignmentExpression> _OutputAssigns
		{
			get
			{
				if (this.m_alexpActualOutputs == null)
				{
					return Array.Empty<_IAssignmentExpression>();
				}
				_IAssignmentExpression[] array = new AssignmentExpression[this.m_alexpActualOutputs.Count];
				_IAssignmentExpression[] array2 = array;
				for (int i = 0; i < this.m_alexpActualOutputs.Count; i++)
				{
					array2[i] = new AssignmentExpression(this.m_alexpActualOutputs[i]);
					array2[i]._RValue = this.m_alexpFormalOutputs[i];
					if ((array2[i]._RValue.Type != null && !TypeTable.IsBlock(array2[i]._RValue.Type.Class)) || !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33200)
					{
						IExpression rvalue = array2[i]._RValue;
						CompilerProxy.MakeImplicitConversionIfNecessary(array2[i]._RValue.Type, array2[i]._LValue.Type, ref rvalue);
						array2[i]._RValue = (rvalue as Expression);
					}
					array2[i].KindOf = Operator.AssignOut;
				}
				return array2;
			}
		}

		// Token: 0x170000A4 RID: 164
		// (get) Token: 0x06000371 RID: 881 RVA: 0x0000BE16 File Offset: 0x0000AE16
		public IList<_IExpression> ParamExpressions
		{
			get
			{
				if (this.m_alexpActualParams == null)
				{
					return Array.Empty<Expression>();
				}
				return this.m_alexpActualParams;
			}
		}

		// Token: 0x170000A5 RID: 165
		public _IExpression this[int i]
		{
			get
			{
				if (this.m_alexpActualParams == null || i < 0 || i >= this.m_alexpActualParams.Count)
				{
					return null;
				}
				return this.m_alexpActualParams[i];
			}
			set
			{
				if (this.m_alexpActualParams == null || i < 0 || i >= this.m_alexpActualParams.Count)
				{
					return;
				}
				this.m_alexpActualParams[i] = value;
			}
		}

		// Token: 0x170000A6 RID: 166
		// (get) Token: 0x06000374 RID: 884 RVA: 0x0000BE80 File Offset: 0x0000AE80
		public IList<_IExpression> Outputs
		{
			get
			{
				if (this.m_alexpFormalOutputs == null)
				{
					return Array.Empty<_IExpression>();
				}
				return this.m_alexpFormalOutputs;
			}
		}

		// Token: 0x170000A7 RID: 167
		// (get) Token: 0x06000375 RID: 885 RVA: 0x0000BE96 File Offset: 0x0000AE96
		public IList<_IExpression> OutputExpressions
		{
			get
			{
				if (this.m_alexpActualOutputs == null)
				{
					return Array.Empty<Expression>();
				}
				return this.m_alexpActualOutputs;
			}
		}

		// Token: 0x170000A8 RID: 168
		// (get) Token: 0x06000376 RID: 886 RVA: 0x0000BEAC File Offset: 0x0000AEAC
		// (set) Token: 0x06000377 RID: 887 RVA: 0x0000BEB4 File Offset: 0x0000AEB4
		public ICallExprInfo CallInfo
		{
			get
			{
				return this.m_CallInfo;
			}
			set
			{
				this.m_CallInfo = value;
			}
		}

		// Token: 0x170000A9 RID: 169
		// (get) Token: 0x06000378 RID: 888 RVA: 0x0000BEBD File Offset: 0x0000AEBD
		// (set) Token: 0x06000379 RID: 889 RVA: 0x0000BEDD File Offset: 0x0000AEDD
		public override IExprInfo Info
		{
			get
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34000)
				{
					return this.m_CallInfo;
				}
				return base.Info;
			}
			set
			{
				base.Info = value;
			}
		}

		// Token: 0x0600037A RID: 890 RVA: 0x0000BEE6 File Offset: 0x0000AEE6
		public void InsertParam(int index, _IExpression exp, _IExpression expVariable)
		{
			if (this.m_alexpActualParams == null)
			{
				this.m_alexpActualParams = new LList<_IExpression>(1);
				this.m_alexpFormalParams = new LList<_IExpression>(1);
			}
			this.m_alexpActualParams.Insert(index, exp);
			this.m_alexpFormalParams.Insert(index, expVariable);
		}

		// Token: 0x0600037B RID: 891 RVA: 0x0000BF22 File Offset: 0x0000AF22
		public void AddParam(_IExpression exp, _IExpression expVariable)
		{
			if (this.m_alexpActualParams == null)
			{
				this.m_alexpActualParams = new LList<_IExpression>(1);
				this.m_alexpFormalParams = new LList<_IExpression>(1);
			}
			this.m_alexpActualParams.Add(exp);
			this.m_alexpFormalParams.Add(expVariable);
		}

		// Token: 0x0600037C RID: 892 RVA: 0x0000BF5C File Offset: 0x0000AF5C
		public void AddParam(_IExpression exp)
		{
			this.AddParam(exp, null);
		}

		// Token: 0x0600037D RID: 893 RVA: 0x0000BF66 File Offset: 0x0000AF66
		public void SetFormalParam(_IExpression expInput, int iIndex)
		{
			this.m_alexpFormalParams[iIndex] = expInput;
		}

		// Token: 0x0600037E RID: 894 RVA: 0x0000BF75 File Offset: 0x0000AF75
		public void SetActualParam(_IExpression expInput, int iIndex)
		{
			this.m_alexpActualParams[iIndex] = expInput;
		}

		// Token: 0x0600037F RID: 895 RVA: 0x0000BF84 File Offset: 0x0000AF84
		public void SetActualOutput(_IExpression exp, int iIndex)
		{
			this.m_alexpActualOutputs[iIndex] = exp;
		}

		// Token: 0x06000380 RID: 896 RVA: 0x0000BF93 File Offset: 0x0000AF93
		public void SetFormalOutput(_IExpression exp, int iIndex)
		{
			this.m_alexpFormalOutputs[iIndex] = exp;
		}

		// Token: 0x06000381 RID: 897 RVA: 0x0000BFA2 File Offset: 0x0000AFA2
		public void AddOutput(_IExpression exp, _IExpression expVariable)
		{
			if (this.m_alexpActualOutputs == null)
			{
				this.m_alexpActualOutputs = new LList<_IExpression>(1);
				this.m_alexpFormalOutputs = new LList<_IExpression>(1);
			}
			this.m_alexpActualOutputs.Add(expVariable);
			this.m_alexpFormalOutputs.Add(exp);
		}

		// Token: 0x06000382 RID: 898 RVA: 0x0000BFDC File Offset: 0x0000AFDC
		public void AddEmptyAssign(_IExpression exp)
		{
			if (this.m_alEmptyAssigns == null)
			{
				this.m_alEmptyAssigns = new LList<_IExpression>();
			}
			this.m_alEmptyAssigns.Add(exp);
		}

		// Token: 0x06000383 RID: 899 RVA: 0x0000BFFD File Offset: 0x0000AFFD
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000384 RID: 900 RVA: 0x0000C006 File Offset: 0x0000B006
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000385 RID: 901 RVA: 0x0000C00F File Offset: 0x0000B00F
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000386 RID: 902 RVA: 0x0000C018 File Offset: 0x0000B018
		public override ISourcePosition GetPosition()
		{
			SourcePosition sourcePosition = this._Callee.GetPosition() as SourcePosition;
			if (sourcePosition != null)
			{
				sourcePosition.Length = this.m_sLength;
			}
			return sourcePosition;
		}

		// Token: 0x170000AA RID: 170
		// (get) Token: 0x06000387 RID: 903 RVA: 0x0000C046 File Offset: 0x0000B046
		public IExpression Condition
		{
			get
			{
				return this._Condition;
			}
		}

		// Token: 0x170000AB RID: 171
		// (get) Token: 0x06000388 RID: 904 RVA: 0x0000C04E File Offset: 0x0000B04E
		// (set) Token: 0x06000389 RID: 905 RVA: 0x0000C056 File Offset: 0x0000B056
		public _IExpression _Condition
		{
			get
			{
				return this.m_expCondition;
			}
			set
			{
				this.m_expCondition = value;
			}
		}

		// Token: 0x0600038A RID: 906 RVA: 0x0000C060 File Offset: 0x0000B060
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-86324")]
		public override _IExprement Duplicate()
		{
			CallExpression callExpression = new CallExpression();
			if (this.m_expCallee != null)
			{
				callExpression.m_expCallee = (this.m_expCallee.Duplicate() as _IExpression);
			}
			if (this._Condition != null)
			{
				callExpression._Condition = (this._Condition.Duplicate() as _IExpression);
			}
			if (this.m_typeExpected != null)
			{
				callExpression.m_typeExpected = this.m_typeExpected.Duplicate;
			}
			this.DuplicateCommon(callExpression);
			if (this.m_alexpActualParams != null)
			{
				callExpression.m_alexpActualParams = new LList<_IExpression>(this.m_alexpActualParams.Count);
				foreach (_IExpression iexpression in this.m_alexpActualParams)
				{
					callExpression.m_alexpActualParams.Add(iexpression.Duplicate() as _IExpression);
				}
			}
			if (this.m_alexpFormalParams != null)
			{
				callExpression.m_alexpFormalParams = new LList<_IExpression>(this.m_alexpFormalParams.Count);
				foreach (_IExpression iexpression2 in this.m_alexpFormalParams)
				{
					if (iexpression2 != null)
					{
						callExpression.m_alexpFormalParams.Add(iexpression2.Duplicate() as _IExpression);
					}
					else
					{
						callExpression.m_alexpFormalParams.Add(null);
					}
				}
			}
			if (this.m_alexpActualOutputs != null)
			{
				callExpression.m_alexpActualOutputs = new LList<_IExpression>();
				foreach (_IExpression iexpression3 in this.m_alexpActualOutputs)
				{
					callExpression.m_alexpActualOutputs.Add(iexpression3.Duplicate() as _IExpression);
				}
				callExpression.m_alexpActualOutputs.TrimExcess();
			}
			if (this.m_alexpFormalOutputs != null)
			{
				callExpression.m_alexpFormalOutputs = new LList<_IExpression>();
				foreach (_IExpression iexpression4 in this.m_alexpFormalOutputs)
				{
					callExpression.m_alexpFormalOutputs.Add(iexpression4.Duplicate() as _IExpression);
				}
				callExpression.m_alexpFormalOutputs.TrimExcess();
			}
			if (this.m_alEmptyAssigns != null)
			{
				callExpression.m_alEmptyAssigns = new LList<_IExpression>();
				foreach (_IExpression iexpression5 in this.m_alEmptyAssigns)
				{
					callExpression.m_alEmptyAssigns.Add(iexpression5.Duplicate() as _IExpression);
				}
				callExpression.m_alEmptyAssigns.TrimExcess();
			}
			callExpression.ScratchOffset = this.ScratchOffset;
			return callExpression;
		}

		// Token: 0x170000AC RID: 172
		// (get) Token: 0x0600038B RID: 907 RVA: 0x0000C304 File Offset: 0x0000B304
		// (set) Token: 0x0600038C RID: 908 RVA: 0x0000C30C File Offset: 0x0000B30C
		public override int ScratchOffset
		{
			get
			{
				return this.m_nScratchOffset;
			}
			set
			{
				this.m_nScratchOffset = value;
			}
		}

		// Token: 0x170000AD RID: 173
		// (get) Token: 0x0600038D RID: 909 RVA: 0x0000C315 File Offset: 0x0000B315
		// (set) Token: 0x0600038E RID: 910 RVA: 0x0000C31D File Offset: 0x0000B31D
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

		// Token: 0x170000AE RID: 174
		// (get) Token: 0x0600038F RID: 911 RVA: 0x0000C326 File Offset: 0x0000B326
		// (set) Token: 0x06000390 RID: 912 RVA: 0x0000C32E File Offset: 0x0000B32E
		public _IType ExpectedType
		{
			get
			{
				return this.m_typeExpected;
			}
			set
			{
				this.m_typeExpected = value;
			}
		}

		// Token: 0x06000391 RID: 913 RVA: 0x0000C338 File Offset: 0x0000B338
		public override ISignature GetSignature(IScope scope)
		{
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34100)
			{
				return base.GetSignature(scope);
			}
			if (base.Type.DeRefType.Class != TypeClass.Userdef)
			{
				return base.GetSignature(scope);
			}
			return (base.Type.DeRefType as UserdefType).GetSignature(scope);
		}

		// Token: 0x170000AF RID: 175
		// (get) Token: 0x06000392 RID: 914 RVA: 0x0000C390 File Offset: 0x0000B390
		// (set) Token: 0x06000393 RID: 915 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[Obfuscation(Feature = "rename")]
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override IMinimalPosition PositionIntern
		{
			get
			{
				return this._Callee.PositionIntern;
			}
			set
			{
			}
		}

		// Token: 0x06000394 RID: 916 RVA: 0x0000C39D File Offset: 0x0000B39D
		[Obfuscation(Feature = "rename")]
		public override void SetPositionIntern(IMinimalPosition minpos)
		{
			this._Callee.PositionIntern = minpos;
		}

		// Token: 0x170000B0 RID: 176
		// (get) Token: 0x06000395 RID: 917 RVA: 0x0000C3AB File Offset: 0x0000B3AB
		// (set) Token: 0x06000396 RID: 918 RVA: 0x0000C3B3 File Offset: 0x0000B3B3
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

		// Token: 0x170000B1 RID: 177
		// (get) Token: 0x06000397 RID: 919 RVA: 0x0000C3BC File Offset: 0x0000B3BC
		// (set) Token: 0x06000398 RID: 920 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override int PrecompileVariableId
		{
			get
			{
				return this._Callee.PrecompileVariableId;
			}
			set
			{
			}
		}

		// Token: 0x170000B2 RID: 178
		// (get) Token: 0x06000399 RID: 921 RVA: 0x0000C3C9 File Offset: 0x0000B3C9
		// (set) Token: 0x0600039A RID: 922 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override int PrecompileSignatureId
		{
			get
			{
				return this._Callee.PrecompileSignatureId;
			}
			set
			{
			}
		}

		// Token: 0x04000081 RID: 129
		[Obfuscation(Feature = "rename")]
		protected short m_sLength;

		// Token: 0x04000082 RID: 130
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("compiledtype")]
		[StorageVersion("3.3.0.0")]
		private ICompiledType m_ctype;

		// Token: 0x04000083 RID: 131
		[DefaultSerialization("Callee")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expCallee;

		// Token: 0x04000084 RID: 132
		[DefaultSerialization("ActualParams")]
		[StorageVersion("3.3.0.0")]
		[StorageSaveAsNonGenericCollection("3.3.0.0-3.5.6.255")]
		[Obfuscation(Feature = "rename")]
		private LList<_IExpression> m_alexpActualParams;

		// Token: 0x04000085 RID: 133
		[DefaultSerialization("FormalParams")]
		[StorageVersion("3.3.0.0")]
		[StorageSaveAsNonGenericCollection("3.3.0.0-3.5.6.255")]
		[Obfuscation(Feature = "rename")]
		private LList<_IExpression> m_alexpFormalParams;

		// Token: 0x04000086 RID: 134
		[DefaultSerialization("Outputs")]
		[StorageVersion("3.3.0.0")]
		[StorageSaveAsNonGenericCollection("3.3.0.0-3.5.6.255")]
		[Obfuscation(Feature = "rename")]
		private LList<_IExpression> m_alexpActualOutputs;

		// Token: 0x04000087 RID: 135
		[DefaultSerialization("FormalOutputs")]
		[StorageVersion("3.3.0.0")]
		[StorageSaveAsNonGenericCollection("3.3.0.0-3.5.6.255")]
		[Obfuscation(Feature = "rename")]
		private LList<_IExpression> m_alexpFormalOutputs;

		// Token: 0x04000088 RID: 136
		[DefaultSerialization("Condition")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expCondition;

		// Token: 0x04000089 RID: 137
		[Obfuscation(Feature = "rename")]
		private ICallExprInfo m_CallInfo;

		// Token: 0x0400008A RID: 138
		[DefaultSerialization("ScratchOffset")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_nScratchOffset = -1;

		// Token: 0x0400008B RID: 139
		[Obfuscation(Feature = "rename")]
		private Breakpoint m_BreakpointCall;

		// Token: 0x0400008C RID: 140
		[Obfuscation(Feature = "rename")]
		private Breakpoint m_BreakpointBeforeCall;

		// Token: 0x0400008D RID: 141
		[Obfuscation(Feature = "rename")]
		private _IType m_typeExpected;

		// Token: 0x0400008E RID: 142
		[DefaultSerialization("EmptyAssignments")]
		[StorageVersion("3.5.7.0")]
		[StorageDefaultValue(null)]
		[Obfuscation(Feature = "rename")]
		private LList<_IExpression> m_alEmptyAssigns;
	}
}

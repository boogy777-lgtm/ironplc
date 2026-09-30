using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001DB RID: 475
	internal abstract class Expression_Green : Exprement_Green, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression
	{
		// Token: 0x06002180 RID: 8576 RVA: 0x0005A5AC File Offset: 0x000595AC
		public bool IsVarInOutInput(IScope scope, bool bWriteToConstants, bool bVarInoutConstant)
		{
			throw new NotSupportedException("no context information in green tree");
		}

		// Token: 0x06002181 RID: 8577 RVA: 0x0005A5AC File Offset: 0x000595AC
		public bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			throw new NotSupportedException("no context information in green tree");
		}

		// Token: 0x06002182 RID: 8578 RVA: 0x0005A5AC File Offset: 0x000595AC
		public bool IsLValue(IScope scope, bool bWriteToConstants, bool bPassToVarInout)
		{
			throw new NotSupportedException("no context information in green tree");
		}

		// Token: 0x06002183 RID: 8579 RVA: 0x0005A5AC File Offset: 0x000595AC
		public bool IsLValue(IScope scope, bool bWriteToConstants, bool bPassToVarInout, bool bRefAssign)
		{
			throw new NotSupportedException("no context information in green tree");
		}

		// Token: 0x17000933 RID: 2355
		// (get) Token: 0x06002184 RID: 8580 RVA: 0x0005A5AC File Offset: 0x000595AC
		public IIdentifierInfo2 IdentifierInfo
		{
			get
			{
				throw new NotSupportedException("no context information in green tree");
			}
		}

		// Token: 0x17000934 RID: 2356
		// (get) Token: 0x06002185 RID: 8581 RVA: 0x0000E5F6 File Offset: 0x0000D5F6
		// (set) Token: 0x06002186 RID: 8582 RVA: 0x0005A448 File Offset: 0x00059448
		public int PrecompileVariableId
		{
			get
			{
				return Common.InvalidID;
			}
			set
			{
				throw new NotSupportedException("Do not attempt to manipulate green trees");
			}
		}

		// Token: 0x17000935 RID: 2357
		// (get) Token: 0x06002187 RID: 8583 RVA: 0x0000E5F6 File Offset: 0x0000D5F6
		// (set) Token: 0x06002188 RID: 8584 RVA: 0x0005A448 File Offset: 0x00059448
		public int PrecompileSignatureId
		{
			get
			{
				return Common.InvalidID;
			}
			set
			{
				throw new NotSupportedException("Do not attempt to manipulate green trees");
			}
		}

		// Token: 0x06002189 RID: 8585 RVA: 0x00005F0F File Offset: 0x00004F0F
		public IVariable GetVariable(IScope scope)
		{
			return null;
		}

		// Token: 0x0600218A RID: 8586 RVA: 0x00005F0F File Offset: 0x00004F0F
		public IVariable GetVariable(IPrecompileScope scope)
		{
			return null;
		}

		// Token: 0x17000936 RID: 2358
		// (get) Token: 0x0600218B RID: 8587 RVA: 0x000042F0 File Offset: 0x000032F0
		public static int InvalidId
		{
			get
			{
				return -1;
			}
		}

		// Token: 0x17000937 RID: 2359
		// (get) Token: 0x0600218C RID: 8588 RVA: 0x0005A5B8 File Offset: 0x000595B8
		// (set) Token: 0x0600218D RID: 8589 RVA: 0x0005A448 File Offset: 0x00059448
		public int VariableId
		{
			get
			{
				return Expression_Green.InvalidId;
			}
			set
			{
				throw new NotSupportedException("Do not attempt to manipulate green trees");
			}
		}

		// Token: 0x0600218E RID: 8590 RVA: 0x0005A5BF File Offset: 0x000595BF
		public bool IsEqual(IExpression expression)
		{
			return base.IsEqual(expression as _IExprement);
		}

		// Token: 0x17000938 RID: 2360
		// (get) Token: 0x0600218F RID: 8591 RVA: 0x00004E6B File Offset: 0x00003E6B
		public bool IsPOUReference
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06002190 RID: 8592 RVA: 0x00005F0F File Offset: 0x00004F0F
		public ISignature GetSignature(IScope scope)
		{
			return null;
		}

		// Token: 0x06002191 RID: 8593 RVA: 0x00005F0F File Offset: 0x00004F0F
		public ISignature GetSignatureEx(IScope scope)
		{
			return null;
		}

		// Token: 0x17000939 RID: 2361
		// (get) Token: 0x06002192 RID: 8594 RVA: 0x0005A5B8 File Offset: 0x000595B8
		// (set) Token: 0x06002193 RID: 8595 RVA: 0x0005A448 File Offset: 0x00059448
		public virtual int SignatureId
		{
			get
			{
				return Expression_Green.InvalidId;
			}
			set
			{
				throw new NotSupportedException("Do not attempt to manipulate green trees");
			}
		}

		// Token: 0x1700093A RID: 2362
		// (get) Token: 0x06002194 RID: 8596 RVA: 0x00004E6B File Offset: 0x00003E6B
		public virtual bool IsLiteral
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06002195 RID: 8597 RVA: 0x0005A5AC File Offset: 0x000595AC
		public ILiteralValue LiteralUnchecked(IScope scope)
		{
			throw new NotSupportedException("no context information in green tree");
		}

		// Token: 0x06002196 RID: 8598 RVA: 0x0005A5AC File Offset: 0x000595AC
		public ILiteralValue LiteralWithRecursionCheck(IScope scope, IDictionary<IVariable, IVariable> variableStack, bool bAllocatedOK, out bool bRecursionError)
		{
			throw new NotSupportedException("no context information in green tree");
		}

		// Token: 0x06002197 RID: 8599 RVA: 0x0005A5AC File Offset: 0x000595AC
		public ILiteralValue LiteralWithRecursionCheck(IPrecompileScope scope, IDictionary<IVariable, IVariable> variableStack, bool bAllocatedOK, out bool bRecursionError)
		{
			throw new NotSupportedException("no context information in green tree");
		}

		// Token: 0x06002198 RID: 8600 RVA: 0x00005F0F File Offset: 0x00004F0F
		public ILiteralValue Literal(IScope scope)
		{
			return null;
		}

		// Token: 0x06002199 RID: 8601 RVA: 0x00005F0F File Offset: 0x00004F0F
		public ILiteralValue Literal(IScope scope, bool bAllocatedOK)
		{
			return null;
		}

		// Token: 0x0600219A RID: 8602 RVA: 0x00005F0F File Offset: 0x00004F0F
		public ILiteralValue Literal(IPrecompileScope scope)
		{
			return null;
		}

		// Token: 0x0600219B RID: 8603 RVA: 0x00005F0F File Offset: 0x00004F0F
		public IDataLocation DataLocation(IScope scope)
		{
			return null;
		}

		// Token: 0x1700093B RID: 2363
		// (get) Token: 0x0600219C RID: 8604 RVA: 0x00004E6B File Offset: 0x00003E6B
		public bool IsCompiled
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700093C RID: 2364
		// (get) Token: 0x0600219D RID: 8605 RVA: 0x00005F0F File Offset: 0x00004F0F
		// (set) Token: 0x0600219E RID: 8606 RVA: 0x0005A448 File Offset: 0x00059448
		public virtual ICompiledType Type
		{
			get
			{
				return null;
			}
			set
			{
				throw new NotSupportedException("Do not attempt to manipulate green trees");
			}
		}

		// Token: 0x0600219F RID: 8607 RVA: 0x00004E6B File Offset: 0x00003E6B
		public bool IsConstant(IScope scope, bool bAllocatedOK)
		{
			return false;
		}

		// Token: 0x1700093D RID: 2365
		// (get) Token: 0x060021A0 RID: 8608 RVA: 0x000042F0 File Offset: 0x000032F0
		// (set) Token: 0x060021A1 RID: 8609 RVA: 0x0005A448 File Offset: 0x00059448
		public int ScratchOffset
		{
			get
			{
				return -1;
			}
			set
			{
				throw new NotSupportedException("Do not attempt to manipulate green trees");
			}
		}

		// Token: 0x1700093E RID: 2366
		// (get) Token: 0x060021A2 RID: 8610 RVA: 0x00004E6B File Offset: 0x00003E6B
		// (set) Token: 0x060021A3 RID: 8611 RVA: 0x0005A448 File Offset: 0x00059448
		[Obsolete("this flag is not supported any more!")]
		public bool IsStatement
		{
			get
			{
				return false;
			}
			set
			{
				throw new NotSupportedException("Do not attempt to manipulate green trees");
			}
		}

		// Token: 0x1700093F RID: 2367
		// (get) Token: 0x060021A4 RID: 8612 RVA: 0x00005F0F File Offset: 0x00004F0F
		// (set) Token: 0x060021A5 RID: 8613 RVA: 0x0005A448 File Offset: 0x00059448
		public virtual ICompiledType _CompiledType
		{
			get
			{
				return null;
			}
			set
			{
				throw new NotSupportedException("Do not attempt to manipulate green trees");
			}
		}
	}
}

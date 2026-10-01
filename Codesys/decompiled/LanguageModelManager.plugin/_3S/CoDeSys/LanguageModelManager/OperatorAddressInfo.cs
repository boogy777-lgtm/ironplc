using System;
using System.Diagnostics;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000F1 RID: 241
	internal class OperatorAddressInfo : AddressInfoBase, IOperatorAddressInfo, IAddressInfo, IMyAddressInfo, IAddressInfo4, IAddressInfo3, IAddressInfo2
	{
		// Token: 0x060011DC RID: 4572 RVA: 0x00032F86 File Offset: 0x00031F86
		public OperatorAddressInfo(IAddressInfo[] operands, Operator op, IType t) : base(t)
		{
			if (operands == null)
			{
				throw new ArgumentNullException("operands");
			}
			this._operands = operands;
			this._operator = op;
		}

		// Token: 0x060011DD RID: 4573 RVA: 0x00032FAC File Offset: 0x00031FAC
		public static OperatorAddressInfo CreateUnsignedToSignedCast(IAddressInfo addrinInfo, TypeClass tc)
		{
			Debug.Assert(TypeTable.IsInteger(tc) && !TypeTable.IsSigned(tc));
			TypeClass correspondingSignedType = TypeTable.GetCorrespondingSignedType(tc);
			return new OperatorAddressInfo(new IAddressInfo[]
			{
				addrinInfo
			}, Operator.__Cast, TypeTable.Get(correspondingSignedType));
		}

		// Token: 0x060011DE RID: 4574 RVA: 0x00032FF3 File Offset: 0x00031FF3
		private OperatorAddressInfo(IType t) : base(t)
		{
		}

		// Token: 0x17000526 RID: 1318
		// (get) Token: 0x060011DF RID: 4575 RVA: 0x00032FFC File Offset: 0x00031FFC
		public IAddressInfo[] Operands
		{
			get
			{
				return this._operands;
			}
		}

		// Token: 0x17000527 RID: 1319
		// (get) Token: 0x060011E0 RID: 4576 RVA: 0x00033004 File Offset: 0x00032004
		public Operator Operator
		{
			get
			{
				return this._operator;
			}
		}

		// Token: 0x17000528 RID: 1320
		// (get) Token: 0x060011E1 RID: 4577 RVA: 0x0003300C File Offset: 0x0003200C
		public override int Size
		{
			get
			{
				ICompiledType compiledType = base.Type as ICompiledType;
				if (compiledType == null)
				{
					return 0;
				}
				return compiledType.Size(null);
			}
		}

		// Token: 0x060011E2 RID: 4578 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void SetSize(int nSize)
		{
		}

		// Token: 0x060011E3 RID: 4579 RVA: 0x00033034 File Offset: 0x00032034
		public IMyAddressInfo Duplicate()
		{
			OperatorAddressInfo operatorAddressInfo = new OperatorAddressInfo(base.Type);
			operatorAddressInfo._operator = this._operator;
			operatorAddressInfo._operands = new IAddressInfo[this._operands.Length];
			for (int i = 0; i < this._operands.Length; i++)
			{
				operatorAddressInfo._operands[i] = ((IMyAddressInfo)this._operands[i]).Duplicate();
			}
			operatorAddressInfo.SignatureID = base.SignatureID;
			operatorAddressInfo.VariableID = base.VariableID;
			return operatorAddressInfo;
		}

		// Token: 0x17000529 RID: 1321
		// (get) Token: 0x060011E4 RID: 4580 RVA: 0x000330B2 File Offset: 0x000320B2
		public override bool ContainsStackRelativeAddress
		{
			get
			{
				return Array.Exists<IAddressInfo>(this._operands, (IAddressInfo op) => op is IAddressInfo4 && (op as IAddressInfo4).ContainsStackRelativeAddress);
			}
		}

		// Token: 0x060011E5 RID: 4581 RVA: 0x000330E0 File Offset: 0x000320E0
		public override int GetHashCode()
		{
			int num = this._operator.GetHashCode();
			foreach (IAddressInfo addressInfo in this._operands)
			{
				num ^= addressInfo.GetHashCode();
			}
			return num;
		}

		// Token: 0x060011E6 RID: 4582 RVA: 0x00033124 File Offset: 0x00032124
		public override bool Equals(object obj)
		{
			OperatorAddressInfo operatorAddressInfo = obj as OperatorAddressInfo;
			if (operatorAddressInfo == null)
			{
				return false;
			}
			if (this._operator != operatorAddressInfo._operator)
			{
				return false;
			}
			if (this._operands.Length != operatorAddressInfo._operands.Length)
			{
				return false;
			}
			for (int i = 0; i < this._operands.Length; i++)
			{
				if (!this._operands[i].Equals(operatorAddressInfo._operands[i]))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0400042E RID: 1070
		private Operator _operator;

		// Token: 0x0400042F RID: 1071
		private IAddressInfo[] _operands;
	}
}

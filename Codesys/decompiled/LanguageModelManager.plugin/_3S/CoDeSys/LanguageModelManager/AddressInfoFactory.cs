using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000F9 RID: 249
	[TypeGuid("{B0253206-A764-43E3-BB40-9E922B4DBCC0}")]
	internal class AddressInfoFactory : IAddressInfoFactory
	{
		// Token: 0x06001236 RID: 4662 RVA: 0x0003379F File Offset: 0x0003279F
		public IOperatorAddressInfo CreateOperator(IAddressInfo[] operands, Operator op, IType t)
		{
			return new OperatorAddressInfo(operands, op, t);
		}

		// Token: 0x06001237 RID: 4663 RVA: 0x000337A9 File Offset: 0x000327A9
		public ILiteralAddressInfo CreateLiteral(ILiteralValue lv, ICompiledType t)
		{
			return new LiteralAddressInfo(lv, t);
		}

		// Token: 0x06001238 RID: 4664 RVA: 0x000337B2 File Offset: 0x000327B2
		public IConversionAddressInfo CreateConversion(TypeClass tcFrom, TypeClass tcTo, IAddressInfo aiBase, bool bImplicit, IType t)
		{
			return new ConversionAddressInfo(tcFrom, tcTo, aiBase, bImplicit, t);
		}
	}
}

using System;
using System.Runtime.CompilerServices;
using \u0014;
using \u0019;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0081
{
	// Token: 0x020002D9 RID: 729
	internal sealed class \u0015
	{
		// Token: 0x17000793 RID: 1939
		// (get) Token: 0x06002BE3 RID: 11235 RVA: 0x00099DF0 File Offset: 0x00097FF0
		private global::\u0014.\u0012 CheckerCheckFunctions { get; }

		// Token: 0x06002BE4 RID: 11236 RVA: 0x00099DF8 File Offset: 0x00097FF8
		public \u0015(global::\u0014.\u0012 \u0097\u0005)
		{
			this.CheckerCheckFunctions = \u0097\u0005;
		}

		// Token: 0x06002BE5 RID: 11237 RVA: 0x00099E08 File Offset: 0x00098008
		public bool \u0001(_IExprement \u0002, ICompiledType \u0003, IScope5 \u0004, ref _ILiteralExpression \u0005)
		{
			if (!TypeTable.IsConcreteType(\u0003.DeRefType.Class))
			{
				return false;
			}
			if (\u0005.ConstantType == TypeClass.None || \u0005.ConstantType == TypeClass.AnyInt || \u0005.ConstantType == TypeClass.AnyReal)
			{
				\u0015.\u0001(\u0003, ref \u0005, \u0005);
				if (\u0003.DeRefType.Class == TypeClass.Pointer)
				{
					\u0015.\u0001(\u0004, \u0005);
				}
				else
				{
					\u0005.ConstantType = \u0003.DeRefType.Class;
				}
				\u0005.Type = \u0003.DeRefType;
				\u0005._Position = \u0002._Position;
				return true;
			}
			return false;
		}

		// Token: 0x06002BE6 RID: 11238 RVA: 0x00099EA4 File Offset: 0x000980A4
		private static void \u0001(IScope5 \u0002, _ILiteralExpression \u0003)
		{
			if (TypeTable.GetSize(TypeClass.Pointer, \u0002) == 8)
			{
				\u0003.ConstantType = TypeClass.LWord;
				return;
			}
			\u0003.ConstantType = TypeClass.DWord;
			if (\u0003.Negative)
			{
				long longValue = \u0003.LongValue;
				IntegerUnion integerUnion = new IntegerUnion
				{
					m_long = longValue
				};
				\u0003.LongValue = (long)((ulong)integerUnion.m_uint0);
			}
		}

		// Token: 0x06002BE7 RID: 11239 RVA: 0x00099EFC File Offset: 0x000980FC
		private static void \u0001(ICompiledType \u0002, ref _ILiteralExpression \u0003, _ILiteralExpression \u0004)
		{
			if (\u0004.ConstantType == TypeClass.AnyInt && (\u0002.DeRefType.Class == TypeClass.Real || \u0002.DeRefType.Class == TypeClass.LReal))
			{
				\u0003 = (\u0004.Negative ? \u0019.\u0003.\u0001((double)\u0004.LongValue) : \u0019.\u0003.\u0001(\u0004.ULongValue));
			}
		}

		// Token: 0x04000857 RID: 2135
		[CompilerGenerated]
		private readonly global::\u0014.\u0012 \u0001;
	}
}

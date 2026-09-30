using System;
using System.Collections.Generic;
using \u000E;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0013
{
	// Token: 0x0200018B RID: 395
	internal sealed class \u0004 : \u0008, IExprementVisitor<bool>
	{
		// Token: 0x06001B8C RID: 7052 RVA: 0x0005B070 File Offset: 0x00059270
		private \u0004(bool \u009E\u0007)
		{
			this.\u0001 = \u009E\u0007;
		}

		// Token: 0x06001B8D RID: 7053 RVA: 0x0005B080 File Offset: 0x00059280
		internal static bool \u0001(_IExprement3 \u0002, bool \u0003)
		{
			return new \u0004(\u0003).\u0001(\u0002);
		}

		// Token: 0x06001B8E RID: 7054 RVA: 0x0005B090 File Offset: 0x00059290
		private bool \u0001(_IExprement3 \u0002)
		{
			return \u0002.Accept<bool>(this);
		}

		// Token: 0x06001B8F RID: 7055 RVA: 0x0005B09C File Offset: 0x0005929C
		public bool \u0001(_IAssignmentExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001B90 RID: 7056 RVA: 0x0005B0A0 File Offset: 0x000592A0
		public bool \u0001(_IBitAccess \u0002)
		{
			return ((_IExprement3)\u0002._Base).Accept<bool>(this);
		}

		// Token: 0x06001B91 RID: 7057 RVA: 0x0005B0B4 File Offset: 0x000592B4
		public bool \u0001(_IPartialAccessExpression \u0002)
		{
			return ((_IExprement3)\u0002._Left).Accept<bool>(this);
		}

		// Token: 0x06001B92 RID: 7058 RVA: 0x0005B0C8 File Offset: 0x000592C8
		public bool \u0001(_INamespaceAccessExpression \u0002)
		{
			return ((_IExprement3)\u0002._Access).Accept<bool>(this);
		}

		// Token: 0x06001B93 RID: 7059 RVA: 0x0005B0DC File Offset: 0x000592DC
		public bool \u0001(_IStructureInitialization \u0002)
		{
			using (IEnumerator<_IAssignmentExpression> enumerator = \u0002._CompoInits.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!((_IExprement3)enumerator.Current).Accept<bool>(this))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06001B94 RID: 7060 RVA: 0x0005B138 File Offset: 0x00059338
		public bool \u0001(_IArrayInitialization \u0002)
		{
			using (IEnumerator<_IExpression> enumerator = \u0002._InitValues.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!((_IExprement3)enumerator.Current).Accept<bool>(this))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06001B95 RID: 7061 RVA: 0x0005B194 File Offset: 0x00059394
		public bool \u0001(_IOperatorExpression \u0002)
		{
			Operator code = \u0002.Code;
			if (code <= Operator.__LocalOffset)
			{
				if (code - Operator.IndexOf > 1 && code != Operator.__LocalOffset)
				{
					goto IL_34;
				}
			}
			else if (code - Operator.__TypeOf > 2 && code != Operator.XSizeOf)
			{
				goto IL_34;
			}
			return true;
			IL_34:
			using (IEnumerator<_IExpression> enumerator = \u0002._OperandsList.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!((_IExprement3)enumerator.Current).Accept<bool>(this))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06001B96 RID: 7062 RVA: 0x0005B224 File Offset: 0x00059424
		public bool \u0001(_IConversionExpression \u0002)
		{
			return ((_IExprement3)\u0002._Exp).Accept<bool>(this);
		}

		// Token: 0x06001B97 RID: 7063 RVA: 0x0005B238 File Offset: 0x00059438
		public bool \u0001(_IVariableExpression \u0002)
		{
			if (\u0002.PrecompileSignatureId == -1 || \u0002.PrecompileVariableId == -1)
			{
				return \u0002.PrecompileSignatureId == -1 && \u0002.PrecompileVariableId == -1;
			}
			ISignature6 signatureForPrecompileID = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(\u0002.PrecompileSignatureId);
			if (signatureForPrecompileID == null)
			{
				return false;
			}
			IVariable variable = signatureForPrecompileID[\u0002.PrecompileVariableId];
			if (variable == null)
			{
				return false;
			}
			if (this.\u0001)
			{
				return variable.HasFlag(VarFlag.ReplacedConstant | VarFlag.Constant);
			}
			return variable.HasFlag(VarFlag.ReplacedConstant);
		}

		// Token: 0x06001B98 RID: 7064 RVA: 0x0005B2B8 File Offset: 0x000594B8
		public bool \u0001(_IIndexAccessExpression \u0002)
		{
			if (!this.\u0001)
			{
				return false;
			}
			if (!((_IExprement3)\u0002._Var).Accept<bool>(this))
			{
				return false;
			}
			using (IEnumerator<_IExpression> enumerator = \u0002._Accesses.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!((_IExprement3)enumerator.Current).Accept<bool>(this))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06001B99 RID: 7065 RVA: 0x0005B330 File Offset: 0x00059530
		public bool \u0001(_ICompoAccessExpression \u0002)
		{
			return ((\u0002._Left.PrecompileSignatureId != -1 || \u0002._Left.PrecompileVariableId != -1) && ((_IExprement3)\u0002._Left).Accept<bool>(this)) || ((\u0002.Left.Type == null || !TypeTable.IsInteger(\u0002.Left.Type.Class)) && ((_IExprement3)\u0002._Right).Accept<bool>(this));
		}

		// Token: 0x06001B9A RID: 7066 RVA: 0x0005B3AC File Offset: 0x000595AC
		public bool \u0001(_IGlobalScopeExpression \u0002)
		{
			return ((_IExprement3)\u0002._Base).Accept<bool>(this);
		}

		// Token: 0x06001B9B RID: 7067 RVA: 0x0005B3C0 File Offset: 0x000595C0
		public bool \u0001(_IPoolScopeExpression \u0002)
		{
			return ((_IExprement3)\u0002._Base).Accept<bool>(this);
		}

		// Token: 0x06001B9C RID: 7068 RVA: 0x0005B3D4 File Offset: 0x000595D4
		public bool \u0001(_ICopyScopeExpression \u0002)
		{
			return ((_IExprement3)\u0002._Base).Accept<bool>(this);
		}

		// Token: 0x06001B9D RID: 7069 RVA: 0x0005B3E8 File Offset: 0x000595E8
		public bool \u0001(_ISystemScopeExpression \u0002)
		{
			return ((_IExprement3)\u0002._Base).Accept<bool>(this);
		}

		// Token: 0x06001B9E RID: 7070 RVA: 0x0005B3FC File Offset: 0x000595FC
		public bool \u0001(_IMultipleIndexInitialization \u0002)
		{
			return ((_IExprement3)\u0002._Value).Accept<bool>(this) && ((_IExprement3)\u0002._Number).Accept<bool>(this);
		}

		// Token: 0x06001B9F RID: 7071 RVA: 0x0005B424 File Offset: 0x00059624
		public bool \u0001(_ICurrentTaskExpression \u0002)
		{
			return ((_IExprement3)\u0002._Base).Accept<bool>(this);
		}

		// Token: 0x06001BA0 RID: 7072 RVA: 0x0005B438 File Offset: 0x00059638
		public bool \u0001(_IFramePointerExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001BA1 RID: 7073 RVA: 0x0005B43C File Offset: 0x0005963C
		public bool \u0001(_IProgramCounterExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001BA2 RID: 7074 RVA: 0x0005B440 File Offset: 0x00059640
		public bool \u0001(_IDeRefAccessExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001BA3 RID: 7075 RVA: 0x0005B444 File Offset: 0x00059644
		public bool \u0001(_IIsEnumTypeExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001BA4 RID: 7076 RVA: 0x0005B448 File Offset: 0x00059648
		public bool \u0001(_INewExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001BA5 RID: 7077 RVA: 0x0005B44C File Offset: 0x0005964C
		public bool \u0001(_ITypeExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001BA6 RID: 7078 RVA: 0x0005B450 File Offset: 0x00059650
		public bool \u0001(_IProjectDefinedExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001BA7 RID: 7079 RVA: 0x0005B454 File Offset: 0x00059654
		public bool \u0001(_IHasConstantTypeExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001BA8 RID: 7080 RVA: 0x0005B458 File Offset: 0x00059658
		public bool \u0001(_IErrorExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001BA9 RID: 7081 RVA: 0x0005B45C File Offset: 0x0005965C
		public bool \u0001(_ICastExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001BAA RID: 7082 RVA: 0x0005B460 File Offset: 0x00059660
		public bool \u0001(_ICompilerVersionExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001BAB RID: 7083 RVA: 0x0005B464 File Offset: 0x00059664
		public bool \u0001(_IRuntimeVersionExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001BAC RID: 7084 RVA: 0x0005B468 File Offset: 0x00059668
		public bool \u0001(_IHasConstantValueExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001BAD RID: 7085 RVA: 0x0005B46C File Offset: 0x0005966C
		public bool \u0001(_ICallExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001BAE RID: 7086 RVA: 0x0005B470 File Offset: 0x00059670
		public bool \u0001(_ICallInstanceExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001BAF RID: 7087 RVA: 0x0005B474 File Offset: 0x00059674
		public bool \u0001(_IThisExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001BB0 RID: 7088 RVA: 0x0005B478 File Offset: 0x00059678
		public bool \u0001(_IBaseExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001BB1 RID: 7089 RVA: 0x0005B47C File Offset: 0x0005967C
		public bool \u0001(_ILiteralExpression \u0002)
		{
			return true;
		}

		// Token: 0x06001BB2 RID: 7090 RVA: 0x0005B480 File Offset: 0x00059680
		public bool \u0001(_IAddressExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001BB3 RID: 7091 RVA: 0x0005B484 File Offset: 0x00059684
		public bool \u0001(_ICaseRangeExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001BB4 RID: 7092 RVA: 0x0005B488 File Offset: 0x00059688
		public bool \u0001(_IDefineReference \u0002)
		{
			return false;
		}

		// Token: 0x06001BB5 RID: 7093 RVA: 0x0005B48C File Offset: 0x0005968C
		public bool \u0001(_IVariableReference \u0002)
		{
			return false;
		}

		// Token: 0x06001BB6 RID: 7094 RVA: 0x0005B490 File Offset: 0x00059690
		public bool \u0001(_ITypeReference \u0002)
		{
			return false;
		}

		// Token: 0x06001BB7 RID: 7095 RVA: 0x0005B494 File Offset: 0x00059694
		public bool \u0001(_IPouReference \u0002)
		{
			return false;
		}

		// Token: 0x06001BB8 RID: 7096 RVA: 0x0005B498 File Offset: 0x00059698
		public bool \u0001(_IDefinedExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001BB9 RID: 7097 RVA: 0x0005B49C File Offset: 0x0005969C
		public bool \u0001(_IPragmaOperatorExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001BBA RID: 7098 RVA: 0x0005B4A0 File Offset: 0x000596A0
		public bool \u0001(_IHasTypeExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001BBB RID: 7099 RVA: 0x0005B4A4 File Offset: 0x000596A4
		public bool \u0001(_IHasAttributeExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001BBC RID: 7100 RVA: 0x0005B4A8 File Offset: 0x000596A8
		public bool \u0001(_IHasValueExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001BBD RID: 7101 RVA: 0x0005B4AC File Offset: 0x000596AC
		public bool \u0001(_IResourceReference \u0002)
		{
			return false;
		}

		// Token: 0x06001BBE RID: 7102 RVA: 0x0005B4B0 File Offset: 0x000596B0
		public bool \u0001(_IXRefExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001BBF RID: 7103 RVA: 0x0005B4B4 File Offset: 0x000596B4
		public bool \u0001(_ITaskReference \u0002)
		{
			return false;
		}

		// Token: 0x06001BC0 RID: 7104 RVA: 0x0005B4B8 File Offset: 0x000596B8
		public bool \u0001(_IQualifiedNameExpression \u0002)
		{
			return false;
		}

		// Token: 0x06001BC1 RID: 7105 RVA: 0x0005B4BC File Offset: 0x000596BC
		public bool \u0001(_INullExpression \u0002)
		{
			return false;
		}

		// Token: 0x040004C1 RID: 1217
		private readonly bool \u0001;
	}
}

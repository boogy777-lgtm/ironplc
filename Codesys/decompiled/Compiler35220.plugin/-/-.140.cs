using System;
using System.Runtime.CompilerServices;
using \u0017;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u000F
{
	// Token: 0x0200018E RID: 398
	internal sealed class \u0007 : ICommonScope, \u0010
	{
		// Token: 0x170005AB RID: 1451
		// (get) Token: 0x06001BE2 RID: 7138 RVA: 0x0005BC68 File Offset: 0x00059E68
		// (set) Token: 0x06001BE3 RID: 7139 RVA: 0x0005BC70 File Offset: 0x00059E70
		private _IPreCompileContext PreCompileContext { get; set; }

		// Token: 0x06001BE4 RID: 7140 RVA: 0x0005BC7C File Offset: 0x00059E7C
		public \u0007(_IPreCompileContext \u0097\u0002)
		{
			this.PreCompileContext = \u0097\u0002;
		}

		// Token: 0x06001BE5 RID: 7141 RVA: 0x0005BC8C File Offset: 0x00059E8C
		public _IVariable \u0001(_IExpression \u0002)
		{
			if (\u0002 == null)
			{
				return null;
			}
			_ISignature isignature = this.\u0001(\u0002);
			if (isignature != null)
			{
				return this.\u0001(isignature.PrecompileId, \u0002.PrecompileVariableId);
			}
			return null;
		}

		// Token: 0x06001BE6 RID: 7142 RVA: 0x0005BCC0 File Offset: 0x00059EC0
		public _ISignature \u0001(_IExpression \u0002)
		{
			if (\u0002 == null)
			{
				return null;
			}
			_IVariableExpression ivariableExpression = \u0002 as _IVariableExpression;
			if (ivariableExpression != null)
			{
				return this.\u0001(ivariableExpression.PrecompileSignatureId);
			}
			_ICompoAccessExpression icompoAccessExpression = \u0002 as _ICompoAccessExpression;
			if (icompoAccessExpression != null)
			{
				return this.\u0001(icompoAccessExpression.PrecompileSignatureId);
			}
			return null;
		}

		// Token: 0x06001BE7 RID: 7143 RVA: 0x0005BD04 File Offset: 0x00059F04
		public _ISignature \u0001(IUserdefType \u0002)
		{
			if (\u0002 == null)
			{
				return null;
			}
			return this.\u0001(\u0002.SignatureId);
		}

		// Token: 0x06001BE8 RID: 7144 RVA: 0x0005BD18 File Offset: 0x00059F18
		public _ISignature \u0001(int \u0002)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(\u0002) as _ISignature;
		}

		// Token: 0x06001BE9 RID: 7145 RVA: 0x0005BD30 File Offset: 0x00059F30
		private _IVariable \u0001(int \u0002, int \u0003)
		{
			ISignature signature = this.\u0001(\u0002);
			if (signature != null)
			{
				return signature[\u0003] as _IVariable;
			}
			return null;
		}

		// Token: 0x06001BEA RID: 7146 RVA: 0x0005BD58 File Offset: 0x00059F58
		public int \u0001(IType \u0002)
		{
			return TypeTable.GetSize2(\u0002.Class, this);
		}

		// Token: 0x06001BEB RID: 7147 RVA: 0x0005BD68 File Offset: 0x00059F68
		public bool \u0001(ISignature \u0002, ISignature \u0003, ICommonScope \u0004)
		{
			_ISignature isignature = \u0002 as _ISignature;
			_ISignature isignature2 = \u0003 as _ISignature;
			return isignature != null && isignature2 != null && (isignature.ObjectGuid == isignature2.ObjectGuid || (isignature.BaseExpression != null || isignature2.BaseExpression != null));
		}

		// Token: 0x06001BEC RID: 7148 RVA: 0x0005BDB4 File Offset: 0x00059FB4
		public ISignature \u0001(IUserdefType \u0002)
		{
			return this.\u0001(\u0002.SignatureId);
		}

		// Token: 0x06001BED RID: 7149 RVA: 0x0005BDC4 File Offset: 0x00059FC4
		public ISignature[] \u0001(IExpression \u0002)
		{
			ISignature signature = this.\u0001(\u0002 as _IExpression);
			if (signature != null)
			{
				return new ISignature[]
				{
					signature
				};
			}
			return null;
		}

		// Token: 0x06001BEE RID: 7150 RVA: 0x0005BDF0 File Offset: 0x00059FF0
		public ISignature \u0001(IEnumType \u0002)
		{
			return this.\u0001(((IEnumType2)\u0002).SignatureId);
		}

		// Token: 0x06001BEF RID: 7151 RVA: 0x0005BE04 File Offset: 0x0005A004
		public bool \u0001(IUserdefType \u0002, IUserdefType \u0003)
		{
			string a = ((_IUserdefType)\u0002).NameExpression.ToString();
			string b = ((_IUserdefType)\u0003).NameExpression.ToString();
			return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06001BF0 RID: 7152 RVA: 0x0005BE3C File Offset: 0x0005A03C
		public ILiteralValue \u0001(IExpression \u0002, bool \u0003)
		{
			if (\u0002.IsLiteral)
			{
				return ((_ILiteralExpression)\u0002).LiteralValue;
			}
			return null;
		}

		// Token: 0x170005AC RID: 1452
		// (get) Token: 0x06001BF1 RID: 7153 RVA: 0x0005BE54 File Offset: 0x0005A054
		public int PointerSize
		{
			get
			{
				return this.PreCompileContext.PointerSize;
			}
		}

		// Token: 0x170005AD RID: 1453
		// (get) Token: 0x06001BF2 RID: 7154 RVA: 0x0005BE64 File Offset: 0x0005A064
		public bool IsPrecompileScope
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170005AE RID: 1454
		// (get) Token: 0x06001BF3 RID: 7155 RVA: 0x0005BE68 File Offset: 0x0005A068
		public bool ConvertAllTypeMismatches
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06001BF4 RID: 7156 RVA: 0x0005BE6C File Offset: 0x0005A06C
		public void AddError(_IExprement exp, MessageId mid, params object[] args)
		{
		}

		// Token: 0x06001BF5 RID: 7157 RVA: 0x0005BE70 File Offset: 0x0005A070
		public void \u0001(_IExprement \u0002, MessageId \u0003, params object[] \u0004)
		{
		}

		// Token: 0x170005AF RID: 1455
		// (get) Token: 0x06001BF6 RID: 7158 RVA: 0x0005BE74 File Offset: 0x0005A074
		public bool InImplicitCode
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170005B0 RID: 1456
		// (get) Token: 0x06001BF7 RID: 7159 RVA: 0x0005BE78 File Offset: 0x0005A078
		public Guid ApplicationGuid
		{
			get
			{
				return this.PreCompileContext.ApplicationGuid;
			}
		}

		// Token: 0x170005B1 RID: 1457
		// (get) Token: 0x06001BF8 RID: 7160 RVA: 0x0005BE88 File Offset: 0x0005A088
		public bool TreatLRealAsReal
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170005B2 RID: 1458
		// (get) Token: 0x06001BF9 RID: 7161 RVA: 0x0005BE8C File Offset: 0x0005A08C
		public bool TreatInt64AsInt32
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170005B3 RID: 1459
		// (get) Token: 0x06001BFA RID: 7162 RVA: 0x0005BE90 File Offset: 0x0005A090
		public bool NoConversionChecks
		{
			get
			{
				return this.PreCompileContext.IsDefined("NO_3_0_CONVERSION_CHECKS");
			}
		}

		// Token: 0x06001BFB RID: 7163 RVA: 0x0005BEA4 File Offset: 0x0005A0A4
		public bool \u0001(_IImplicitConversionExpression \u0002)
		{
			return false;
		}

		// Token: 0x040004C5 RID: 1221
		[CompilerGenerated]
		private _IPreCompileContext \u0001;
	}
}

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using \u0019;
using \u001C;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0080;

namespace _3S.CoDeSys.Compiler35220.Phase1_Typification
{
	// Token: 0x0200030C RID: 780
	public class NameManglingService : ILMNameManglingService
	{
		// Token: 0x170007DD RID: 2013
		// (get) Token: 0x06002F22 RID: 12066 RVA: 0x000B13D0 File Offset: 0x000AF5D0
		internal static ILMNameManglingService Instance { get; } = new NameManglingService(null, null);

		// Token: 0x06002F23 RID: 12067 RVA: 0x000B13D8 File Offset: 0x000AF5D8
		private NameManglingService(\u001C.\u0011 lookupService, ICommonScope scope)
		{
			this.\u0001 = scope;
			this.\u0001 = lookupService;
		}

		// Token: 0x06002F24 RID: 12068 RVA: 0x000B13F0 File Offset: 0x000AF5F0
		private static string \u0001(_ISignature \u0002, _ICompileContext \u0003, ICommonScope \u0004)
		{
			return new NameManglingService(new \u0080.\u0014(\u0003), \u0004).\u0001(\u0002).Replace("__", "_");
		}

		// Token: 0x06002F25 RID: 12069 RVA: 0x000B1414 File Offset: 0x000AF614
		internal static string \u0001(ICompiledType \u0002, \u001C.\u0011 \u0003, ICommonScope \u0004)
		{
			StringBuilder stringBuilder = new StringBuilder();
			new NameManglingService(\u0003, \u0004).\u0001(\u0002, stringBuilder);
			return stringBuilder.ToString();
		}

		// Token: 0x06002F26 RID: 12070 RVA: 0x000B143C File Offset: 0x000AF63C
		internal static void \u0001(_ICompileContext \u0002)
		{
			foreach (_ISignature4 isignature in \u0002.AllFlat.OfType<_ISignature4>().Where(new Func<_ISignature4, bool>(NameManglingService.<>c.<>9.\u0001)))
			{
				_ISignature4 isignature2 = \u0002.GetSignatureById(isignature.ParentSignatureId) as _ISignature4;
				if (isignature2 != null)
				{
					_IVariableExpression ivariableExpression = \u0019.\u0003.\u0001(NameManglingService.\u0001(isignature, \u0002, \u0002.CreateIScope(isignature.Id) as ICommonScope));
					ivariableExpression._Position = isignature._NameExpression._Position;
					isignature2.ChangeSubSignatureName(isignature, ivariableExpression);
				}
			}
		}

		// Token: 0x06002F27 RID: 12071 RVA: 0x000B14F8 File Offset: 0x000AF6F8
		internal static string \u0001(_ISignature \u0002)
		{
			return NameManglingService.\u0001(\u0002, false);
		}

		// Token: 0x06002F28 RID: 12072 RVA: 0x000B1504 File Offset: 0x000AF704
		private static string \u0001(_ISignature \u0002, bool \u0003)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (\u0003)
			{
				NameManglingService.\u0002(stringBuilder, \u0002);
			}
			else
			{
				NameManglingService.\u0001(stringBuilder, \u0002);
			}
			return stringBuilder.ToString();
		}

		// Token: 0x06002F29 RID: 12073 RVA: 0x000B1530 File Offset: 0x000AF730
		private static void \u0001(StringBuilder \u0002, _ISignature \u0003)
		{
			\u0002.Append("`");
			\u0002.Append(\u0003.Name);
			foreach (IVariable variable in \u0003.AllInputs)
			{
				\u0002.Append(string.Format("@{0}@", variable.Type));
			}
			\u0002.Append("`");
		}

		// Token: 0x06002F2A RID: 12074 RVA: 0x000B1594 File Offset: 0x000AF794
		private static void \u0002(StringBuilder \u0002, _ISignature \u0003)
		{
			\u0002.Append(\u0003.OrgName);
			\u0002.Append("(");
			bool flag = false;
			foreach (IVariable variable in \u0003.AllInputs)
			{
				if (flag)
				{
					\u0002.Append(", ");
				}
				\u0002.Append(string.Format("{0}", variable.Type));
				flag = true;
			}
			\u0002.Append(")");
		}

		// Token: 0x06002F2B RID: 12075 RVA: 0x000B160C File Offset: 0x000AF80C
		public string GetContextFreeMangledName(ISignature sign, bool bHumanReadable)
		{
			return NameManglingService.\u0001((_ISignature)sign, bHumanReadable);
		}

		// Token: 0x06002F2C RID: 12076 RVA: 0x000B161C File Offset: 0x000AF81C
		private string \u0001(ISignature \u0002)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("`");
			string attributeValue = \u0002.GetAttributeValue("overloads");
			stringBuilder.Append(attributeValue);
			foreach (IVariable variable in \u0002.AllInputs)
			{
				if (!variable.GetFlag(VarFlag.Implicit))
				{
					stringBuilder.Append("@");
					this.\u0001(variable.CompiledType, stringBuilder);
					stringBuilder.Append("@");
				}
			}
			stringBuilder.Append("`");
			return stringBuilder.ToString();
		}

		// Token: 0x06002F2D RID: 12077 RVA: 0x000B16B0 File Offset: 0x000AF8B0
		private void \u0001(ICompiledType \u0002, StringBuilder \u0003)
		{
			_IWStringType iwstringType = \u0002 as _IWStringType;
			if (iwstringType != null)
			{
				this.\u0001(iwstringType, \u0003);
				return;
			}
			_IStringType istringType = \u0002 as _IStringType;
			if (istringType != null)
			{
				this.\u0001(istringType, \u0003);
				return;
			}
			_IUserdefType iuserdefType = \u0002 as _IUserdefType;
			if (iuserdefType != null)
			{
				this.\u0001(iuserdefType, \u0003);
				return;
			}
			_IArrayType iarrayType = \u0002 as _IArrayType;
			if (iarrayType != null)
			{
				this.\u0001(iarrayType, \u0003);
				return;
			}
			_ISubrangeType isubrangeType = \u0002 as _ISubrangeType;
			if (isubrangeType == null)
			{
				\u0003.Append(\u0002);
				return;
			}
			this.\u0001(isubrangeType, \u0003);
		}

		// Token: 0x06002F2E RID: 12078 RVA: 0x000B172C File Offset: 0x000AF92C
		private void \u0001(_IWStringType \u0002, StringBuilder \u0003)
		{
			\u0003.Append("WSTRING(");
			_IExpression iexpression = \u0002.LengthExpression as _IExpression;
			if (iexpression != null)
			{
				this.\u0001(iexpression, \u0003);
			}
			\u0003.Append(")");
		}

		// Token: 0x06002F2F RID: 12079 RVA: 0x000B1768 File Offset: 0x000AF968
		private void \u0001(_IStringType \u0002, StringBuilder \u0003)
		{
			\u0003.Append("STRING(");
			_IExpression iexpression = \u0002.LengthExpression as _IExpression;
			if (iexpression != null)
			{
				this.\u0001(iexpression, \u0003);
			}
			\u0003.Append(")");
		}

		// Token: 0x06002F30 RID: 12080 RVA: 0x000B17A4 File Offset: 0x000AF9A4
		private void \u0001(_IUserdefType \u0002, StringBuilder \u0003)
		{
			_ISignature isignature = this.\u0001.FindSignature(\u0002) as _ISignature;
			if (isignature == null)
			{
				\u0003.Append(\u0002);
				return;
			}
			\u0003.Append(this.\u0001.\u0001(isignature));
		}

		// Token: 0x06002F31 RID: 12081 RVA: 0x000B17E4 File Offset: 0x000AF9E4
		private void \u0001(_IArrayType \u0002, StringBuilder \u0003)
		{
			\u0003.Append("[");
			foreach (_IArrayDimension iarrayDimension in \u0002._Dimensions)
			{
				this.\u0001(iarrayDimension._LowerBorder, \u0003);
				\u0003.Append("..");
				this.\u0001(iarrayDimension._UpperBorder, \u0003);
				\u0003.Append(",");
			}
			\u0003.Append("]");
			this.\u0001(\u0002._Base, \u0003);
		}

		// Token: 0x06002F32 RID: 12082 RVA: 0x000B1884 File Offset: 0x000AFA84
		private void \u0001(_ISubrangeType \u0002, StringBuilder \u0003)
		{
			this.\u0001(\u0002._Base, \u0003);
			\u0003.Append("(");
			this.\u0001(\u0002._LowerBorder, \u0003);
			\u0003.Append("..");
			this.\u0001(\u0002._UpperBorder, \u0003);
			\u0003.Append(")");
		}

		// Token: 0x06002F33 RID: 12083 RVA: 0x000B18DC File Offset: 0x000AFADC
		private bool \u0001(_IExpression \u0002, StringBuilder \u0003)
		{
			_IOperatorExpression ioperatorExpression = \u0002 as _IOperatorExpression;
			if (ioperatorExpression != null)
			{
				if (ioperatorExpression.Code != Operator.SizeOf && ioperatorExpression.Code != Operator.XSizeOf)
				{
					\u0003.Append(Scanner.GetTextOfOperator(ioperatorExpression.Code));
					\u0003.Append("(");
					foreach (_IExpression u in ioperatorExpression._OperandsList)
					{
						if (!this.\u0002(u, \u0003))
						{
							return false;
						}
						\u0003.Append(",");
					}
					\u0003.Append(")");
					return true;
				}
				if (!ioperatorExpression.Operands.Any<IExpression>())
				{
					return false;
				}
				\u0003.Append(Scanner.GetTextOfOperator(ioperatorExpression.Code));
				\u0003.Append("(");
				this.\u0001(ioperatorExpression.Operands.First<IExpression>().Type, \u0003);
				\u0003.Append(")");
				return true;
			}
			return false;
		}

		// Token: 0x06002F34 RID: 12084 RVA: 0x000B19E4 File Offset: 0x000AFBE4
		private bool \u0002(_IExpression \u0002, StringBuilder \u0003)
		{
			ILiteralValue literalValue = this.\u0001.GetLiteralValue(\u0002, true);
			int @int;
			if (literalValue != null && literalValue.GetInt(out @int))
			{
				bool flag;
				@int = literalValue.GetInt(out flag);
				\u0003.Append(@int.ToString());
				return true;
			}
			return this.\u0001(\u0002, \u0003);
		}

		// Token: 0x06002F35 RID: 12085 RVA: 0x000B1A34 File Offset: 0x000AFC34
		private void \u0001(_IExpression \u0002, StringBuilder \u0003)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (this.\u0002(\u0002, stringBuilder))
			{
				\u0003.Append(stringBuilder);
				return;
			}
			\u0003.Append(\u0002.ToString());
		}

		// Token: 0x040008FA RID: 2298
		private readonly \u001C.\u0011 \u0001;

		// Token: 0x040008FB RID: 2299
		private readonly ICommonScope \u0001;

		// Token: 0x040008FC RID: 2300
		[CompilerGenerated]
		private static readonly ILMNameManglingService \u0001;
	}
}

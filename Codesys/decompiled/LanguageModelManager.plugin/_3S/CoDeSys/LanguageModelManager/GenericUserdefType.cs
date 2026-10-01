using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020001A5 RID: 421
	[TypeGuid("{CA3BCC2A-606F-4F21-B198-C82DF5B0A340}")]
	[StorageVersion("3.5.18.0")]
	public class GenericUserdefType : UserdefType, IGenericUserdefType2, IGenericUserdefType, _IUserdefType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, IUserdefType2, IUserdefType
	{
		// Token: 0x17000800 RID: 2048
		// (get) Token: 0x06001E95 RID: 7829 RVA: 0x000549BA File Offset: 0x000539BA
		// (set) Token: 0x06001E96 RID: 7830 RVA: 0x000549C2 File Offset: 0x000539C2
		[DefaultSerialization("Initializations")]
		[StorageVersion("3.5.19.30")]
		[StorageDefaultValueEmptyCollection]
		[DefaultDuplication(DuplicationMethod.Deep)]
		[Obfuscation(Feature = "rename")]
		private LList<_IExpression> Initializations { get; set; } = new LList<_IExpression>();

		// Token: 0x17000801 RID: 2049
		// (get) Token: 0x06001E97 RID: 7831 RVA: 0x000549CB File Offset: 0x000539CB
		public IEnumerable<_IExpression> GenericConstantsInitializations
		{
			get
			{
				return this.Initializations;
			}
		}

		// Token: 0x06001E98 RID: 7832 RVA: 0x000549D3 File Offset: 0x000539D3
		public GenericUserdefType()
		{
		}

		// Token: 0x06001E99 RID: 7833 RVA: 0x000549E6 File Offset: 0x000539E6
		internal GenericUserdefType(_IExpression qne) : base(qne)
		{
		}

		// Token: 0x06001E9A RID: 7834 RVA: 0x000549FA File Offset: 0x000539FA
		public GenericUserdefType(string stName) : base(stName)
		{
		}

		// Token: 0x06001E9B RID: 7835 RVA: 0x00054A0E File Offset: 0x00053A0E
		public void AddGenericConstantInitialization(_IExpression exp)
		{
			this.Initializations.Add(exp);
		}

		// Token: 0x06001E9C RID: 7836 RVA: 0x00054A1C File Offset: 0x00053A1C
		public void SetGenericConstantInitialization(_IExpression exp, int index)
		{
			this.Initializations[index] = exp;
		}

		// Token: 0x06001E9D RID: 7837 RVA: 0x00054A2C File Offset: 0x00053A2C
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.ToString());
			stringBuilder.Append("<");
			for (int i = 0; i < this.Initializations.Count; i++)
			{
				_IExpression iexpression = this.Initializations[i];
				object obj = iexpression is IVariableExpression || iexpression is ILiteralExpression;
				bool flag = i == this.Initializations.Count - 1;
				object obj2 = obj;
				if (obj2 == null)
				{
					stringBuilder.Append("(");
				}
				stringBuilder.Append(CompilerProxy._ExprementWriter.WriteExprement(iexpression, WriteExprementFlags.MinimalParentheses));
				if (obj2 == null)
				{
					stringBuilder.Append(")");
				}
				if (!flag)
				{
					stringBuilder.Append(", ");
				}
			}
			stringBuilder.Append(">");
			return stringBuilder.ToString();
		}

		// Token: 0x06001E9E RID: 7838 RVA: 0x00054AF5 File Offset: 0x00053AF5
		public override void Accept(ITypeVisitor typvis)
		{
			if (typvis is ITypeVisitor4)
			{
				(typvis as ITypeVisitor4).visit(this);
				return;
			}
			base.Accept(typvis);
		}

		// Token: 0x06001E9F RID: 7839 RVA: 0x00054B13 File Offset: 0x00053B13
		public override bool IsEqualPreCompile(ICompiledType type, IScope scope)
		{
			return type is GenericUserdefType && base.IsEqualPreCompile(type, scope);
		}

		// Token: 0x06001EA0 RID: 7840 RVA: 0x00054B28 File Offset: 0x00053B28
		public override _IType _Duplicate(bool bDeep)
		{
			GenericUserdefType genericUserdefType;
			if (this.m_qneTypeDef != null)
			{
				genericUserdefType = new GenericUserdefType(this.m_qneTypeDef.Duplicate() as _IExpression);
			}
			else
			{
				genericUserdefType = new GenericUserdefType(null);
			}
			if (bDeep)
			{
				genericUserdefType.SignatureId = base.SignatureId;
				genericUserdefType.ScopeId = base.ScopeId;
			}
			else
			{
				genericUserdefType.SignatureId = Common.InvalidID;
				genericUserdefType.ScopeId = Common.InvalidID;
			}
			foreach (_IExpression iexpression in this.Initializations)
			{
				genericUserdefType.AddGenericConstantInitialization(iexpression.Duplicate() as _IExpression);
			}
			return genericUserdefType;
		}

		// Token: 0x06001EA1 RID: 7841 RVA: 0x00054BDC File Offset: 0x00053BDC
		public _IUserdefType GetNonGenericBaseType()
		{
			return new UserdefType(this.m_qneTypeDef);
		}

		// Token: 0x17000802 RID: 2050
		// (get) Token: 0x06001EA2 RID: 7842 RVA: 0x00054BE9 File Offset: 0x00053BE9
		// (set) Token: 0x06001EA3 RID: 7843 RVA: 0x00054BF1 File Offset: 0x00053BF1
		public string OriginalDeclaration { get; set; }
	}
}

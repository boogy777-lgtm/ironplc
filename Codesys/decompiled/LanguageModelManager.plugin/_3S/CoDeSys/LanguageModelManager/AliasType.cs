using System;
using System.Reflection;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020001A3 RID: 419
	[TypeGuid("{4b4ec020-a837-40a5-a308-89707ef17af6}")]
	[StorageVersion("3.3.0.0")]
	public class AliasType : UserdefType, _IAliasType, _IUserdefType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, IUserdefType2, IUserdefType, IAliasTypeSerializable
	{
		// Token: 0x06001E67 RID: 7783 RVA: 0x00053CB3 File Offset: 0x00052CB3
		public AliasType()
		{
		}

		// Token: 0x06001E68 RID: 7784 RVA: 0x00053CBB File Offset: 0x00052CBB
		public AliasType(_IType orgType)
		{
			this.m_orgType = orgType;
		}

		// Token: 0x170007F9 RID: 2041
		// (get) Token: 0x06001E69 RID: 7785 RVA: 0x00053CCA File Offset: 0x00052CCA
		public override ICompiledType EffectiveType
		{
			get
			{
				return this.m_orgType;
			}
		}

		// Token: 0x06001E6A RID: 7786 RVA: 0x00053CD2 File Offset: 0x00052CD2
		public void SetEffectiveType(ICompiledType effectiveType)
		{
			this.m_orgType = (effectiveType as _IType);
		}

		// Token: 0x06001E6B RID: 7787 RVA: 0x00053CE0 File Offset: 0x00052CE0
		public override bool IsEqual(ICompiledType type, IScope scope)
		{
			AliasType aliasType = type as AliasType;
			return aliasType != null && base.IsEqual(type, scope) && ((ICompiledType3)this.EffectiveType).IsEqual(aliasType.EffectiveType, scope);
		}

		// Token: 0x06001E6C RID: 7788 RVA: 0x00053D1C File Offset: 0x00052D1C
		public override bool IsEqualPreCompile(ICompiledType type, IScope scope)
		{
			AliasType aliasType = type as AliasType;
			return aliasType != null && base.IsEqualPreCompile(type, scope) && ((ICompiledType5)this.EffectiveType).IsEqualPreCompile(aliasType.EffectiveType, scope);
		}

		// Token: 0x170007FA RID: 2042
		// (get) Token: 0x06001E6D RID: 7789 RVA: 0x00053D58 File Offset: 0x00052D58
		// (set) Token: 0x06001E6E RID: 7790 RVA: 0x00053D60 File Offset: 0x00052D60
		public _IExpression _DefaultValue
		{
			get
			{
				return this.m_defaultValue;
			}
			set
			{
				this.m_defaultValue = value;
			}
		}

		// Token: 0x06001E6F RID: 7791 RVA: 0x00053D6C File Offset: 0x00052D6C
		public override void Accept(ITypeVisitor typvis)
		{
			ITypeVisitor3 typeVisitor = typvis as ITypeVisitor3;
			if (typeVisitor != null)
			{
				typeVisitor.visit(this);
				return;
			}
			typvis.visit(this);
		}

		// Token: 0x06001E70 RID: 7792 RVA: 0x00053D94 File Offset: 0x00052D94
		private _IType DuplicateFlat()
		{
			return new AliasType(this.m_orgType.Duplicate)
			{
				m_qneTypeDef = (this.m_qneTypeDef.Duplicate() as _IExpression),
				SignatureId = Common.InvalidID,
				ScopeId = Common.InvalidID,
				_DefaultValue = this._DefaultValue
			};
		}

		// Token: 0x06001E71 RID: 7793 RVA: 0x00053DEC File Offset: 0x00052DEC
		public override _IType _Duplicate(bool bDeep)
		{
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351930)
			{
				return this.DuplicateFlat();
			}
			AliasType aliasType = new AliasType(this.m_orgType._Duplicate(bDeep))
			{
				m_qneTypeDef = (this.m_qneTypeDef.Duplicate() as _IExpression),
				_DefaultValue = this._DefaultValue
			};
			if (bDeep)
			{
				aliasType.SignatureId = base.SignatureId;
				aliasType.ScopeId = base.ScopeId;
			}
			else
			{
				aliasType.SignatureId = Common.InvalidID;
				aliasType.ScopeId = Common.InvalidID;
			}
			return aliasType;
		}

		// Token: 0x06001E72 RID: 7794 RVA: 0x00053E79 File Offset: 0x00052E79
		public override void BeforeSerialize()
		{
			if (this._DefaultValue != null)
			{
				this._DefaultValue.Type = null;
			}
			base.BeforeSerialize();
		}

		// Token: 0x040005FF RID: 1535
		[DefaultSerialization("OrgType")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IType m_orgType;

		// Token: 0x04000600 RID: 1536
		[DefaultSerialization("DefaultValue")]
		[StorageVersion("3.5.8.0")]
		[StorageDefaultValue(null)]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_defaultValue;
	}
}

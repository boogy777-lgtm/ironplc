using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000198 RID: 408
	[TypeGuid("{861aad71-b796-49c8-8e44-0d476c5ed1ab}")]
	[StorageVersion("3.3.0.0")]
	public class ReferenceType : IECType, _IReferenceType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, IReferenceType2, IReferenceType
	{
		// Token: 0x06001D7D RID: 7549 RVA: 0x0004EC38 File Offset: 0x0004DC38
		public ReferenceType()
		{
		}

		// Token: 0x06001D7E RID: 7550 RVA: 0x000514BB File Offset: 0x000504BB
		public ReferenceType(_IType typeBase)
		{
			this.m_typeBase = typeBase;
		}

		// Token: 0x170007BB RID: 1979
		// (get) Token: 0x06001D7F RID: 7551 RVA: 0x000514CA File Offset: 0x000504CA
		// (set) Token: 0x06001D80 RID: 7552 RVA: 0x000514E6 File Offset: 0x000504E6
		public _IType _Base
		{
			get
			{
				if (this.m_typeBase == null)
				{
					return null;
				}
				return this.m_typeBase.EffectiveType as _IType;
			}
			set
			{
				this.m_typeBase = value;
			}
		}

		// Token: 0x170007BC RID: 1980
		// (get) Token: 0x06001D81 RID: 7553 RVA: 0x000514EF File Offset: 0x000504EF
		public override ICompiledType BaseType
		{
			get
			{
				if (this.m_typeBase == null)
				{
					return null;
				}
				return this.m_typeBase.EffectiveType;
			}
		}

		// Token: 0x170007BD RID: 1981
		// (get) Token: 0x06001D82 RID: 7554 RVA: 0x00051506 File Offset: 0x00050506
		public ICompiledType OriginalBase
		{
			get
			{
				return this.m_typeBase;
			}
		}

		// Token: 0x170007BE RID: 1982
		// (get) Token: 0x06001D83 RID: 7555 RVA: 0x00051510 File Offset: 0x00050510
		public override ICompiledType DeRefType
		{
			get
			{
				if (this.m_typeBase == null)
				{
					return null;
				}
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35000)
				{
					return (this.m_typeBase.DeRefType as _IType).EffectiveType.DeRefType;
				}
				return (this.m_typeBase.DeRefType as _IType).EffectiveType;
			}
		}

		// Token: 0x06001D84 RID: 7556 RVA: 0x00051568 File Offset: 0x00050568
		public override string ToString()
		{
			if (this.m_typeBase == null)
			{
				return "ERROR";
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600)
			{
				return "REFERENCE TO " + this.m_typeBase.ToString();
			}
			return this.m_typeBase.ToString();
		}

		// Token: 0x06001D85 RID: 7557 RVA: 0x000515B8 File Offset: 0x000505B8
		public override bool IsEqual(ICompiledType type, IScope scope)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3300)
			{
				ReferenceType referenceType = type as ReferenceType;
				return referenceType != null && this.m_typeBase.IsEqual(referenceType.m_typeBase, scope);
			}
			return base.IsEqual(type, scope);
		}

		// Token: 0x06001D86 RID: 7558 RVA: 0x00051600 File Offset: 0x00050600
		public override bool IsEqualPreCompile(ICompiledType type, IScope scope)
		{
			ReferenceType referenceType = type as ReferenceType;
			return referenceType != null && this.m_typeBase.IsEqualPreCompile(referenceType.m_typeBase, scope);
		}

		// Token: 0x06001D87 RID: 7559 RVA: 0x0005162B File Offset: 0x0005062B
		public override string GetConstantString(IScope scope)
		{
			if (this.m_typeBase == null)
			{
				return "ERROR";
			}
			return this.m_typeBase.GetConstantString(scope);
		}

		// Token: 0x170007BF RID: 1983
		// (get) Token: 0x06001D88 RID: 7560 RVA: 0x00051647 File Offset: 0x00050647
		public override TypeClass Class
		{
			get
			{
				return TypeClass.Reference;
			}
		}

		// Token: 0x06001D89 RID: 7561 RVA: 0x0005164B File Offset: 0x0005064B
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001D8A RID: 7562 RVA: 0x00051654 File Offset: 0x00050654
		public override _IType _Duplicate(bool bDeep)
		{
			if (this.m_typeBase == null)
			{
				return new ReferenceType(null);
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351500)
			{
				return new ReferenceType(this.m_typeBase._Duplicate(bDeep));
			}
			return new ReferenceType(this._Base._Duplicate(bDeep));
		}

		// Token: 0x170007C0 RID: 1984
		// (get) Token: 0x06001D8B RID: 7563 RVA: 0x000516A4 File Offset: 0x000506A4
		internal _IType _BaseType
		{
			get
			{
				return this.DeRefType as _IType;
			}
		}

		// Token: 0x06001D8C RID: 7564 RVA: 0x000516B1 File Offset: 0x000506B1
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return this._BaseType.CanConvertRaw(raw, byteOrder, scope);
		}

		// Token: 0x06001D8D RID: 7565 RVA: 0x000516C1 File Offset: 0x000506C1
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return this._BaseType.ConvertRaw(raw, byteOrder, scope);
		}

		// Token: 0x06001D8E RID: 7566 RVA: 0x000516D1 File Offset: 0x000506D1
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return this._BaseType.CanConvertToRaw(value, byteOrder, scope);
		}

		// Token: 0x06001D8F RID: 7567 RVA: 0x000516E1 File Offset: 0x000506E1
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return this._BaseType.ConvertToRaw(value, byteOrder, scope);
		}

		// Token: 0x170007C1 RID: 1985
		// (get) Token: 0x06001D90 RID: 7568 RVA: 0x000516F1 File Offset: 0x000506F1
		IType IReferenceType.Base
		{
			get
			{
				return this._Base;
			}
		}

		// Token: 0x040005EB RID: 1515
		[DefaultSerialization("BaseType")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		protected _IType m_typeBase;
	}
}

using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200019C RID: 412
	[TypeGuid("{F7849D2B-2BF2-4224-93B8-24A5D9713BD7}")]
	[StorageVersion("3.5.7.0")]
	public class ImplicitEnumerationType : IECType, IImplicitEnumerationType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, _IEnumType, IEnumType2, IEnumType
	{
		// Token: 0x06001DAD RID: 7597 RVA: 0x00051AD4 File Offset: 0x00050AD4
		public ImplicitEnumerationType()
		{
			this._edeclliststatement = null;
		}

		// Token: 0x06001DAE RID: 7598 RVA: 0x00051AEE File Offset: 0x00050AEE
		public ImplicitEnumerationType(_IEnumDeclarationListStatement edeclliststatement, string stImplicitDeclarationName)
		{
			this._edeclliststatement = edeclliststatement;
			this._stImplicitName = stImplicitDeclarationName;
		}

		// Token: 0x06001DAF RID: 7599 RVA: 0x00051B0F File Offset: 0x00050B0F
		public override string ToString()
		{
			return this._edeclliststatement.ToString();
		}

		// Token: 0x06001DB0 RID: 7600 RVA: 0x00051202 File Offset: 0x00050202
		public override string ToUpperString()
		{
			return this.ToString().ToUpperInvariant();
		}

		// Token: 0x06001DB1 RID: 7601 RVA: 0x00051B1C File Offset: 0x00050B1C
		public override bool IsEqual(ICompiledType type, IScope scope)
		{
			if (!base.IsEqual(type, scope))
			{
				return false;
			}
			ImplicitEnumerationType implicitEnumerationType = type as ImplicitEnumerationType;
			return implicitEnumerationType != null && this._edeclliststatement.IsEqual(implicitEnumerationType._edeclliststatement);
		}

		// Token: 0x06001DB2 RID: 7602 RVA: 0x0005054F File Offset: 0x0004F54F
		public override bool IsEqualPreCompile(ICompiledType type, IScope scope)
		{
			return this.IsEqual(type, scope);
		}

		// Token: 0x170007CA RID: 1994
		// (get) Token: 0x06001DB3 RID: 7603 RVA: 0x00051B52 File Offset: 0x00050B52
		public override TypeClass Class
		{
			get
			{
				return TypeClass.Enum;
			}
		}

		// Token: 0x170007CB RID: 1995
		// (get) Token: 0x06001DB4 RID: 7604 RVA: 0x00051B56 File Offset: 0x00050B56
		public _IEnumDeclarationListStatement Enumerations
		{
			get
			{
				return this._edeclliststatement;
			}
		}

		// Token: 0x06001DB5 RID: 7605 RVA: 0x00051B5E File Offset: 0x00050B5E
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001DB6 RID: 7606 RVA: 0x00051B67 File Offset: 0x00050B67
		public override _IType _Duplicate(bool bDeep)
		{
			return new ImplicitEnumerationType(this._edeclliststatement, this._stImplicitName);
		}

		// Token: 0x170007CC RID: 1996
		// (get) Token: 0x06001DB7 RID: 7607 RVA: 0x0004F273 File Offset: 0x0004E273
		public override ICompiledType BaseType
		{
			get
			{
				return TypeTable.Int;
			}
		}

		// Token: 0x170007CD RID: 1997
		// (get) Token: 0x06001DB8 RID: 7608 RVA: 0x0004F273 File Offset: 0x0004E273
		public override ICompiledType DeRefType
		{
			get
			{
				return TypeTable.Int;
			}
		}

		// Token: 0x06001DB9 RID: 7609 RVA: 0x00051B7A File Offset: 0x00050B7A
		public override int SizeChecked(IScope scope, out bool bValid)
		{
			return (TypeTable.Int.EffectiveType as _IType).SizeChecked(scope, out bValid);
		}

		// Token: 0x06001DBA RID: 7610 RVA: 0x00051B92 File Offset: 0x00050B92
		public override int Size(IScope scope)
		{
			return TypeTable.Int.EffectiveType.Size(scope);
		}

		// Token: 0x06001DBB RID: 7611 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001DBC RID: 7612 RVA: 0x00051BA4 File Offset: 0x00050BA4
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			throw new NotImplementedException("ImplicitEnumerationType.ConvertRaw not implemented yet");
		}

		// Token: 0x06001DBD RID: 7613 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001DBE RID: 7614 RVA: 0x00051BB0 File Offset: 0x00050BB0
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			throw new NotImplementedException("ImplicitEnumerationType.ConvertToRaw not implemented yet");
		}

		// Token: 0x170007CE RID: 1998
		// (get) Token: 0x06001DBF RID: 7615 RVA: 0x0000E5F6 File Offset: 0x0000D5F6
		// (set) Token: 0x06001DC0 RID: 7616 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public int SignatureId
		{
			get
			{
				return Common.InvalidID;
			}
			set
			{
			}
		}

		// Token: 0x06001DC1 RID: 7617 RVA: 0x00005F0F File Offset: 0x00004F0F
		public ISignature GetSignature(IScope scope)
		{
			return null;
		}

		// Token: 0x170007CF RID: 1999
		// (get) Token: 0x06001DC2 RID: 7618 RVA: 0x0004F273 File Offset: 0x0004E273
		// (set) Token: 0x06001DC3 RID: 7619 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public _IType _Base
		{
			get
			{
				return TypeTable.Int;
			}
			set
			{
			}
		}

		// Token: 0x170007D0 RID: 2000
		// (get) Token: 0x06001DC4 RID: 7620 RVA: 0x00051BBC File Offset: 0x00050BBC
		// (set) Token: 0x06001DC5 RID: 7621 RVA: 0x00051BC4 File Offset: 0x00050BC4
		public string Name
		{
			get
			{
				return this._stImplicitName;
			}
			set
			{
				this._stImplicitName = value;
			}
		}

		// Token: 0x170007D1 RID: 2001
		// (get) Token: 0x06001DC6 RID: 7622 RVA: 0x00005F0F File Offset: 0x00004F0F
		public IExpression DefaultValue
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170007D2 RID: 2002
		// (get) Token: 0x06001DC7 RID: 7623 RVA: 0x00005F0F File Offset: 0x00004F0F
		// (set) Token: 0x06001DC8 RID: 7624 RVA: 0x0000677E File Offset: 0x0000577E
		public _IVariableExpression _DefaultValue
		{
			get
			{
				return null;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x040005EF RID: 1519
		private readonly _IEnumDeclarationListStatement _edeclliststatement;

		// Token: 0x040005F0 RID: 1520
		private string _stImplicitName = string.Empty;
	}
}

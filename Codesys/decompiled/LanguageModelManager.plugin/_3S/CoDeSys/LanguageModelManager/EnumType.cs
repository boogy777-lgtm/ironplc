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
	// Token: 0x0200019D RID: 413
	[TypeGuid("{5677bd2d-c2f2-4498-a6d6-5e2aecdbd55d}")]
	[StorageVersion("3.3.0.0")]
	public class EnumType : IECType, _IEnumType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, IEnumType2, IEnumType, IEnumTypeSerializable
	{
		// Token: 0x06001DC9 RID: 7625 RVA: 0x00051BCD File Offset: 0x00050BCD
		public EnumType()
		{
		}

		// Token: 0x06001DCA RID: 7626 RVA: 0x00051BF6 File Offset: 0x00050BF6
		public EnumType(string stName)
		{
			this.m_stName = stName;
		}

		// Token: 0x06001DCB RID: 7627 RVA: 0x00051C26 File Offset: 0x00050C26
		public EnumType(string stName, int idSignature)
		{
			this.m_stName = stName;
			this.m_idSignature = idSignature;
		}

		// Token: 0x06001DCC RID: 7628 RVA: 0x00051C5D File Offset: 0x00050C5D
		public EnumType(string stName, int idSignature, _IType typeBase)
		{
			this.m_stName = stName;
			this.m_idSignature = idSignature;
			this.m_typeBase = typeBase;
		}

		// Token: 0x170007D3 RID: 2003
		// (get) Token: 0x06001DCD RID: 7629 RVA: 0x00051C9B File Offset: 0x00050C9B
		// (set) Token: 0x06001DCE RID: 7630 RVA: 0x00051CA3 File Offset: 0x00050CA3
		public string Name
		{
			get
			{
				return this.m_stName;
			}
			set
			{
				this.m_stName = value;
			}
		}

		// Token: 0x170007D4 RID: 2004
		// (get) Token: 0x06001DCF RID: 7631 RVA: 0x00051CAC File Offset: 0x00050CAC
		// (set) Token: 0x06001DD0 RID: 7632 RVA: 0x00051CB4 File Offset: 0x00050CB4
		public int SignatureId
		{
			get
			{
				return this.m_idSignature;
			}
			set
			{
				this.m_idSignature = value;
			}
		}

		// Token: 0x06001DD1 RID: 7633 RVA: 0x00051CBD File Offset: 0x00050CBD
		public ISignature GetSignature(IScope scope)
		{
			return scope[this.m_idSignature];
		}

		// Token: 0x06001DD2 RID: 7634 RVA: 0x00051C9B File Offset: 0x00050C9B
		public override string ToString()
		{
			return this.m_stName;
		}

		// Token: 0x06001DD3 RID: 7635 RVA: 0x00051202 File Offset: 0x00050202
		public override string ToUpperString()
		{
			return this.ToString().ToUpperInvariant();
		}

		// Token: 0x06001DD4 RID: 7636 RVA: 0x00051CCB File Offset: 0x00050CCB
		public override string GetConstantString(IScope scope)
		{
			return scope[this.SignatureId].Name;
		}

		// Token: 0x06001DD5 RID: 7637 RVA: 0x00051CE0 File Offset: 0x00050CE0
		public override bool IsEqual(ICompiledType type, IScope scope)
		{
			if (!base.IsEqual(type, scope))
			{
				return false;
			}
			EnumType enumType = type as EnumType;
			return enumType != null && (this.m_idSignature == Common.InvalidID || enumType.m_idSignature == Common.InvalidID || enumType.m_idSignature == this.m_idSignature);
		}

		// Token: 0x170007D5 RID: 2005
		// (get) Token: 0x06001DD6 RID: 7638 RVA: 0x00051B52 File Offset: 0x00050B52
		public override TypeClass Class
		{
			get
			{
				return TypeClass.Enum;
			}
		}

		// Token: 0x06001DD7 RID: 7639 RVA: 0x00051D2F File Offset: 0x00050D2F
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001DD8 RID: 7640 RVA: 0x00051D38 File Offset: 0x00050D38
		public override _IType _Duplicate(bool bDeep)
		{
			EnumType enumType = new EnumType(this.m_stName, this.m_idSignature, this.m_typeBase);
			enumType.SignatureId = Common.InvalidID;
			enumType._DefaultValue = this.m_defaultValue;
			if (bDeep && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351700)
			{
				enumType.SignatureId = this.SignatureId;
			}
			else
			{
				enumType.SignatureId = Common.InvalidID;
			}
			return enumType;
		}

		// Token: 0x06001DD9 RID: 7641 RVA: 0x00051DA4 File Offset: 0x00050DA4
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			try
			{
				this.ConvertRaw(raw, byteOrder, scope);
			}
			catch
			{
				return false;
			}
			return true;
		}

		// Token: 0x06001DDA RID: 7642 RVA: 0x00051DD8 File Offset: 0x00050DD8
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			object obj = this.m_typeBase.ConvertRaw(raw, byteOrder, scope);
			IntegerUnion integerUnion = default(IntegerUnion);
			integerUnion.m_long = 0L;
			if (this.m_typeBase.Class != TypeClass.Bool)
			{
				integerUnion.m_long = TypeHelper.GetNumericValue(obj, scope);
			}
			ISignature signature = scope[this.m_idSignature];
			if (signature == null)
			{
				return obj;
			}
			if (this.m_typeBase.Class == TypeClass.Bool)
			{
				return signature.All[(int)raw[0]].Name;
			}
			string text = null;
			foreach (IVariable variable in signature.All)
			{
				if (variable.Type.Class == TypeClass.Enum)
				{
					_IExpression iexpression = variable.Initial as _IExpression;
					if (iexpression != null && iexpression.Literal(scope, true) != null)
					{
						ILiteralValue literalValue = iexpression.Literal(scope, true);
						if ((literalValue.KindOf == KindOfLiteral.SignedInteger && integerUnion.m_long == literalValue.SignedLong) || (literalValue.KindOf == KindOfLiteral.UnsignedInteger && integerUnion.m_ulong == literalValue.UnsignedLong))
						{
							text = signature.OrgName + "." + variable.OrgName;
							break;
						}
					}
				}
			}
			if (text != null)
			{
				return text;
			}
			return obj;
		}

		// Token: 0x06001DDB RID: 7643 RVA: 0x00051F04 File Offset: 0x00050F04
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			bool result;
			try
			{
				this.ConvertToRaw(value, byteOrder, scope);
				result = true;
			}
			catch (Exception)
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06001DDC RID: 7644 RVA: 0x00051F38 File Offset: 0x00050F38
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			IntegerUnion integerUnion = default(IntegerUnion);
			integerUnion.m_long = 0L;
			bool flag = false;
			bool flag2 = false;
			string text = value as string;
			try
			{
				integerUnion.m_long = this.GetNumericEnumValue(value, scope);
				return this.m_typeBase.ConvertToRaw(integerUnion.m_long, byteOrder, scope);
			}
			catch
			{
				flag2 = true;
			}
			if (flag2 && text != null)
			{
				ISignature signature = scope[this.m_idSignature];
				int num = text.LastIndexOf('.');
				if (num >= 0)
				{
					text = text.Substring(num + 1);
				}
				text = text.ToUpperInvariant();
				IVariable[] all = signature.All;
				int i = 0;
				while (i < all.Length)
				{
					IVariable variable = all[i];
					flag = (variable.Name == text);
					if (flag)
					{
						_IExpression2 iexpression = variable.Initial as _IExpression2;
						if (iexpression == null)
						{
							break;
						}
						IRecursionGuard recursionGuard = new RecursionGuard();
						bool flag3;
						ILiteralValue literalValue = iexpression.LiteralWithRecursionCheck(scope, recursionGuard, true, out flag3);
						if (literalValue == null)
						{
							break;
						}
						if (literalValue.KindOf == KindOfLiteral.SignedInteger)
						{
							integerUnion.m_long = literalValue.SignedLong;
							break;
						}
						if (literalValue.KindOf == KindOfLiteral.UnsignedInteger)
						{
							integerUnion.m_ulong = literalValue.UnsignedLong;
							break;
						}
						throw new ArgumentException(string.Format("{0} is no valid enum value", value), "value");
					}
					else
					{
						i++;
					}
				}
			}
			if (flag)
			{
				return this.m_typeBase.ConvertToRaw(integerUnion.m_ulong, byteOrder, scope);
			}
			throw new ArgumentException(string.Format("{0} is no valid enum value", value), "value");
		}

		// Token: 0x06001DDD RID: 7645 RVA: 0x000520C0 File Offset: 0x000510C0
		private long GetNumericEnumValue(object o, IScope5 scope)
		{
			if (o is string && scope != null)
			{
				try
				{
					string text = o as string;
					foreach (IVariable variable in (scope.FindSignature(this) as _ISignature).AllVariables)
					{
						if (variable.OrgName.ToLowerInvariant() == text.ToLowerInvariant() && (variable.HasFlag(VarFlag.Constant) || variable.HasFlag(VarFlag.ReplacedConstant)))
						{
							ILiteralValue literalValue = variable.Initial.Literal(scope);
							if (literalValue.KindOf == KindOfLiteral.SignedInteger)
							{
								return literalValue.SignedLong;
							}
							if (literalValue.KindOf == KindOfLiteral.UnsignedInteger)
							{
								return (long)literalValue.UnsignedLong;
							}
						}
					}
				}
				catch
				{
				}
			}
			return TypeHelper.GetNumericValue(o, scope);
		}

		// Token: 0x170007D6 RID: 2006
		// (get) Token: 0x06001DDE RID: 7646 RVA: 0x000521A4 File Offset: 0x000511A4
		// (set) Token: 0x06001DDF RID: 7647 RVA: 0x000521C0 File Offset: 0x000511C0
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

		// Token: 0x170007D7 RID: 2007
		// (get) Token: 0x06001DE0 RID: 7648 RVA: 0x000521C9 File Offset: 0x000511C9
		// (set) Token: 0x06001DE1 RID: 7649 RVA: 0x000521D1 File Offset: 0x000511D1
		public _IVariableExpression _DefaultValue
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

		// Token: 0x170007D8 RID: 2008
		// (get) Token: 0x06001DE2 RID: 7650 RVA: 0x000521DA File Offset: 0x000511DA
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

		// Token: 0x170007D9 RID: 2009
		// (get) Token: 0x06001DE3 RID: 7651 RVA: 0x000521DA File Offset: 0x000511DA
		public override ICompiledType DeRefType
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

		// Token: 0x06001DE4 RID: 7652 RVA: 0x000521F1 File Offset: 0x000511F1
		public override int SizeChecked(IScope scope, out bool bValid)
		{
			return (this.m_typeBase.EffectiveType as _IType).SizeChecked(scope, out bValid);
		}

		// Token: 0x06001DE5 RID: 7653 RVA: 0x0005220A File Offset: 0x0005120A
		public override int Size(IScope scope)
		{
			return this.m_typeBase.EffectiveType.Size(scope);
		}

		// Token: 0x06001DE6 RID: 7654 RVA: 0x0005221D File Offset: 0x0005121D
		public override void BeforeSerialize()
		{
			if (this._DefaultValue != null)
			{
				this._DefaultValue.Type = null;
			}
			base.BeforeSerialize();
		}

		// Token: 0x06001DE7 RID: 7655 RVA: 0x00052239 File Offset: 0x00051239
		public override void AfterDeserialize()
		{
			base.AfterDeserialize();
			if (this._DefaultValue != null)
			{
				this._DefaultValue.Type = this;
			}
		}

		// Token: 0x040005F1 RID: 1521
		[DefaultSerialization("SignatureId")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		[DefaultDuplication(DuplicationMethod.Shallow)]
		private int m_idSignature = Common.InvalidID;

		// Token: 0x040005F2 RID: 1522
		[DefaultSerialization("Name")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		[DefaultDuplication(DuplicationMethod.Shallow)]
		private string m_stName = string.Empty;

		// Token: 0x040005F3 RID: 1523
		[DefaultSerialization("BaseType")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		[DefaultDuplication(DuplicationMethod.Deep)]
		private _IType m_typeBase = TypeTable.Int;

		// Token: 0x040005F4 RID: 1524
		[DefaultSerialization("DefaultValue")]
		[StorageVersion("3.5.7.0")]
		[StorageDefaultValue(null)]
		[Obfuscation(Feature = "rename")]
		[DefaultDuplication(DuplicationMethod.Deep)]
		private _IVariableExpression m_defaultValue;
	}
}

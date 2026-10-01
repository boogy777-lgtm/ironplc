using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200019B RID: 411
	[TypeGuid("{abaeea6e-0a30-48c7-9034-51e0ad478bc3}")]
	[StorageVersion("3.3.0.0")]
	public class SubrangeType : IECType, _ISubrangeType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, ISubrangeType2, ISubrangeType
	{
		// Token: 0x06001D95 RID: 7573 RVA: 0x0004EC38 File Offset: 0x0004DC38
		public SubrangeType()
		{
		}

		// Token: 0x06001D96 RID: 7574 RVA: 0x0005170A File Offset: 0x0005070A
		internal SubrangeType(_IExpression expLower, _IExpression expUpper)
		{
			this.m_expLowerBorder = expLower;
			this.m_expUpperBorder = expUpper;
		}

		// Token: 0x170007C2 RID: 1986
		// (get) Token: 0x06001D97 RID: 7575 RVA: 0x00051720 File Offset: 0x00050720
		public IExpression LowerBorder
		{
			get
			{
				return this._LowerBorder;
			}
		}

		// Token: 0x170007C3 RID: 1987
		// (get) Token: 0x06001D98 RID: 7576 RVA: 0x00051728 File Offset: 0x00050728
		public IExpression UpperBorder
		{
			get
			{
				return this._UpperBorder;
			}
		}

		// Token: 0x170007C4 RID: 1988
		// (get) Token: 0x06001D99 RID: 7577 RVA: 0x00051730 File Offset: 0x00050730
		// (set) Token: 0x06001D9A RID: 7578 RVA: 0x00051738 File Offset: 0x00050738
		public _IExpression _LowerBorder
		{
			get
			{
				return this.m_expLowerBorder;
			}
			set
			{
				this.m_expLowerBorder = value;
			}
		}

		// Token: 0x170007C5 RID: 1989
		// (get) Token: 0x06001D9B RID: 7579 RVA: 0x00051741 File Offset: 0x00050741
		// (set) Token: 0x06001D9C RID: 7580 RVA: 0x00051749 File Offset: 0x00050749
		public _IExpression _UpperBorder
		{
			get
			{
				return this.m_expUpperBorder;
			}
			set
			{
				this.m_expUpperBorder = value;
			}
		}

		// Token: 0x170007C6 RID: 1990
		// (get) Token: 0x06001D9D RID: 7581 RVA: 0x00051752 File Offset: 0x00050752
		// (set) Token: 0x06001D9E RID: 7582 RVA: 0x0005176E File Offset: 0x0005076E
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

		// Token: 0x170007C7 RID: 1991
		// (get) Token: 0x06001D9F RID: 7583 RVA: 0x00051777 File Offset: 0x00050777
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

		// Token: 0x170007C8 RID: 1992
		// (get) Token: 0x06001DA0 RID: 7584 RVA: 0x00051777 File Offset: 0x00050777
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

		// Token: 0x06001DA1 RID: 7585 RVA: 0x00051790 File Offset: 0x00050790
		public override string ToString()
		{
			if (this.m_typeBase == null || this.m_expLowerBorder == null || this.m_expUpperBorder == null)
			{
				return "ERROR";
			}
			string text = string.Empty;
			text = this.m_typeBase.ToString();
			string[] array = new string[6];
			array[0] = text;
			array[1] = " (";
			int num = 2;
			_IExpression expLowerBorder = this.m_expLowerBorder;
			array[num] = ((expLowerBorder != null) ? expLowerBorder.ToString() : null);
			array[3] = "..";
			int num2 = 4;
			_IExpression expUpperBorder = this.m_expUpperBorder;
			array[num2] = ((expUpperBorder != null) ? expUpperBorder.ToString() : null);
			array[5] = ")";
			return string.Concat(array);
		}

		// Token: 0x06001DA2 RID: 7586 RVA: 0x00051820 File Offset: 0x00050820
		public override string GetConstantString(IScope scope)
		{
			if (this.m_typeBase == null || this.m_expLowerBorder == null || this.m_expUpperBorder == null)
			{
				return "ERROR";
			}
			string constantString = this.m_typeBase.GetConstantString(scope);
			bool flag;
			int @int = TypeHelper.GetInt(this.m_expLowerBorder, scope as IScope5, out flag);
			bool flag2;
			int int2 = TypeHelper.GetInt(this.m_expUpperBorder, scope as IScope5, out flag2);
			if (!flag || !flag2)
			{
				return constantString;
			}
			return string.Concat(new string[]
			{
				constantString,
				" (",
				@int.ToString(),
				"..",
				int2.ToString(),
				")"
			});
		}

		// Token: 0x170007C9 RID: 1993
		// (get) Token: 0x06001DA3 RID: 7587 RVA: 0x000518C6 File Offset: 0x000508C6
		public override TypeClass Class
		{
			get
			{
				return TypeClass.Subrange;
			}
		}

		// Token: 0x06001DA4 RID: 7588 RVA: 0x000518CA File Offset: 0x000508CA
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001DA5 RID: 7589 RVA: 0x000518D4 File Offset: 0x000508D4
		public override _IType _Duplicate(bool bDeep)
		{
			SubrangeType subrangeType = new SubrangeType();
			if (this.m_expLowerBorder != null)
			{
				subrangeType.m_expLowerBorder = (this.m_expLowerBorder.Duplicate() as _IExpression);
			}
			if (this.m_expUpperBorder != null)
			{
				subrangeType.m_expUpperBorder = (this.m_expUpperBorder.Duplicate() as _IExpression);
			}
			if (this.m_typeBase != null)
			{
				subrangeType.m_typeBase = this.m_typeBase._Duplicate(bDeep);
			}
			return subrangeType;
		}

		// Token: 0x06001DA6 RID: 7590 RVA: 0x0005193E File Offset: 0x0005093E
		public override int SizeChecked(IScope scope, out bool bValid)
		{
			return (this.m_typeBase.EffectiveType as _IType).SizeChecked(scope, out bValid);
		}

		// Token: 0x06001DA7 RID: 7591 RVA: 0x00051957 File Offset: 0x00050957
		public override int Size(IScope scope)
		{
			return this.m_typeBase.EffectiveType.Size(scope);
		}

		// Token: 0x06001DA8 RID: 7592 RVA: 0x0005196A File Offset: 0x0005096A
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return this.m_typeBase.CanConvertRaw(raw, byteOrder, scope);
		}

		// Token: 0x06001DA9 RID: 7593 RVA: 0x0005197A File Offset: 0x0005097A
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return this.m_typeBase.ConvertRaw(raw, byteOrder, scope);
		}

		// Token: 0x06001DAA RID: 7594 RVA: 0x0005198A File Offset: 0x0005098A
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return this.m_typeBase.CanConvertToRaw(value, byteOrder, scope);
		}

		// Token: 0x06001DAB RID: 7595 RVA: 0x0005199A File Offset: 0x0005099A
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return this.m_typeBase.ConvertToRaw(value, byteOrder, scope);
		}

		// Token: 0x06001DAC RID: 7596 RVA: 0x000519AC File Offset: 0x000509AC
		public override bool IsEqualPreCompile(ICompiledType type, IScope scope)
		{
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV352200)
			{
				return base.IsEqualPreCompile(type, scope);
			}
			SubrangeType subrangeType = type as SubrangeType;
			if (subrangeType == null)
			{
				return false;
			}
			if (!this.m_typeBase.IsEqualPreCompile(subrangeType.m_typeBase, scope))
			{
				return false;
			}
			if (scope == null)
			{
				string text = this.m_expLowerBorder.ToString();
				string text2 = subrangeType.m_expLowerBorder.ToString();
				string text3 = this.m_expUpperBorder.ToString();
				string text4 = subrangeType.m_expUpperBorder.ToString();
				return text.ToUpperInvariant() == text2.ToUpperInvariant() && text3.ToUpperInvariant() == text4.ToUpperInvariant();
			}
			ILiteralValue literalValue = this.m_expLowerBorder.Literal(scope, true);
			ILiteralValue literalValue2 = subrangeType.m_expLowerBorder.Literal(scope, true);
			ILiteralValue literalValue3 = this.m_expUpperBorder.Literal(scope, true);
			ILiteralValue literalValue4 = subrangeType.m_expUpperBorder.Literal(scope, true);
			int num;
			int num2;
			int num3;
			int num4;
			return literalValue != null && literalValue2 != null && literalValue3 != null && literalValue4 != null && literalValue.GetInt(out num) && literalValue3.GetInt(out num2) && literalValue2.GetInt(out num3) && literalValue4.GetInt(out num4) && num == num3 && num2 == num4;
		}

		// Token: 0x040005EC RID: 1516
		[DefaultSerialization("LowerBorder")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expLowerBorder;

		// Token: 0x040005ED RID: 1517
		[DefaultSerialization("UpperBorder")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expUpperBorder;

		// Token: 0x040005EE RID: 1518
		[DefaultSerialization("BaseType")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IType m_typeBase;
	}
}

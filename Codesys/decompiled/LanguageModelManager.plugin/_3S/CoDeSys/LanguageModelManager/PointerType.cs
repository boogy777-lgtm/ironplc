using System;
using System.Diagnostics;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000197 RID: 407
	[TypeGuid("{e833a88e-e01b-4cc9-b0cb-fe17b4370a01}")]
	[StorageVersion("3.3.0.0")]
	public class PointerType : IECType, _IPointerType2, _IPointerType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, IPointerType
	{
		// Token: 0x06001D67 RID: 7527 RVA: 0x0004EC38 File Offset: 0x0004DC38
		public PointerType()
		{
		}

		// Token: 0x06001D68 RID: 7528 RVA: 0x00051183 File Offset: 0x00050183
		public PointerType(_IType typeBase)
		{
			this.m_typeBase = typeBase;
		}

		// Token: 0x170007B6 RID: 1974
		// (get) Token: 0x06001D69 RID: 7529 RVA: 0x00051192 File Offset: 0x00050192
		public IType Base
		{
			get
			{
				if (this.m_typeBase != null)
				{
					return this.m_typeBase.EffectiveType;
				}
				return null;
			}
		}

		// Token: 0x170007B7 RID: 1975
		// (get) Token: 0x06001D6A RID: 7530 RVA: 0x000511A9 File Offset: 0x000501A9
		// (set) Token: 0x06001D6B RID: 7531 RVA: 0x000511C5 File Offset: 0x000501C5
		public _IType _Base
		{
			get
			{
				if (this.m_typeBase != null)
				{
					return this.m_typeBase.EffectiveType as _IType;
				}
				return null;
			}
			set
			{
				this.m_typeBase = value;
			}
		}

		// Token: 0x170007B8 RID: 1976
		// (get) Token: 0x06001D6C RID: 7532 RVA: 0x000511A9 File Offset: 0x000501A9
		public override ICompiledType BaseType
		{
			get
			{
				if (this.m_typeBase != null)
				{
					return this.m_typeBase.EffectiveType as _IType;
				}
				return null;
			}
		}

		// Token: 0x170007B9 RID: 1977
		// (get) Token: 0x06001D6D RID: 7533 RVA: 0x000511CE File Offset: 0x000501CE
		public _IType OriginalBase
		{
			get
			{
				return this.m_typeBase;
			}
		}

		// Token: 0x06001D6E RID: 7534 RVA: 0x000511D6 File Offset: 0x000501D6
		public override string ToString()
		{
			if (this.m_typeBase == null)
			{
				return "POINTER TO NULL";
			}
			string str = "POINTER TO ";
			_IType typeBase = this.m_typeBase;
			return str + ((typeBase != null) ? typeBase.ToString() : null);
		}

		// Token: 0x06001D6F RID: 7535 RVA: 0x00051202 File Offset: 0x00050202
		public override string ToUpperString()
		{
			return this.ToString().ToUpperInvariant();
		}

		// Token: 0x06001D70 RID: 7536 RVA: 0x00051210 File Offset: 0x00050210
		public override bool IsEqual(ICompiledType type, IScope scope)
		{
			PointerType pointerType = type as PointerType;
			return pointerType != null && this.m_typeBase != null && this.m_typeBase.IsEqual(pointerType.m_typeBase, scope);
		}

		// Token: 0x06001D71 RID: 7537 RVA: 0x00051244 File Offset: 0x00050244
		public override bool IsEqualPreCompile(ICompiledType type, IScope scope)
		{
			PointerType pointerType = type as PointerType;
			return pointerType != null && this.m_typeBase.IsEqualPreCompile(pointerType.m_typeBase, scope);
		}

		// Token: 0x06001D72 RID: 7538 RVA: 0x0005126F File Offset: 0x0005026F
		public override string GetConstantString(IScope scope)
		{
			if (this.m_typeBase == null)
			{
				return "POINTER TO NULL";
			}
			return "POINTER TO " + this.m_typeBase.GetConstantString(scope);
		}

		// Token: 0x170007BA RID: 1978
		// (get) Token: 0x06001D73 RID: 7539 RVA: 0x00051295 File Offset: 0x00050295
		public override TypeClass Class
		{
			get
			{
				return TypeClass.Pointer;
			}
		}

		// Token: 0x06001D74 RID: 7540 RVA: 0x00051299 File Offset: 0x00050299
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001D75 RID: 7541 RVA: 0x000512A2 File Offset: 0x000502A2
		public override _IType _Duplicate(bool bDeep)
		{
			if (this.m_typeBase == null)
			{
				return new PointerType(null);
			}
			return new PointerType(this._Base._Duplicate(bDeep));
		}

		// Token: 0x06001D76 RID: 7542 RVA: 0x000512C4 File Offset: 0x000502C4
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			bool result;
			try
			{
				this.ConvertRaw(raw, byteOrder, scope);
				result = true;
			}
			catch (Exception)
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06001D77 RID: 7543 RVA: 0x000512F8 File Offset: 0x000502F8
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			object result = 0;
			if (raw.Length == 2)
			{
				ushort num = 0;
				for (int i = 0; i < 2; i++)
				{
					ushort num2 = (ushort)(raw[i] << 8 * i);
					num |= num2;
				}
				if (byteOrder == ByteOrder.Motorola)
				{
					BitHelper.Swap(ref num);
				}
				result = num;
			}
			else if (raw.Length == 4)
			{
				uint num3 = 0U;
				for (int j = 0; j < 4; j++)
				{
					uint num4 = (uint)((uint)raw[j] << 8 * j);
					num3 |= num4;
				}
				if (byteOrder == ByteOrder.Motorola)
				{
					BitHelper.Swap(ref num3);
				}
				result = num3;
			}
			else
			{
				if (raw.Length != 8)
				{
					throw new InvalidCastException("Invalid length for a pointer value");
				}
				ulong num5 = 0UL;
				for (int k = 0; k < 8; k++)
				{
					ulong num6 = (ulong)raw[k] << 8 * k;
					num5 |= num6;
				}
				if (byteOrder == ByteOrder.Motorola)
				{
					BitHelper.Swap(ref num5);
				}
				result = num5;
			}
			return result;
		}

		// Token: 0x06001D78 RID: 7544 RVA: 0x000513DC File Offset: 0x000503DC
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

		// Token: 0x06001D79 RID: 7545 RVA: 0x00051410 File Offset: 0x00050410
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			ulong unsignedNumericValue = TypeHelper.GetUnsignedNumericValue(value, scope);
			int num = this.Size(scope);
			checked
			{
				byte[] bytes;
				if (num != 2)
				{
					if (num != 4)
					{
						if (num != 8)
						{
							Debug.Fail("Invalid pointer size");
							throw new Exception("Invalid pointer size");
						}
						bytes = BitConverter.GetBytes(unsignedNumericValue);
					}
					else
					{
						bytes = BitConverter.GetBytes((uint)unsignedNumericValue);
					}
				}
				else
				{
					bytes = BitConverter.GetBytes((ushort)unsignedNumericValue);
				}
				Debug.Assert(bytes != null);
				Debug.Assert(bytes.Length == this.Size(scope));
				if (byteOrder == ByteOrder.Motorola)
				{
					Array.Reverse(bytes);
				}
				return bytes;
			}
		}

		// Token: 0x06001D7A RID: 7546 RVA: 0x00005E58 File Offset: 0x00004E58
		public override int GetNumOfElements(IScope5 scope)
		{
			return 1;
		}

		// Token: 0x06001D7B RID: 7547 RVA: 0x00051492 File Offset: 0x00050492
		public override ICompiledType GetComponent(int i, IScope5 scope)
		{
			if (i != 0)
			{
				throw new ArgumentOutOfRangeException("i");
			}
			return this.BaseType;
		}

		// Token: 0x06001D7C RID: 7548 RVA: 0x000514A8 File Offset: 0x000504A8
		public override string[] GetComponents(IScope5 scope, out bool bValid)
		{
			bValid = true;
			return new string[]
			{
				"^"
			};
		}

		// Token: 0x040005EA RID: 1514
		[DefaultSerialization("BaseType")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		[DefaultDuplication(DuplicationMethod.Deep)]
		private _IType m_typeBase;
	}
}

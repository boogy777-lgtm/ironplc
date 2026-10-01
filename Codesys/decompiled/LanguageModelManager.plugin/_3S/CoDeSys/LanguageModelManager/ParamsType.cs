using System;
using System.Reflection;
using System.Text;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020001A1 RID: 417
	[TypeGuid("{199053C3-8DB6-47a9-B8D9-A7BB1F28A940}")]
	[StorageVersion("3.3.0.0")]
	public class ParamsType : IECType, _IParamsType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001E3D RID: 7741 RVA: 0x0004EC38 File Offset: 0x0004DC38
		public ParamsType()
		{
		}

		// Token: 0x06001E3E RID: 7742 RVA: 0x0005385D File Offset: 0x0005285D
		public ParamsType(_IType typeBase, _IExpression count)
		{
			this.m_typeBase = typeBase;
			this._expCount = count;
		}

		// Token: 0x170007ED RID: 2029
		// (get) Token: 0x06001E3F RID: 7743 RVA: 0x00053873 File Offset: 0x00052873
		public IType Base
		{
			get
			{
				return this.m_typeBase;
			}
		}

		// Token: 0x170007EE RID: 2030
		// (get) Token: 0x06001E40 RID: 7744 RVA: 0x00053873 File Offset: 0x00052873
		// (set) Token: 0x06001E41 RID: 7745 RVA: 0x0005387B File Offset: 0x0005287B
		public _IType _Base
		{
			get
			{
				return this.m_typeBase;
			}
			set
			{
				this.m_typeBase = value;
			}
		}

		// Token: 0x170007EF RID: 2031
		// (get) Token: 0x06001E42 RID: 7746 RVA: 0x00053884 File Offset: 0x00052884
		// (set) Token: 0x06001E43 RID: 7747 RVA: 0x0005388C File Offset: 0x0005288C
		public _IExpression Count
		{
			get
			{
				return this._expCount;
			}
			set
			{
				this._expCount = value;
			}
		}

		// Token: 0x170007F0 RID: 2032
		// (get) Token: 0x06001E44 RID: 7748 RVA: 0x00053873 File Offset: 0x00052873
		public override ICompiledType BaseType
		{
			get
			{
				return this.m_typeBase;
			}
		}

		// Token: 0x06001E45 RID: 7749 RVA: 0x00053895 File Offset: 0x00052895
		public override string ToString()
		{
			if (this.m_typeBase == null)
			{
				return "ERROR";
			}
			string str = "PARAMS OF ";
			_IType typeBase = this.m_typeBase;
			return str + ((typeBase != null) ? typeBase.ToString() : null);
		}

		// Token: 0x06001E46 RID: 7750 RVA: 0x000538C1 File Offset: 0x000528C1
		public override string GetConstantString(IScope scope)
		{
			if (this.m_typeBase == null)
			{
				return "ERROR";
			}
			return "PARAMS OF " + this.m_typeBase.GetConstantString(scope);
		}

		// Token: 0x06001E47 RID: 7751 RVA: 0x00051202 File Offset: 0x00050202
		public override string ToUpperString()
		{
			return this.ToString().ToUpperInvariant();
		}

		// Token: 0x170007F1 RID: 2033
		// (get) Token: 0x06001E48 RID: 7752 RVA: 0x000538E7 File Offset: 0x000528E7
		public override TypeClass Class
		{
			get
			{
				return TypeClass.Params;
			}
		}

		// Token: 0x06001E49 RID: 7753 RVA: 0x000538EB File Offset: 0x000528EB
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001E4A RID: 7754 RVA: 0x000538F4 File Offset: 0x000528F4
		public override _IType _Duplicate(bool bDeep)
		{
			return new ParamsType
			{
				_Base = this.m_typeBase._Duplicate(bDeep)
			};
		}

		// Token: 0x06001E4B RID: 7755 RVA: 0x0005390D File Offset: 0x0005290D
		public override int SizeChecked(IScope scope, out bool bValid)
		{
			return this.m_typeBase.SizeChecked(scope, out bValid);
		}

		// Token: 0x06001E4C RID: 7756 RVA: 0x0005391C File Offset: 0x0005291C
		public override int Size(IScope scope)
		{
			return this.m_typeBase.Size(scope);
		}

		// Token: 0x06001E4D RID: 7757 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001E4E RID: 7758 RVA: 0x0005392A File Offset: 0x0005292A
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			throw new NotImplementedException("ArrayType.ConvertRaw not implemented yet");
		}

		// Token: 0x06001E4F RID: 7759 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001E50 RID: 7760 RVA: 0x00053936 File Offset: 0x00052936
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			throw new NotImplementedException("ArrayType.ConvertToRaw not implemented yet");
		}

		// Token: 0x170007F2 RID: 2034
		// (get) Token: 0x06001E51 RID: 7761 RVA: 0x00053944 File Offset: 0x00052944
		public string ImplicitStructName
		{
			get
			{
				if (this.BaseType == null)
				{
					return string.Empty;
				}
				string text = this.BaseType.ToString();
				if (this.BaseType.Class == TypeClass.Array)
				{
					text = text.Replace(' ', '_');
					text = text.Replace(']', '_');
					text = text.Replace('[', '_');
					text = text.Replace('.', '_');
				}
				return string.Format("__PARAMS__{0}__{1}", text, this.Count.ToString());
			}
		}

		// Token: 0x170007F3 RID: 2035
		// (get) Token: 0x06001E52 RID: 7762 RVA: 0x000539BC File Offset: 0x000529BC
		public string ImplicitStructDefinition
		{
			get
			{
				if (this.BaseType == null)
				{
					return string.Empty;
				}
				return new StringBuilder().Append("{p0}").Append("{implicit on}").AppendLine().Append("{attribute 'friend_instance' := 'addextensible'}").AppendLine().Append("{p1}").Append("TYPE " + this.ImplicitStructName + " :").AppendLine().Append("{p2}").Append("STRUCT").AppendLine().Append("{p3}").Append("(*" + Strings.ParamsCount_Comment + "*)").AppendLine().Append("{p4}").Append("\tnCount: DINT;").AppendLine().Append("{p5}").Append("(*" + Strings.ParamsArray_Comment + "*)").AppendLine().Append("{p6}").Append(string.Format("\tarrParams: ARRAY[1..{0}] OF {1};", this.Count.ToString(), this.BaseType)).AppendLine().Append("{p7}").Append("END_STRUCT").AppendLine().Append("{p8}").Append("END_TYPE").Append("{p9}").Append("{implicit off}").ToString();
			}
		}

		// Token: 0x040005FB RID: 1531
		[DefaultSerialization("BaseType")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IType m_typeBase;

		// Token: 0x040005FC RID: 1532
		[DefaultSerialization("MaximalNumber")]
		[StorageVersion("3.5.8.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private _IExpression _expCount;
	}
}

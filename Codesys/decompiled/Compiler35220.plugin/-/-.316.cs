using System;
using System.Runtime.CompilerServices;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001A
{
	// Token: 0x02000359 RID: 857
	internal sealed class \u0014
	{
		// Token: 0x06003365 RID: 13157 RVA: 0x000C7E30 File Offset: 0x000C6030
		public \u0014(IScope5 \u0088\u0004, IScope5 \u0089\u0004)
		{
			this.\u0001 = \u0088\u0004;
			this.\u0002 = \u0089\u0004;
		}

		// Token: 0x06003366 RID: 13158 RVA: 0x000C7E54 File Offset: 0x000C6054
		public bool \u0001()
		{
			return this.MovedVariable != null && this.MovedVariableRef == null;
		}

		// Token: 0x17000870 RID: 2160
		// (get) Token: 0x06003367 RID: 13159 RVA: 0x000C7E6C File Offset: 0x000C606C
		public ISignature ContainingSignature
		{
			get
			{
				return this.Scope[this.ContainingSignatureRef.Id];
			}
		}

		// Token: 0x17000871 RID: 2161
		// (get) Token: 0x06003368 RID: 13160 RVA: 0x000C7E84 File Offset: 0x000C6084
		// (set) Token: 0x06003369 RID: 13161 RVA: 0x000C7E8C File Offset: 0x000C608C
		public ISignature ContainingSignatureRef { get; internal set; }

		// Token: 0x17000872 RID: 2162
		// (get) Token: 0x0600336A RID: 13162 RVA: 0x000C7E98 File Offset: 0x000C6098
		public IVariable MovedVariable
		{
			get
			{
				ISignature signature = this.ContainingSignature;
				if (signature == null)
				{
					return null;
				}
				return signature[this.MovedVariableRef.Id];
			}
		}

		// Token: 0x17000873 RID: 2163
		// (get) Token: 0x0600336B RID: 13163 RVA: 0x000C7EB8 File Offset: 0x000C60B8
		// (set) Token: 0x0600336C RID: 13164 RVA: 0x000C7EC0 File Offset: 0x000C60C0
		public IVariable MovedVariableRef { get; internal set; }

		// Token: 0x17000874 RID: 2164
		// (get) Token: 0x0600336D RID: 13165 RVA: 0x000C7ECC File Offset: 0x000C60CC
		// (set) Token: 0x0600336E RID: 13166 RVA: 0x000C7ED4 File Offset: 0x000C60D4
		public string VariablePath { get; internal set; }

		// Token: 0x17000875 RID: 2165
		// (get) Token: 0x0600336F RID: 13167 RVA: 0x000C7EE0 File Offset: 0x000C60E0
		// (set) Token: 0x06003370 RID: 13168 RVA: 0x000C7EE8 File Offset: 0x000C60E8
		public _IType VariableType { get; internal set; }

		// Token: 0x17000876 RID: 2166
		// (get) Token: 0x06003371 RID: 13169 RVA: 0x000C7EF4 File Offset: 0x000C60F4
		public int VarId
		{
			get
			{
				return this.MovedVariableRef.Id;
			}
		}

		// Token: 0x17000877 RID: 2167
		// (get) Token: 0x06003372 RID: 13170 RVA: 0x000C7F04 File Offset: 0x000C6104
		public int SignId
		{
			get
			{
				return this.ContainingSignatureRef.Id;
			}
		}

		// Token: 0x06003373 RID: 13171 RVA: 0x000C7F14 File Offset: 0x000C6114
		public bool \u0002()
		{
			return this.MovedVariableRef.HasAttribute(CompileAttributes.ATTRIBUTE_NO_RELINK_CODE);
		}

		// Token: 0x17000878 RID: 2168
		// (get) Token: 0x06003374 RID: 13172 RVA: 0x000C7F28 File Offset: 0x000C6128
		public IScope5 Scope
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x17000879 RID: 2169
		// (get) Token: 0x06003375 RID: 13173 RVA: 0x000C7F30 File Offset: 0x000C6130
		public IScope5 ScopeRef
		{
			get
			{
				return this.\u0002;
			}
		}

		// Token: 0x1700087A RID: 2170
		// (get) Token: 0x06003376 RID: 13174 RVA: 0x000C7F38 File Offset: 0x000C6138
		// (set) Token: 0x06003377 RID: 13175 RVA: 0x000C7F40 File Offset: 0x000C6140
		public \u0014 ParentVar { get; internal set; }

		// Token: 0x1700087B RID: 2171
		// (get) Token: 0x06003378 RID: 13176 RVA: 0x000C7F4C File Offset: 0x000C614C
		// (set) Token: 0x06003379 RID: 13177 RVA: 0x000C7F54 File Offset: 0x000C6154
		public ValueTuple<IDataLocation, IDataLocation>? FixedParentDataLocation { get; internal set; }

		// Token: 0x1700087C RID: 2172
		// (get) Token: 0x0600337A RID: 13178 RVA: 0x000C7F60 File Offset: 0x000C6160
		// (set) Token: 0x0600337B RID: 13179 RVA: 0x000C7F68 File Offset: 0x000C6168
		public int ArrayIndexOld { get; internal set; }

		// Token: 0x1700087D RID: 2173
		// (get) Token: 0x0600337C RID: 13180 RVA: 0x000C7F74 File Offset: 0x000C6174
		// (set) Token: 0x0600337D RID: 13181 RVA: 0x000C7F7C File Offset: 0x000C617C
		public int? ArrayIndexNew { get; internal set; } = new int?(0);

		// Token: 0x0600337E RID: 13182 RVA: 0x000C7F88 File Offset: 0x000C6188
		internal void \u0001(out IDataLocation \u0002, out IDataLocation \u0003)
		{
			if (this.MovedVariableRef.DataLocation.IsRelativ)
			{
				\u0003 = ((this.MovedVariable != null) ? \u0019.\u0003.\u0001(this.MovedVariable.DataLocation.Offset) : null);
				\u0002 = \u0019.\u0003.\u0001(this.MovedVariableRef.DataLocation.Offset);
			}
			else
			{
				\u0003 = ((this.MovedVariable != null) ? \u0019.\u0003.\u0001(this.MovedVariable.DataLocation.Area, this.MovedVariable.DataLocation.Offset) : null);
				\u0002 = \u0019.\u0003.\u0001(this.MovedVariableRef.DataLocation.Area, this.MovedVariableRef.DataLocation.Offset);
				this.\u0001(\u0002, ref \u0003);
			}
			while (\u0002.IsRelativ)
			{
				IDataLocation item;
				IDataLocation item2;
				if (this.FixedParentDataLocation != null)
				{
					item = this.FixedParentDataLocation.Value.Item1;
					item2 = this.FixedParentDataLocation.Value.Item2;
				}
				else
				{
					this.ParentVar.\u0001(out item, out item2);
				}
				if (\u0003 == null || item2 == null)
				{
					\u0003 = null;
				}
				else
				{
					\u0003 = \u0019.\u0003.\u0001(item2.Area, \u0003.Offset + item2.Offset);
				}
				\u0002 = \u0019.\u0003.\u0001(item.Area, \u0002.Offset + item.Offset);
				this.\u0001(\u0002, ref \u0003);
			}
		}

		// Token: 0x0600337F RID: 13183 RVA: 0x000C80EC File Offset: 0x000C62EC
		private void \u0001(IDataLocation \u0002, ref IDataLocation \u0003)
		{
			int num = 0;
			int num2 = 0;
			if (this.ArrayIndexOld != 0)
			{
				num2 = this.ArrayIndexOld * this.VariableType.Size(this.ScopeRef);
			}
			if (this.ArrayIndexNew == null)
			{
				\u0003 = null;
			}
			else
			{
				int? num3 = this.ArrayIndexNew;
				int num4 = 0;
				if (!(num3.GetValueOrDefault() == num4 & num3 != null))
				{
					num = this.ArrayIndexNew.Value * this.VariableType.Size(this.Scope);
				}
			}
			((_IDataLocation)\u0002).Offset += num2;
			if (\u0003 != null)
			{
				((_IDataLocation)\u0003).Offset += num;
			}
		}

		// Token: 0x06003380 RID: 13184 RVA: 0x000C819C File Offset: 0x000C639C
		public \u0014 \u0001(_ISignature \u0002, _IVariable \u0003)
		{
			return new \u0014(this.Scope, this.ScopeRef)
			{
				VariablePath = this.VariablePath + "." + \u0003.OrgName,
				MovedVariableRef = \u0003,
				ContainingSignatureRef = \u0002,
				VariableType = (\u0003.CompiledType as _IType),
				ParentVar = this
			};
		}

		// Token: 0x06003381 RID: 13185 RVA: 0x000C81FC File Offset: 0x000C63FC
		public string \u0002()
		{
			return string.Format("{0} : {1}", this.VariablePath, this.VariableType);
		}

		// Token: 0x040009D7 RID: 2519
		private readonly IScope5 \u0001;

		// Token: 0x040009D8 RID: 2520
		private readonly IScope5 \u0002;

		// Token: 0x040009D9 RID: 2521
		[CompilerGenerated]
		private ISignature \u0001;

		// Token: 0x040009DA RID: 2522
		[CompilerGenerated]
		private IVariable \u0001;

		// Token: 0x040009DB RID: 2523
		[CompilerGenerated]
		private string \u0001;

		// Token: 0x040009DC RID: 2524
		[CompilerGenerated]
		private _IType \u0001;

		// Token: 0x040009DD RID: 2525
		[CompilerGenerated]
		private \u0014 \u0001;

		// Token: 0x040009DE RID: 2526
		[CompilerGenerated]
		private ValueTuple<IDataLocation, IDataLocation>? \u0001;

		// Token: 0x040009DF RID: 2527
		[CompilerGenerated]
		private int \u0001;

		// Token: 0x040009E0 RID: 2528
		[CompilerGenerated]
		private int? \u0001;
	}
}

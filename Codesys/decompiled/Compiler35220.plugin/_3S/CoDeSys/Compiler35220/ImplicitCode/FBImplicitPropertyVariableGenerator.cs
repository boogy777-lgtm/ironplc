using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.ImplicitCode
{
	// Token: 0x020003B6 RID: 950
	public static class FBImplicitPropertyVariableGenerator
	{
		// Token: 0x06003696 RID: 13974 RVA: 0x000DD398 File Offset: 0x000DB598
		internal static void \u0001(_ISignature \u0002, _ISignature \u0003)
		{
			foreach (FBImplicitPropertyVariableGenerator.PropertyInfo u in FBImplicitPropertyVariableGenerator.\u0001(\u0002))
			{
				FBImplicitPropertyVariableGenerator.\u0001(\u0002, \u0003, u);
			}
		}

		// Token: 0x06003697 RID: 13975 RVA: 0x000DD3E8 File Offset: 0x000DB5E8
		private static void \u0001(_ISignature \u0002, _ISignature \u0003, FBImplicitPropertyVariableGenerator.PropertyInfo \u0004)
		{
			_IVariable ivariable = \u0002[\u0004.PropertyName] as _IVariable;
			if (ivariable == null)
			{
				int id;
				if (\u0003 != null && \u0003[\u0004.PropertyName] != null)
				{
					id = \u0003[\u0004.PropertyName].Id;
				}
				else
				{
					id = \u0002.NextId;
				}
				ivariable = FBImplicitPropertyVariableGenerator.CreatePropertyVariable(\u0002, \u0004);
				ivariable.SetFlag(VarFlag.IsCompiled, true);
				ivariable.Id = id;
				\u0002.AddVariable(ivariable);
			}
			if (\u0002.POUType != Operator.Interface)
			{
				FBImplicitPropertyVariableGenerator.\u0001(\u0002, \u0003, \u0004, ivariable);
			}
		}

		// Token: 0x06003698 RID: 13976 RVA: 0x000DD470 File Offset: 0x000DB670
		public static _IVariable CreatePropertyVariable(_ISignature sign, FBImplicitPropertyVariableGenerator.PropertyInfo property)
		{
			_IVariable ivariable = \u0003.\u0001(\u0003.\u0001());
			ivariable.Name = property.PropertyName;
			ivariable._Type = property.Type;
			ivariable.SetFlag(VarFlag.Local, true);
			if (Operator.Program == sign.POUType || Operator.VarGlobal == sign.POUType)
			{
				ivariable.SetFlag(VarFlag.Absolut, true);
			}
			ivariable.AddAttribute(CompileAttributes.ATTRIBUTE_PROPERTY, string.Empty);
			if (property.Transition)
			{
				ivariable.AddAttribute(CompileAttributes.ATTRIBUTE_TRANSITION, string.Empty);
			}
			else
			{
				if (property.Getter)
				{
					ivariable.AddAttribute(CompileAttributes.ATTRIBUTE_GET, string.Empty);
				}
				if (property.Setter)
				{
					ivariable.AddAttribute(CompileAttributes.ATTRIBUTE_SET, string.Empty);
				}
			}
			ivariable.AddAttribute(CompileAttributes.ATTRIBUTE_MESSAGE_GUID, property.MessageGuid.ToString());
			FBImplicitPropertyVariableGenerator.\u0001(property, ivariable);
			return ivariable;
		}

		// Token: 0x06003699 RID: 13977 RVA: 0x000DD54C File Offset: 0x000DB74C
		private static void \u0001(FBImplicitPropertyVariableGenerator.PropertyInfo \u0002, _IVariable \u0003)
		{
			if (\u0002.Attributes == null)
			{
				return;
			}
			foreach (ValueTuple<string, string> valueTuple in \u0002.Attributes)
			{
				\u0003.AddAttribute(valueTuple.Item1, valueTuple.Item2);
			}
		}

		// Token: 0x0600369A RID: 13978 RVA: 0x000DD5B0 File Offset: 0x000DB7B0
		private static void \u0001(_ISignature \u0002, _ISignature \u0003, FBImplicitPropertyVariableGenerator.PropertyInfo \u0004, _IVariable \u0005)
		{
			if (\u0004.MonitoringByVariable)
			{
				_IVariable ivariable = \u0003.\u0001(\u0003.\u0001());
				ivariable.Name = "__Watch_" + \u0002.OrgName + "_" + \u0004.PropertyName;
				ivariable._Type = \u0004.Type;
				ivariable.SetFlag(VarFlag.Local | VarFlag.IsCompiled | VarFlag.NoInit | VarFlag.Implicit, true);
				ivariable.SetFlag(VarFlag.Absolut, \u0005.GetFlag(VarFlag.Absolut));
				ivariable.AddAttribute(CompileAttributes.ATTRIBUTE_USELOCATION, \u0005.OrgName);
				if (\u0003 != null && \u0003[ivariable.VersionedName] != null)
				{
					ivariable.Id = \u0003[ivariable.VersionedName].Id;
				}
				else
				{
					ivariable.Id = \u0002.NextId;
				}
				\u0002.AddVariable(ivariable);
			}
		}

		// Token: 0x0600369B RID: 13979 RVA: 0x000DD674 File Offset: 0x000DB874
		private static IReadOnlyList<FBImplicitPropertyVariableGenerator.PropertyInfo> \u0001(_ISignature \u0002)
		{
			Dictionary<string, ValueTuple<ISignature, ISignature>> dictionary = null;
			foreach (object obj in \u0002._SubSignatures)
			{
				ISignature signature = (ISignature)obj;
				string key;
				if (signature.GetFlag(SignatureFlag.RawSTProperty) && signature.\u0001(CompileAttributes.ATTRIBUTE_PROPERTY, out key))
				{
					dictionary = (dictionary ?? new Dictionary<string, ValueTuple<ISignature, ISignature>>());
					ValueTuple<ISignature, ISignature> value;
					dictionary.TryGetValue(key, out value);
					if (signature.Name.StartsWith("__GET"))
					{
						value.Item2 = signature;
					}
					else if (signature.Name.StartsWith("__SET"))
					{
						value.Item1 = signature;
					}
					dictionary[key] = value;
				}
			}
			if (dictionary != null)
			{
				List<FBImplicitPropertyVariableGenerator.PropertyInfo> list = new List<FBImplicitPropertyVariableGenerator.PropertyInfo>();
				foreach (KeyValuePair<string, ValueTuple<ISignature, ISignature>> keyValuePair in dictionary)
				{
					FBImplicitPropertyVariableGenerator.PropertyInfo propertyInfo = FBImplicitPropertyVariableGenerator.PropertyInfo.Create(keyValuePair.Key, keyValuePair.Value.Item2, keyValuePair.Value.Item1);
					if (propertyInfo != null)
					{
						list.Add(propertyInfo);
					}
				}
				return list;
			}
			return Array.Empty<FBImplicitPropertyVariableGenerator.PropertyInfo>();
		}

		// Token: 0x0600369C RID: 13980 RVA: 0x000DD7C4 File Offset: 0x000DB9C4
		internal static bool \u0001(this ISignature \u0002, string \u0003, out string \u0004)
		{
			\u0004 = \u0002.GetAttributeValue(\u0003);
			return \u0004 != null;
		}

		// Token: 0x020003B7 RID: 951
		public sealed class PropertyInfo
		{
			// Token: 0x170008DF RID: 2271
			// (get) Token: 0x0600369D RID: 13981 RVA: 0x000DD7D4 File Offset: 0x000DB9D4
			// (set) Token: 0x0600369E RID: 13982 RVA: 0x000DD7DC File Offset: 0x000DB9DC
			public bool Getter { get; private set; }

			// Token: 0x170008E0 RID: 2272
			// (get) Token: 0x0600369F RID: 13983 RVA: 0x000DD7E8 File Offset: 0x000DB9E8
			// (set) Token: 0x060036A0 RID: 13984 RVA: 0x000DD7F0 File Offset: 0x000DB9F0
			public bool Setter { get; private set; }

			// Token: 0x170008E1 RID: 2273
			// (get) Token: 0x060036A1 RID: 13985 RVA: 0x000DD7FC File Offset: 0x000DB9FC
			// (set) Token: 0x060036A2 RID: 13986 RVA: 0x000DD804 File Offset: 0x000DBA04
			internal bool Transition { get; private set; }

			// Token: 0x170008E2 RID: 2274
			// (get) Token: 0x060036A3 RID: 13987 RVA: 0x000DD810 File Offset: 0x000DBA10
			// (set) Token: 0x060036A4 RID: 13988 RVA: 0x000DD818 File Offset: 0x000DBA18
			public _IType Type { get; private set; }

			// Token: 0x170008E3 RID: 2275
			// (get) Token: 0x060036A5 RID: 13989 RVA: 0x000DD824 File Offset: 0x000DBA24
			// (set) Token: 0x060036A6 RID: 13990 RVA: 0x000DD82C File Offset: 0x000DBA2C
			public Guid MessageGuid { get; private set; }

			// Token: 0x170008E4 RID: 2276
			// (get) Token: 0x060036A7 RID: 13991 RVA: 0x000DD838 File Offset: 0x000DBA38
			// (set) Token: 0x060036A8 RID: 13992 RVA: 0x000DD840 File Offset: 0x000DBA40
			public bool MonitoringByVariable { get; private set; }

			// Token: 0x060036A9 RID: 13993 RVA: 0x000DD84C File Offset: 0x000DBA4C
			private void \u0001(string \u0002, string \u0003)
			{
				if (this.\u0001 == null)
				{
					this.\u0001 = new List<ValueTuple<string, string>>();
				}
				this.\u0001.Add(new ValueTuple<string, string>(\u0002, \u0003));
			}

			// Token: 0x170008E5 RID: 2277
			// (get) Token: 0x060036AA RID: 13994 RVA: 0x000DD874 File Offset: 0x000DBA74
			internal IEnumerable<ValueTuple<string, string>> Attributes
			{
				get
				{
					return this.\u0001;
				}
			}

			// Token: 0x060036AB RID: 13995 RVA: 0x000DD87C File Offset: 0x000DBA7C
			private PropertyInfo(string propertyName, bool getter, bool setter)
			{
				this.PropertyName = propertyName;
				this.Getter = getter;
				this.Setter = setter;
				this.Type = null;
				this.MessageGuid = Guid.Empty;
				this.MonitoringByVariable = false;
			}

			// Token: 0x060036AC RID: 13996 RVA: 0x000DD8B4 File Offset: 0x000DBAB4
			public static FBImplicitPropertyVariableGenerator.PropertyInfo Create(string stName, ISignature getter, ISignature setter)
			{
				if (getter == null && setter == null)
				{
					return null;
				}
				FBImplicitPropertyVariableGenerator.PropertyInfo propertyInfo = new FBImplicitPropertyVariableGenerator.PropertyInfo(stName, getter != null, setter != null);
				ISignature signature = getter ?? setter;
				propertyInfo.Transition = signature.HasAttribute(CompileAttributes.ATTRIBUTE_TRANSITION);
				_IVariable ivariable = (_IVariable)signature[stName];
				if (ivariable == null)
				{
					return null;
				}
				propertyInfo.Type = ivariable._Type;
				propertyInfo.MessageGuid = signature.ObjectGuid;
				if (signature.GetAttributeValue(CompileAttributes.ATTRIBUTE_MONITORING) == CompileAttributes.ATTRIBUTEVALUE_VARIABLE)
				{
					propertyInfo.MonitoringByVariable = true;
				}
				foreach (string text in signature.Attributes)
				{
					string attributeValue = signature.GetAttributeValue(text);
					propertyInfo.\u0001(text, attributeValue);
				}
				return propertyInfo;
			}

			// Token: 0x04000AA4 RID: 2724
			public readonly string PropertyName;

			// Token: 0x04000AA5 RID: 2725
			[CompilerGenerated]
			private bool \u0001;

			// Token: 0x04000AA6 RID: 2726
			[CompilerGenerated]
			private bool \u0002;

			// Token: 0x04000AA7 RID: 2727
			[CompilerGenerated]
			private bool \u0003;

			// Token: 0x04000AA8 RID: 2728
			[CompilerGenerated]
			private _IType \u0001;

			// Token: 0x04000AA9 RID: 2729
			[CompilerGenerated]
			private Guid \u0001;

			// Token: 0x04000AAA RID: 2730
			[CompilerGenerated]
			private bool \u0004;

			// Token: 0x04000AAB RID: 2731
			private List<ValueTuple<string, string>> \u0001;
		}
	}
}

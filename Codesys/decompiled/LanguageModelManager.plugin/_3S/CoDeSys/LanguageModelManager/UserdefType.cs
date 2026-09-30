using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020001A4 RID: 420
	[TypeGuid("{8499b87a-2b75-4c62-8657-e4aa9316c18b}")]
	[StorageVersion("3.3.0.0")]
	public class UserdefType : IECType, _IUserdefType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, IUserdefType2, IUserdefType
	{
		// Token: 0x06001E73 RID: 7795 RVA: 0x00053E95 File Offset: 0x00052E95
		public UserdefType()
		{
		}

		// Token: 0x06001E74 RID: 7796 RVA: 0x00053EB3 File Offset: 0x00052EB3
		internal UserdefType(_IExpression qne)
		{
			this.m_qneTypeDef = qne;
		}

		// Token: 0x06001E75 RID: 7797 RVA: 0x00053ED8 File Offset: 0x00052ED8
		public UserdefType(string stName)
		{
			this.m_qneTypeDef = LanguageModelBuilder.Singleton.CreateVariableExpression(stName);
		}

		// Token: 0x170007FB RID: 2043
		// (get) Token: 0x06001E76 RID: 7798 RVA: 0x00053F07 File Offset: 0x00052F07
		[Obsolete("QualifiedNameExpression is no longer used, function will return null. Use InstanceExpression instead")]
		public IQualifiedNameExpression Name
		{
			get
			{
				return this.m_qneTypeDef as IQualifiedNameExpression;
			}
		}

		// Token: 0x170007FC RID: 2044
		// (get) Token: 0x06001E77 RID: 7799 RVA: 0x00053F14 File Offset: 0x00052F14
		// (set) Token: 0x06001E78 RID: 7800 RVA: 0x00053F1C File Offset: 0x00052F1C
		public IExpression NameExpression
		{
			get
			{
				return this.m_qneTypeDef;
			}
			set
			{
				this.m_qneTypeDef = (value as _IExpression);
			}
		}

		// Token: 0x06001E79 RID: 7801 RVA: 0x00053F2A File Offset: 0x00052F2A
		public ISignature GetSignature(IScope scope)
		{
			return scope[this.m_iSignatureId];
		}

		// Token: 0x170007FD RID: 2045
		// (get) Token: 0x06001E7A RID: 7802 RVA: 0x00053F38 File Offset: 0x00052F38
		// (set) Token: 0x06001E7B RID: 7803 RVA: 0x00053F40 File Offset: 0x00052F40
		public int SignatureId
		{
			get
			{
				return this.m_iSignatureId;
			}
			set
			{
				this.m_iSignatureId = value;
			}
		}

		// Token: 0x06001E7C RID: 7804 RVA: 0x00053F49 File Offset: 0x00052F49
		public IScope2 GetScope(IScope2 scope)
		{
			return scope.GetScopeById(this.m_iScopeId) as IScope2;
		}

		// Token: 0x170007FE RID: 2046
		// (get) Token: 0x06001E7D RID: 7805 RVA: 0x00053F5C File Offset: 0x00052F5C
		// (set) Token: 0x06001E7E RID: 7806 RVA: 0x00053F64 File Offset: 0x00052F64
		public int ScopeId
		{
			get
			{
				return this.m_iScopeId;
			}
			set
			{
				this.m_iScopeId = value;
			}
		}

		// Token: 0x06001E7F RID: 7807 RVA: 0x00053F70 File Offset: 0x00052F70
		public override string ToString()
		{
			if (this.m_qneTypeDef == null && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34100)
			{
				return "???";
			}
			string text = this.m_qneTypeDef.ToString();
			if (text.EndsWith("__Union", StringComparison.Ordinal))
			{
				text = text.Remove(text.LastIndexOf("__Union", StringComparison.Ordinal));
			}
			if (text.StartsWith("__PARAMS"))
			{
				string[] separator = new string[]
				{
					"__"
				};
				string[] array = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length == 3)
				{
					return string.Concat(new string[]
					{
						array[0],
						"(",
						array[2],
						") OF ",
						array[1]
					});
				}
			}
			return text;
		}

		// Token: 0x06001E80 RID: 7808 RVA: 0x00051202 File Offset: 0x00050202
		public override string ToUpperString()
		{
			return this.ToString().ToUpperInvariant();
		}

		// Token: 0x170007FF RID: 2047
		// (get) Token: 0x06001E81 RID: 7809 RVA: 0x00054023 File Offset: 0x00053023
		public override TypeClass Class
		{
			get
			{
				return TypeClass.Userdef;
			}
		}

		// Token: 0x06001E82 RID: 7810 RVA: 0x00054027 File Offset: 0x00053027
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001E83 RID: 7811 RVA: 0x00054030 File Offset: 0x00053030
		public override bool IsEqual(ICompiledType type, IScope scope)
		{
			UserdefType userdefType = type as UserdefType;
			if (userdefType == null)
			{
				return false;
			}
			if (this.m_iSignatureId == Common.InvalidID && this.m_iScopeId == Common.InvalidID)
			{
				return base.IsEqual(type, scope);
			}
			if (userdefType.m_iSignatureId == Common.InvalidID && userdefType.m_iScopeId == Common.InvalidID)
			{
				return base.IsEqual(type, scope);
			}
			return this.m_iSignatureId == userdefType.m_iSignatureId && this.m_iScopeId == userdefType.m_iScopeId;
		}

		// Token: 0x06001E84 RID: 7812 RVA: 0x000540AD File Offset: 0x000530AD
		public override bool IsEqualPreCompile(ICompiledType type, IScope scope)
		{
			return type is UserdefType && base.IsEqualPreCompile(type, scope);
		}

		// Token: 0x06001E85 RID: 7813 RVA: 0x000540C4 File Offset: 0x000530C4
		public override _IType _Duplicate(bool bDeep)
		{
			UserdefType userdefType;
			if (this.m_qneTypeDef != null)
			{
				userdefType = new UserdefType(this.m_qneTypeDef.Duplicate() as _IExpression);
			}
			else
			{
				userdefType = new UserdefType(null);
			}
			if (bDeep)
			{
				userdefType.SignatureId = this.SignatureId;
				userdefType.ScopeId = this.ScopeId;
			}
			else
			{
				userdefType.SignatureId = Common.InvalidID;
				userdefType.ScopeId = Common.InvalidID;
			}
			return userdefType;
		}

		// Token: 0x06001E86 RID: 7814 RVA: 0x0005412C File Offset: 0x0005312C
		public override int SizeChecked(IScope scope, out bool bValid)
		{
			ISignature signature = scope[this.m_iSignatureId];
			bValid = false;
			if (signature == null)
			{
				return 0;
			}
			int result;
			if (signature.GetFlag(SignatureFlag.Enum) && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35500 && signature.All.Length != 0)
			{
				result = (signature.All[0].CompiledType.DeRefType as _IType).SizeChecked(scope, out bValid);
			}
			else if (!signature.GetFlag(SignatureFlag.Located))
			{
				result = 0;
			}
			else
			{
				result = signature.Size;
				bValid = true;
			}
			return result;
		}

		// Token: 0x06001E87 RID: 7815 RVA: 0x000541B4 File Offset: 0x000531B4
		public override int Size(IScope scope)
		{
			ISignature signature = scope[this.m_iSignatureId];
			if (signature == null)
			{
				return 0;
			}
			return signature.Size;
		}

		// Token: 0x06001E88 RID: 7816 RVA: 0x000541DC File Offset: 0x000531DC
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			ISignature signature = this.GetSignature(scope);
			Debug.Assert(signature != null && signature.GetFlag(SignatureFlag.Compiled));
			if (signature.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
			{
				if (raw.Length != scope.PointerSize)
				{
					return false;
				}
			}
			else if (raw.Length != signature.Size)
			{
				return false;
			}
			return true;
		}

		// Token: 0x06001E89 RID: 7817 RVA: 0x00054234 File Offset: 0x00053234
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			ISignature signature = this.GetSignature(scope);
			Debug.Assert(signature != null && signature.GetFlag(SignatureFlag.Compiled));
			if (!signature.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
			{
				ArrayList arrayList = new ArrayList();
				foreach (IVariable variable in signature.All)
				{
					if (variable.DataLocation != null && variable.DataLocation.IsRelativ)
					{
						int num = variable.CompiledType.Size(scope);
						if (variable.DataLocation.Offset + num > raw.Length)
						{
							throw new InvalidCastException("Not enough data available for doing a ConvertRaw on the userdeftype '" + signature.OrgName + "'");
						}
						byte[] array = new byte[num];
						Array.Copy(raw, variable.DataLocation.Offset, array, 0, num);
						if (((_IType)variable.CompiledType).CanConvertRaw(array, byteOrder, scope))
						{
							arrayList.Add(((_IType)variable.Type).ConvertRaw(array, byteOrder, scope));
						}
					}
				}
				object[] array2 = new object[arrayList.Count];
				arrayList.CopyTo(array2);
				return array2;
			}
			if (raw.Length != scope.PointerSize)
			{
				throw new InvalidCastException("Invalid length for an interface");
			}
			if (raw.Length != 4)
			{
				ulong num2 = 0UL;
				for (int j = 0; j < 8; j++)
				{
					ulong num3 = (ulong)raw[j] << 8 * j;
					num2 |= num3;
				}
				if (byteOrder == ByteOrder.Motorola)
				{
					BitHelper.Swap(ref num2);
				}
				return num2;
			}
			uint num4 = 0U;
			for (int k = 0; k < 4; k++)
			{
				num4 |= (uint)((uint)raw[k] << 8 * k);
			}
			if (byteOrder == ByteOrder.Motorola)
			{
				return BitHelper.Swap(num4);
			}
			return num4;
		}

		// Token: 0x06001E8A RID: 7818 RVA: 0x000543F4 File Offset: 0x000533F4
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			ISignature signature = this.GetSignature(scope);
			Debug.Assert(signature != null && signature.GetFlag(SignatureFlag.Compiled));
			return !signature.GetFlag(SignatureFlag.ImplicitInterfaceUnion);
		}

		// Token: 0x06001E8B RID: 7819 RVA: 0x00054434 File Offset: 0x00053434
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			_ISignature isignature = this.GetSignature(scope) as _ISignature;
			Debug.Assert(isignature != null && isignature.GetFlag(SignatureFlag.Compiled));
			if (isignature.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
			{
				throw new InvalidCastException("Interface cannot be written");
			}
			byte[] array = new byte[isignature.Size];
			object[] array2 = value as object[];
			int i = 0;
			int num = 0;
			while (i < isignature.AllVariables.Count)
			{
				IVariable variable = isignature.AllVariables[i];
				object value2 = array2[num];
				if (variable.DataLocation != null && variable.DataLocation.IsRelativ && ((_IType)variable.CompiledType).CanConvertToRaw(value2, byteOrder, scope))
				{
					num++;
					byte[] array3 = ((_IType)variable.Type).ConvertToRaw(value2, byteOrder, scope);
					Debug.Assert(variable.DataLocation.Offset + array3.Length <= array.Length);
					Array.Copy(array3, 0, array, variable.DataLocation.Offset, array3.Length);
				}
				i++;
			}
			return array;
		}

		// Token: 0x06001E8C RID: 7820 RVA: 0x00054548 File Offset: 0x00053548
		internal static int GetNumOfComponents(IScope5 scope, _ISignature sign)
		{
			if (sign != null)
			{
				_ISignature sign2 = scope[sign.BaseSignatureId] as _ISignature;
				UserdefType.GetNumOfComponents(scope, sign2);
				IVariable[] all = sign.All;
				int num = 0;
				IVariable[] array = all;
				for (int i = 0; i < array.Length; i++)
				{
					if (!array[i].GetFlag(VarFlag.Implicit))
					{
						num++;
					}
				}
				return num;
			}
			return 0;
		}

		// Token: 0x06001E8D RID: 7821 RVA: 0x000545A0 File Offset: 0x000535A0
		public override int GetNumOfElements(IScope5 scope)
		{
			ISignature signature = this.GetSignature(scope);
			return UserdefType.GetNumOfComponents(scope, signature as _ISignature);
		}

		// Token: 0x06001E8E RID: 7822 RVA: 0x000545C4 File Offset: 0x000535C4
		public override ICompiledType GetComponent(int i, IScope5 scope)
		{
			if (i < 0 || i >= this.GetNumOfElements(scope))
			{
				throw new ArgumentOutOfRangeException("i");
			}
			_ISignature isignature = this.GetSignature(scope) as _ISignature;
			if (isignature != null)
			{
				IEnumerable<IVariable> allVariables = isignature.AllVariables;
				int num = 0;
				foreach (IVariable variable in allVariables)
				{
					if (!variable.GetFlag(VarFlag.Implicit))
					{
						if (num == i)
						{
							return ((_IVariable)variable)._Type;
						}
						num++;
					}
				}
			}
			return null;
		}

		// Token: 0x06001E8F RID: 7823 RVA: 0x00054660 File Offset: 0x00053660
		internal static ISignatureMemberHierachyInfo[] GetSignatureComponents(IScope5 scope, _ISignature sign, GUIHidingFlags eFlagsToConsider, int iInheritanceHiearchyLevel)
		{
			_ISignature isignature = scope[sign.BaseSignatureId] as _ISignature;
			LList<ISignatureMemberHierachyInfo> llist = new LList<ISignatureMemberHierachyInfo>();
			if (isignature != null)
			{
				llist.AddRange(UserdefType.GetSignatureComponents(scope, isignature, eFlagsToConsider, iInheritanceHiearchyLevel + 1));
			}
			IList<_IVariable> allVariables = sign.AllVariables;
			string text = ".";
			if (sign.Name == "__MAIN")
			{
				text += "__MAIN.";
			}
			if (!UserdefType.AddVariables(scope, sign, eFlagsToConsider, iInheritanceHiearchyLevel, llist, allVariables, text) && isignature != null)
			{
				llist.Add(new SignatureMemberHierachyInfo(sign, iInheritanceHiearchyLevel));
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351400)
			{
				UserdefType.AddInterfaces(scope, sign, eFlagsToConsider, iInheritanceHiearchyLevel, llist);
			}
			if (sign.POUType == Operator.FunctionBlock)
			{
				_ISignature isignature2 = sign.GetSubSignature("__MAIN") as _ISignature;
				if (isignature2 != null)
				{
					llist.AddRange(UserdefType.GetSignatureComponents(scope, isignature2, eFlagsToConsider, iInheritanceHiearchyLevel));
				}
			}
			return llist.ToArray();
		}

		// Token: 0x06001E90 RID: 7824 RVA: 0x00054734 File Offset: 0x00053734
		private static void AddInterfaces(IScope5 scope, _ISignature sign, GUIHidingFlags eFlagsToConsider, int iInheritanceHiearchyLevel, LList<ISignatureMemberHierachyInfo> components)
		{
			int[] interfaceIds = sign.InterfaceIds;
			if (interfaceIds.Length != 0)
			{
				foreach (int nId in interfaceIds)
				{
					_ISignature isignature = scope[nId] as _ISignature;
					if (isignature != null)
					{
						components.AddRange(UserdefType.GetSignatureComponents(scope, isignature, eFlagsToConsider, iInheritanceHiearchyLevel));
					}
				}
			}
		}

		// Token: 0x06001E91 RID: 7825 RVA: 0x00054784 File Offset: 0x00053784
		private static bool AddVariables(IScope5 scope, _ISignature sign, GUIHidingFlags eFlagsToConsider, int iInheritanceHiearchyLevel, LList<ISignatureMemberHierachyInfo> components, IList<_IVariable> allVars, string prefix)
		{
			bool result = false;
			foreach (IVariable variable in allVars)
			{
				bool flag = false;
				if (eFlagsToConsider != GUIHidingFlags.None && sign.IsLibraryObject)
				{
					ILMCompiledApplicationQuery4 ilmcompiledApplicationQuery = APEnvironmentFacade.Instance.LMServiceProvider.CompileService.QueryCompiledApplicationSet(scope.ApplicationContext.ApplicationGuid) as ILMCompiledApplicationQuery4;
					_ISignature isignature = ((ilmcompiledApplicationQuery != null) ? ilmcompiledApplicationQuery.FindPrecompileSignature(sign) : null) as _ISignature;
					if (isignature == null)
					{
						flag = true;
					}
					else if (isignature.IsCompiledLibraryObject)
					{
						flag = (APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.IsHiddenVariable(isignature, variable, eFlagsToConsider) || UserdefType.IsInternalProperty(sign, variable));
					}
				}
				if (!variable.GetFlag(VarFlag.Implicit) && !flag)
				{
					components.Add(new SignatureMemberHierachyInfo(prefix + variable.OrgName, sign, iInheritanceHiearchyLevel));
					result = true;
				}
			}
			return result;
		}

		// Token: 0x06001E92 RID: 7826 RVA: 0x00054878 File Offset: 0x00053878
		private static bool IsInternalProperty(_ISignature sign, IVariable var)
		{
			if (var.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY))
			{
				ISignature subSignature = sign.GetSubSignature("__get" + var.OrgName);
				if (subSignature != null && subSignature.GetFlag(SignatureFlag.Internal))
				{
					return true;
				}
				ISignature subSignature2 = sign.GetSubSignature("__set" + var.OrgName);
				if (subSignature2 != null && subSignature2.GetFlag(SignatureFlag.Internal))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001E93 RID: 7827 RVA: 0x000548F0 File Offset: 0x000538F0
		public override string[] GetComponents(IScope5 scope, out bool bValid)
		{
			return (from sse in this.GetComponents(scope, out bValid, GUIHidingFlags.None)
			where !sse.EmptyLevel
			select sse.MemberName).ToArray<string>();
		}

		// Token: 0x06001E94 RID: 7828 RVA: 0x00054954 File Offset: 0x00053954
		internal ISignatureMemberHierachyInfo[] GetComponents(IScope5 scope, out bool bValid, GUIHidingFlags eFlagsToConsider)
		{
			bValid = true;
			ISignature signature = this.GetSignature(scope);
			if (signature == null)
			{
				return new SignatureMemberHierachyInfo[0];
			}
			if (signature.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
			{
				UserdefType userdefType = signature["__Interface"].CompiledType.BaseType as UserdefType;
				if (userdefType != null)
				{
					signature = userdefType.GetSignature(scope);
				}
			}
			return UserdefType.GetSignatureComponents(scope, signature as _ISignature, eFlagsToConsider, 0);
		}

		// Token: 0x04000601 RID: 1537
		[DefaultSerialization("TypeDef")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		[DefaultDuplication(DuplicationMethod.Deep)]
		internal _IExpression m_qneTypeDef;

		// Token: 0x04000602 RID: 1538
		[DefaultSerialization("SignatureId")]
		[StorageVersion("3.3.0.0")]
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[Obfuscation(Feature = "rename")]
		protected int m_iSignatureId = Common.InvalidID;

		// Token: 0x04000603 RID: 1539
		[DefaultSerialization("ScopeId")]
		[StorageVersion("3.3.0.0")]
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[Obfuscation(Feature = "rename")]
		protected int m_iScopeId = Common.InvalidID;
	}
}

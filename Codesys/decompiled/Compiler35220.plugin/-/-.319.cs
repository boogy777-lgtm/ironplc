using System;
using System.Collections.Generic;
using System.Linq;
using \u0002;
using \u0007;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0084;

namespace \u0001
{
	// Token: 0x02000366 RID: 870
	internal sealed class \u0010
	{
		// Token: 0x060033F0 RID: 13296 RVA: 0x000CC07C File Offset: 0x000CA27C
		internal \u0010(_ICompileContext \u001C\u0004, _ICompileContext \u001B\u0003)
		{
			this.\u0001 = \u001C\u0004;
			this.\u0002 = \u001B\u0003;
			this.\u0001 = global::\u0007.\u0005.\u0001(this.\u0001);
			this.\u0002 = global::\u0007.\u0005.\u0001(this.\u0002);
		}

		// Token: 0x060033F1 RID: 13297 RVA: 0x000CC0B4 File Offset: 0x000CA2B4
		internal void \u0001()
		{
			IList<_ISignature> allFlat = this.\u0001.AllFlat;
			foreach (_ISignature isignature in allFlat)
			{
				isignature.SetFlag(SignatureFlag.Temp, false);
				isignature.SetFlag(SignatureFlag.OnlineChanged, false);
			}
			foreach (_ISignature isignature2 in allFlat)
			{
				_ISignature isignature3 = this.\u0002[isignature2.Id];
				bool flag = isignature2.HasAttribute(CompileAttributes.ATTRIBUTE_PARAMETERLIST) && isignature3 != null && this.\u0002.ParameterTableChecksum != this.\u0001.ParameterTableChecksum;
				if (isignature3 == null || (isignature3.Checksum != 0U && isignature2.Checksum != isignature3.Checksum) || isignature2.GetFlag((SignatureFlag)((ulong)-2147483648)) || flag)
				{
					if (isignature3 != null && isignature3.Checksum == 0U)
					{
						if (isignature2.IsEqualCompile(isignature3, false))
						{
							continue;
						}
					}
					else if (isignature3 != null && isignature2.GetFlag((SignatureFlag)((ulong)-2147483648)) && isignature2.ChecksumNoInit == isignature3.ChecksumNoInit && !flag)
					{
						if (global::\u0001.\u0010.\u0001(isignature2, isignature3))
						{
							continue;
						}
					}
					else if (isignature3 != null && isignature2.ChecksumNoInit == isignature3.ChecksumNoInit && !flag)
					{
						continue;
					}
					if (!isignature2.GetFlag(SignatureFlag.Temp))
					{
						_ISignature u = null;
						if (this.\u0002 != null)
						{
							u = this.\u0002[isignature2.Id];
						}
						this.\u0001(isignature2, u);
					}
					if (isignature2.GetFlagInternal(SignatureFlagInternal.ContainsInstanceVars) || (isignature3 != null && isignature3.GetFlagInternal(SignatureFlagInternal.ContainsInstanceVars)))
					{
						bool flag2 = isignature3 == null || isignature2.InstanceLocalsChanged(isignature3);
						_ISignature isignature4 = this.\u0001.GetSignatureById(isignature2.ParentSignatureId) as _ISignature;
						if (flag2)
						{
							_ISignature u2 = null;
							if (this.\u0002 != null)
							{
								u2 = this.\u0002[isignature4.Id];
							}
							this.\u0001(isignature4, u2);
						}
					}
				}
			}
			foreach (_ISignature isignature5 in allFlat)
			{
				isignature5.SetFlag(SignatureFlag.Temp, false);
			}
		}

		// Token: 0x060033F2 RID: 13298 RVA: 0x000CC328 File Offset: 0x000CA528
		private void \u0001(_ISignature \u0002, _ISignature \u0003)
		{
			\u0002.SetFlag(SignatureFlag.Temp, true);
			if (\u0002.GetFlag(SignatureFlag.OnlineChanged))
			{
				return;
			}
			\u0002.SetFlag(SignatureFlag.OnlineChanged, true);
			this.\u0001(\u0002);
			if (\u0003 == null)
			{
				return;
			}
			this.\u0002(\u0002, \u0003);
		}

		// Token: 0x060033F3 RID: 13299 RVA: 0x000CC368 File Offset: 0x000CA568
		private void \u0001(_ISignature \u0002)
		{
			foreach (int nId in \u0002.DeclarerIds)
			{
				_ISignature isignature = this.\u0001[nId];
				_ISignature u = null;
				if (this.\u0002 != null)
				{
					u = this.\u0002[nId];
				}
				bool flag = isignature.BaseSignatureId == \u0002.Id;
				if (!global::\u0001.\u0010.\u0001(\u0002, isignature, flag) || flag)
				{
					this.\u0001(isignature, u);
				}
			}
		}

		// Token: 0x060033F4 RID: 13300 RVA: 0x000CC3E0 File Offset: 0x000CA5E0
		private static bool \u0001(_ISignature \u0002, _ISignature \u0003, bool \u0004)
		{
			if (\u0004)
			{
				return true;
			}
			bool result = true;
			foreach (_IVariable u in \u0003.AllVariables)
			{
				if (!global::\u0001.\u0010.\u0001(\u0002, u))
				{
					result = false;
					break;
				}
			}
			return result;
		}

		// Token: 0x060033F5 RID: 13301 RVA: 0x000CC43C File Offset: 0x000CA63C
		private static bool \u0001(_ISignature \u0002, _IVariable \u0003)
		{
			return (\u0003.CompiledType.Class != TypeClass.Userdef || (\u0003.CompiledType as _IUserdefType).SignatureId != \u0002.Id) && (\u0003.CompiledType.Class != TypeClass.Enum || (\u0003.CompiledType as _IEnumType).SignatureId != \u0002.Id) && (\u0003.CompiledType.Class != TypeClass.Array || \u0084.\u0004.\u0001(\u0003.CompiledType).Class != TypeClass.Userdef || (\u0084.\u0004.\u0001(\u0003.CompiledType) as _IUserdefType).SignatureId != \u0002.Id) && (\u0003.OriginalType == null || \u0003.OriginalType.Class != TypeClass.Userdef || (\u0003.OriginalType as _IUserdefType).SignatureId != \u0002.Id) && (\u0003.OriginalType.Class != TypeClass.Enum || (\u0003.OriginalType as _IEnumType).SignatureId != \u0002.Id) && (\u0003.OriginalType.Class != TypeClass.Array || \u0084.\u0004.\u0002(\u0003.OriginalType).Class != TypeClass.Userdef || (\u0084.\u0004.\u0002(\u0003.OriginalType) as _IUserdefType).SignatureId != \u0002.Id);
		}

		// Token: 0x060033F6 RID: 13302 RVA: 0x000CC57C File Offset: 0x000CA77C
		private void \u0002(_ISignature \u0002, _ISignature \u0003)
		{
			foreach (IVariable variable in \u0002.AllConstants.ToArray<IVariable>())
			{
				IVariable variable2 = \u0003[variable.Id];
				if (variable2 != null && !Helper.\u0001(this.\u0001, this.\u0002, \u0002, variable2 as _IVariable, variable as _IVariable))
				{
					if (variable.Initial != null)
					{
						((_IVariable)variable).SetFlag(VarFlag.OnlChangeInit, true);
					}
					LDictionary<string, string> ldictionary = new LDictionary<string, string>();
					ldictionary.Add(variable.Name, variable.Name);
					this.\u0001(variable, ldictionary);
				}
			}
		}

		// Token: 0x060033F7 RID: 13303 RVA: 0x000CC618 File Offset: 0x000CA818
		private void \u0001(IVariable \u0002, LDictionary<string, string> \u0003)
		{
			foreach (ICrossReference crossReference in \u0002.CrossReferences)
			{
				_ISignature isignature = this.\u0001[crossReference.CodeId];
				if (isignature != null)
				{
					bool flag = false;
					foreach (IVariable u in isignature.AllVariables)
					{
						flag = global::\u0001.\u0010.\u0001(\u0003, u);
						if (flag)
						{
							break;
						}
					}
					if (flag)
					{
						_ISignature u2 = null;
						if (this.\u0002 != null)
						{
							u2 = this.\u0002[isignature.Id];
						}
						this.\u0001(isignature, u2);
					}
				}
			}
		}

		// Token: 0x060033F8 RID: 13304 RVA: 0x000CC6D8 File Offset: 0x000CA8D8
		internal void \u0002()
		{
			LHashSet<int> lhashSet = new LHashSet<int>();
			foreach (_ISignature isignature in this.\u0001.AllFlat)
			{
				_ISignature isignature2 = this.\u0002[isignature.Id];
				if (isignature2 != null && isignature2.SubSignatures.Length != isignature.SubSignatures.Length)
				{
					isignature.SetFlag(SignatureFlag.FunctionTableChanged, true);
				}
				if (isignature2 == null)
				{
					_ISignature isignature3 = this.\u0001[isignature.ParentSignatureId] as _ISignature;
					if (isignature3 != null && isignature3.POUType != Operator.Interface && !isignature3.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
					{
						isignature3.SetFlag(SignatureFlag.FunctionTableChanged, true);
					}
				}
				if (isignature.BaseSignatureId != Helper.InvalidId)
				{
					ISignature signature = this.\u0001[isignature.BaseSignatureId];
					if (signature != null && signature.GetFlag(SignatureFlag.FunctionTableChanged))
					{
						isignature.SetFlag(SignatureFlag.FunctionTableChanged, true);
					}
					if (isignature.InterfaceIds.Length != 0 && signature != null)
					{
						ISignature signature2 = this.\u0002[signature.Id];
						if (signature2 == null || signature2.Size != signature.Size)
						{
							isignature.SetFlag(SignatureFlag.FunctionTableChanged, true);
							isignature.SetFlag(SignatureFlag.InitializeVirtualFunctionTable, true);
						}
					}
				}
				if (isignature2 != null && isignature2.BaseSignatureId != isignature.BaseSignatureId)
				{
					isignature.SetFlag(SignatureFlag.FunctionTableChanged, true);
				}
				foreach (int num in isignature.InterfaceIds)
				{
					if (lhashSet.Contains(num))
					{
						if (isignature.POUType != Operator.Interface && !isignature.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
						{
							isignature.SetFlag(SignatureFlag.FunctionTableChanged, true);
						}
						else
						{
							lhashSet.Add(isignature.Id);
						}
					}
				}
				if (isignature2 != null)
				{
					if (isignature2.InterfaceIds.Count<int>() != isignature.InterfaceIds.Count<int>())
					{
						isignature.SetFlag(SignatureFlag.FunctionTableChanged, true);
						lhashSet.Add(isignature.Id);
					}
					else
					{
						for (int j = 0; j < isignature.InterfaceIds.Count<int>(); j++)
						{
							if (isignature.InterfaceIds[j] != isignature2.InterfaceIds[j])
							{
								isignature.SetFlag(SignatureFlag.FunctionTableChanged, true);
								lhashSet.Add(isignature.Id);
							}
						}
					}
				}
				if (isignature.POUType == Operator.Interface && isignature2 != null && isignature.SubSignatures.Length != isignature2.SubSignatures.Length)
				{
					lhashSet.Add(isignature.Id);
				}
				if (isignature.GetFlag(SignatureFlag.FunctionTableChanged))
				{
					foreach (int nId in isignature.DeclarerIds)
					{
						_ISignature isignature4 = this.\u0001[nId] as _ISignature;
						if (isignature4 != null)
						{
							isignature4.SetFlag(SignatureFlag.FunctionTableChanged, true);
						}
					}
				}
			}
		}

		// Token: 0x060033F9 RID: 13305 RVA: 0x000CC9EC File Offset: 0x000CABEC
		private static bool \u0001(_ISignature \u0002, _ISignature \u0003)
		{
			foreach (IVariable variable in \u0002.AllLazy)
			{
				_IVariable ivariable = (_IVariable)variable;
				_IVariable ivariable2 = \u0003[ivariable.Id] as _IVariable;
				if (ivariable2 == null)
				{
					return false;
				}
				if (!ivariable._Type.IsEqual(ivariable2._Type))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060033FA RID: 13306 RVA: 0x000CCA6C File Offset: 0x000CAC6C
		private static bool \u0001(LDictionary<string, string> \u0002, IVariable \u0003)
		{
			return global::\u0001.\u0010.\u0001(\u0002, \u0003.Type) || (\u0003.HasFlag(VarFlag.ReplacedConstant | VarFlag.Constant) && \u0003.Initial != null && global::\u0002.\u0004.\u0001(((_IVariable)\u0003)._Initial, \u0002, false));
		}

		// Token: 0x060033FB RID: 13307 RVA: 0x000CCAA8 File Offset: 0x000CACA8
		private static bool \u0001(LDictionary<string, string> \u0002, IType \u0003)
		{
			if (\u0003.Class == TypeClass.Array)
			{
				_IArrayType iarrayType = \u0003 as _IArrayType;
				foreach (_IArrayDimension iarrayDimension in iarrayType._Dimensions)
				{
					if (global::\u0002.\u0004.\u0001(iarrayDimension._LowerBorder, \u0002, false) || global::\u0002.\u0004.\u0001(iarrayDimension._UpperBorder, \u0002, false))
					{
						return true;
					}
				}
				if (global::\u0001.\u0010.\u0001(\u0002, iarrayType.BaseType))
				{
					return true;
				}
				return false;
			}
			if (\u0003.Class == TypeClass.String)
			{
				if (global::\u0002.\u0004.\u0001((\u0003 as _IStringType).Length, \u0002, false))
				{
					return true;
				}
			}
			else if (\u0003.Class == TypeClass.WString && global::\u0002.\u0004.\u0001((\u0003 as _IWStringType).Length, \u0002, false))
			{
				return true;
			}
			return false;
		}

		// Token: 0x04000A09 RID: 2569
		private readonly _ICompileContext \u0001;

		// Token: 0x04000A0A RID: 2570
		private readonly _ICompileContext \u0002;

		// Token: 0x04000A0B RID: 2571
		private readonly IScope5 \u0001;

		// Token: 0x04000A0C RID: 2572
		private readonly IScope5 \u0002;
	}
}

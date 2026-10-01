using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class ExportSignatureCollector
	{
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-77832")]
		internal LList<ISignature> SortByDependencies(IList<ISignature> alSigns, ICompileContext comcon, bool bExportLibTypes)
		{
			SortedDictionary<string, ISignature> sortedDictionary = new SortedDictionary<string, ISignature>();
			ArrayList arrayList = new ArrayList(alSigns.Count);
			Hashtable htAlreadyInserted = new Hashtable(alSigns.Count);
			foreach (ISignature item in alSigns.OrderBy((ISignature sign) => sign.Name))
			{
				if (item.HasAttribute(RTSExportAttributes.ATTRIBUTE_M4EXPORT_HIDE))
				{
					continue;
				}
				if (item.GetFlag(SignatureFlag.Enum))
				{
					sortedDictionary.Add(item.OrgName, item);
					continue;
				}
				bool flag = false;
				if (Operator.Function == item.POUType)
				{
					ICompiledPOU compiledPOU = comcon.GetCompiledPOU(item.ObjectGuid);
					if (compiledPOU != null)
					{
						flag = compiledPOU.GetFlag(CompiledPOUFlags.TopLevel);
					}
				}
				bool flag2 = item.GetFlag(SignatureFlag.External) || item.HasAttribute(RTSExportAttributes.ATTRIBUTE_M4EXPORT) || flag;
				bool num = item.GetFlag(SignatureFlag.Structure) || item.GetFlag(SignatureFlag.Union) || item.GetFlag(SignatureFlag.Alias) || item.POUType == Operator.Interface;
				bool flag3 = item.POUType == Operator.Program || item.POUType == Operator.VarGlobal || item.POUType == Operator.FunctionBlock || item.POUType == Operator.Function || item.POUType == Operator.Method;
				if (num || (flag3 && flag2))
				{
					IScope2 scope = comcon.CreateIScope(item.Id) as IScope2;
					SortedInsert(item, arrayList, htAlreadyInserted, scope, bExportLibTypes);
				}
			}
			LList<ISignature> val = new LList<ISignature>(arrayList.Count);
			foreach (KeyValuePair<string, ISignature> item2 in sortedDictionary)
			{
				val.Add(item2.Value);
			}
			foreach (ISignature item3 in arrayList)
			{
				if (!val.Contains(item3))
				{
					val.Add(item3);
				}
			}
			return val;
		}

		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-77832")]
		internal bool SortedInsert(ISignature sign, ArrayList alSignNew, Hashtable htAlreadyInserted, IScope2 scope, bool bExportLibTypes)
		{
			if (htAlreadyInserted[sign.Id] != null)
			{
				return true;
			}
			htAlreadyInserted.Add(sign.Id, sign);
			if (!bExportLibTypes && !string.IsNullOrEmpty(sign.LibraryPath))
			{
				return true;
			}
			ISignature signature = scope[sign.BaseSignatureId];
			if (signature != null && !SortedInsert(signature, alSignNew, htAlreadyInserted, scope, bExportLibTypes))
			{
				return false;
			}
			int[] interfaceIds = sign.InterfaceIds;
			foreach (int nId in interfaceIds)
			{
				ISignature signature2 = scope[nId];
				if (signature2 != null && !SortedInsert(signature2, alSignNew, htAlreadyInserted, scope, bExportLibTypes))
				{
					return false;
				}
			}
			foreach (IVariable2 item in sign.All.OfType<IVariable2>())
			{
				if (!item.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY))
				{
					ICompiledType compiledType = item.CompiledType;
					if (compiledType.Class == TypeClass.Array)
					{
						compiledType = GetRealBaseType(compiledType as IArrayType);
					}
					compiledType = GetRealBaseType(compiledType);
					ISignature signature3 = null;
					if (compiledType.Class == TypeClass.Userdef)
					{
						IUserdefType userdefType = compiledType as IUserdefType;
						signature3 = scope[userdefType.SignatureId];
					}
					else if (compiledType.Class == TypeClass.Enum)
					{
						IEnumType2 enumType = compiledType as IEnumType2;
						signature3 = scope[enumType.SignatureId];
					}
					if (signature3 != null && !SortedInsert(signature3, alSignNew, htAlreadyInserted, scope, bExportLibTypes))
					{
						return false;
					}
				}
			}
			if (sign.HasAttribute(RTSExportAttributes.ATTRIBUTE_M4EXPORT_HIDE))
			{
				return true;
			}
			alSignNew.Add(sign);
			return true;
		}

		private static ICompiledType GetRealBaseType(ICompiledType type)
		{
			if (type.Class == TypeClass.Pointer)
			{
				return GetRealBaseType(type as IPointerType);
			}
			if (type.Class == TypeClass.Array)
			{
				return GetRealBaseType(type as IArrayType);
			}
			if (TypeClass.Reference == type.Class)
			{
				return GetRealBaseType((IReferenceType)type);
			}
			return type;
		}

		private static ICompiledType GetRealBaseType(IPointerType ptrtype)
		{
			if (ptrtype.Base is IPointerType ptrtype2)
			{
				return GetRealBaseType(ptrtype2);
			}
			return GetRealBaseType(ptrtype.Base as ICompiledType);
		}

		private static ICompiledType GetRealBaseType(IArrayType arrtype)
		{
			if (arrtype.Base is IArrayType arrtype2)
			{
				return GetRealBaseType(arrtype2);
			}
			return GetRealBaseType(arrtype.Base as ICompiledType);
		}

		private static ICompiledType GetRealBaseType(IReferenceType reftype)
		{
			if (reftype.Base is IReferenceType reftype2)
			{
				return GetRealBaseType(reftype2);
			}
			return GetRealBaseType(reftype.Base as ICompiledType);
		}
	}
}

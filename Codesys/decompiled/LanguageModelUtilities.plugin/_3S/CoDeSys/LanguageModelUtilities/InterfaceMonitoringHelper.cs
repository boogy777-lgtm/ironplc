using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Online;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[TypeGuid("{947B44C1-1922-4FDE-8775-93CDBD45CEEC}")]
	public class InterfaceMonitoringHelper : IInterfaceMonitoringHelper
	{
		private Tuple<IArea, ulong>[] _allDataAreaStartAddresses;

		public void ReadAllDataAreaAddresses(Guid guidApplication)
		{
			if (!(APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(guidApplication) is ICompileContext12 compileContext))
			{
				_allDataAreaStartAddresses = new Tuple<IArea, ulong>[0];
				return;
			}
			LList<Tuple<IArea, ulong>> val = new LList<Tuple<IArea, ulong>>();
			IArea[] areas = compileContext.GetDataManager().Areas;
			foreach (IArea area in areas)
			{
				if ((area.Flags & DataSegmentFlags.Data) != 0 && area.Size < int.MaxValue && ReadAreaAddressFromService(area.Index, out var areaStartAddress, guidApplication))
				{
					val.Add(new Tuple<IArea, ulong>(area, areaStartAddress));
				}
			}
			_allDataAreaStartAddresses = val.ToArray();
		}

		public void DiscardAllDataAreaAddresses()
		{
			_allDataAreaStartAddresses = null;
		}

		public bool GetAreaOffsetForAbsoluteAddess(Guid guidApplication, ulong ulAddress, out uint uiArea, out uint uiOffset)
		{
			uiArea = uint.MaxValue;
			uiOffset = uint.MaxValue;
			if (_allDataAreaStartAddresses == null)
			{
				ReadAllDataAreaAddresses(guidApplication);
			}
			Tuple<IArea, ulong>[] allDataAreaStartAddresses = _allDataAreaStartAddresses;
			foreach (Tuple<IArea, ulong> tuple in allDataAreaStartAddresses)
			{
				if (ulAddress >= tuple.Item2 && ulAddress < (ulong)((long)tuple.Item2 + (long)tuple.Item1.Size))
				{
					uiArea = (uint)tuple.Item1.Index;
					uiOffset = (uint)(ulAddress - tuple.Item2);
					return true;
				}
			}
			return false;
		}

		public string GetInterfaceInstancePath(Guid guidApplication, object interfaceValue, out bool bHidden)
		{
			uint uiArea = uint.MaxValue;
			uint uiOffset = uint.MaxValue;
			bHidden = false;
			if (!GetInterfaceAreaOffset(out uiArea, out uiOffset, guidApplication, interfaceValue))
			{
				return string.Empty;
			}
			return GetInstancePathAtAreaOffset(guidApplication, uiArea, uiOffset, out bHidden);
		}

		internal static int GetPointerSize(Guid guidApplication)
		{
			if (!(APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(guidApplication) is ICompileContext10 compileContext))
			{
				return 4;
			}
			return compileContext.PointerSize;
		}

		private static string GetInstancePathAtLocalOffsetUserdefType(string stInstancePathSoFar, ICompileContext comcon, IScope scope, ISignature signUserdefType, uint uiOffset, out bool bHidden)
		{
			bHidden = false;
			while (signUserdefType != null)
			{
				IVariable[] all = signUserdefType.All;
				foreach (IVariable variable in all)
				{
					IScope scope2 = comcon.CreateIScope(signUserdefType.Id);
					if (!variable.GetFlag(VarFlag.RelativeInstance))
					{
						continue;
					}
					IDataLocation dataLocation = variable.DataLocation;
					if (dataLocation.Offset <= uiOffset && dataLocation.Offset + variable.CompiledType.Size(scope) > uiOffset)
					{
						if (IsHiddenVariable(signUserdefType, variable))
						{
							bHidden = true;
							return string.Empty;
						}
						return GetInstancePathAtLocalOffset(stInstancePathSoFar, comcon, scope2, signUserdefType, variable, (uint)(uiOffset - dataLocation.Offset), out bHidden);
					}
				}
				signUserdefType = scope[signUserdefType.BaseSignatureId];
			}
			return string.Empty;
		}

		private static string GetInstancePathAtLocalOffset(string stInstancePathSoFar, ICompileContext comcon, IScope scope, ISignature sign, IVariable var, uint uiOffset, out bool bHidden)
		{
			string result = string.Empty;
			string text = string.Join(".", stInstancePathSoFar, var.OrgName);
			bHidden = false;
			if (uiOffset == 0)
			{
				result = stInstancePathSoFar;
			}
			else if (var.Type is IUserdefType)
			{
				IUserdefType userdefType = var.Type as IUserdefType;
				ISignature signature = scope[userdefType.SignatureId];
				if (signature != null)
				{
					result = GetInstancePathAtLocalOffsetUserdefType(text, comcon, scope, signature, uiOffset, out bHidden);
				}
			}
			else if (var.Type is IArrayType)
			{
				IArrayType arrayType = var.Type as IArrayType;
				if (arrayType.Base is IUserdefType)
				{
					IUserdefType userdefType2 = arrayType.Base as IUserdefType;
					ISignature signature2 = scope[userdefType2.SignatureId];
					if (signature2 != null && !APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(signature2, GUIHidingFlags.AllCommon))
					{
						uint num = uiOffset / (uint)signature2.Size;
						uiOffset -= (uint)((int)num * signature2.Size);
						string indexAccesses = GetIndexAccesses(arrayType, num, scope);
						text += $"[{indexAccesses}]";
						result = GetInstancePathAtLocalOffsetUserdefType(text, comcon, scope, signature2, uiOffset, out bHidden);
					}
				}
			}
			return result;
		}

		private static string GetIndexAccesses(IArrayType artype, uint uiLinearizedArrayIndex, IScope scope)
		{
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Expected O, but got Unknown
			IScope5 scope2 = scope as IScope5;
			long num = uiLinearizedArrayIndex;
			int num2 = artype.Dimensions.Length;
			long[] array = new long[num2];
			long[] array2 = new long[num2];
			long[] array3 = new long[num2];
			for (int i = 0; i < num2; i++)
			{
				array[i] = GetLowerBorder(artype, i, scope);
				array2[i] = GetIndexRange(artype, i, scope);
			}
			for (int j = 0; j < num2; j++)
			{
				long num3 = 1L;
				for (int k = j + 1; k < num2; k++)
				{
					num3 *= array2[k];
				}
				array3[j] = num / num3;
				num -= array3[j] * num3;
			}
			LStringBuilder val = new LStringBuilder();
			for (int l = 0; l < num2; l++)
			{
				if (0 < l)
				{
					val.Append(",");
				}
				long num4 = array[l] + array3[l];
				bool flag = false;
				if (artype.Dimensions[l].LowerBorder.Type is IEnumType etype && scope2 != null)
				{
					ISignature signature = scope2.FindSignature(etype);
					if (signature != null)
					{
						IVariable[] all = signature.All;
						int num5 = 0;
						while (!flag && num5 < all.Length)
						{
							if (!(all[num5].Initial is ILiteralExpression literalExpression) || literalExpression.LiteralValue == null)
							{
								continue;
							}
							if (LiteralAsLong(literalExpression.LiteralValue) == num4)
							{
								flag = true;
								if (signature.HasAttribute(CompileAttributes.ATTRIBUTE_QUALIFIED_ONLY))
								{
									val.Append(signature.OrgName).Append(".");
								}
								val.Append(all[num5].OrgName);
							}
							else
							{
								num5++;
							}
						}
					}
				}
				if (!flag)
				{
					val.Append(num4);
				}
			}
			return ((object)val).ToString();
		}

		private static long LiteralAsLong(ILiteralValue litVal)
		{
			long result = 0L;
			switch (litVal.KindOf)
			{
			case KindOfLiteral.UnsignedInteger:
				result = (long)litVal.UnsignedLong;
				break;
			case KindOfLiteral.SignedInteger:
				result = litVal.SignedLong;
				break;
			}
			return result;
		}

		private static long GetLowerBorder(IArrayType artype, int iWhichDimension, IScope scope)
		{
			long result = 0L;
			if (0 <= iWhichDimension && iWhichDimension < artype.Dimensions.Length)
			{
				bool bValid = false;
				result = artype.Dimensions[iWhichDimension].LowerBorderInt(out bValid, scope);
			}
			return result;
		}

		private static long GetIndexRange(IArrayType artype, int iWhichDimension, IScope scope)
		{
			long result = 0L;
			if (0 <= iWhichDimension && iWhichDimension < artype.Dimensions.Length)
			{
				bool bValid = false;
				result = artype.Dimensions[iWhichDimension].Range(out bValid, scope);
			}
			return result;
		}

		private static bool IsHiddenVariable(ISignature sign, IVariable var)
		{
			GUIHidingFlags flagsToConsider = GUIHidingFlags.EvaluateAttributes;
			if (!APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(sign, flagsToConsider))
			{
				return APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenVariable(sign as ISignature6, var, flagsToConsider);
			}
			return true;
		}

		private static string GetInstancePathAtAreaOffset(Guid guidApplication, uint uiArea, uint uiOffset, out bool bHidden)
		{
			ICompileContext referenceContextIfAvailable = APEnvironmentFacade.Instance.LanguageModelMgr.GetReferenceContextIfAvailable(guidApplication);
			bHidden = false;
			ISignature[] allSignatures = referenceContextIfAvailable.AllSignatures;
			foreach (ISignature signature in allSignatures)
			{
				IScope scope = referenceContextIfAvailable.CreateIScope(signature.Id);
				IVariable[] all = signature.All;
				foreach (IVariable variable in all)
				{
					if (!variable.GetFlag(VarFlag.Absolut))
					{
						continue;
					}
					IDataLocation dataLocation = variable.DataLocation;
					if (dataLocation != null && variable.CompiledType != null && dataLocation.Area == uiArea && dataLocation.Offset <= uiOffset && dataLocation.Offset + variable.CompiledType.Size(scope) > uiOffset)
					{
						if (IsHiddenVariable(signature, variable))
						{
							bHidden = true;
							return string.Empty;
						}
						return GetInstancePathAtLocalOffset(signature.OrgName, referenceContextIfAvailable, scope, signature, variable, (uint)(uiOffset - dataLocation.Offset), out bHidden);
					}
				}
			}
			return string.Empty;
		}

		private bool GetInterfaceAreaOffset(out uint uiArea, out uint uiOffset, Guid guidApplication, object interfaceValue)
		{
			uiArea = uint.MaxValue;
			uiOffset = uint.MaxValue;
			int pointerSize = GetPointerSize(guidApplication);
			ulong num = 0uL;
			if (pointerSize == 4 && interfaceValue is uint)
			{
				num = (uint)interfaceValue;
			}
			else
			{
				if (pointerSize != 8 || !(interfaceValue is ulong))
				{
					return false;
				}
				num = (ulong)interfaceValue;
			}
			return GetAreaOffsetForAbsoluteAddess(guidApplication, num, out uiArea, out uiOffset);
		}

		private bool ReadAreaAddressFromService(int area, out ulong areaStartAddress, Guid guidApplication)
		{
			int pointerSize = GetPointerSize(guidApplication);
			bool result = true;
			areaStartAddress = 0uL;
			if (APEnvironmentFacade.Instance.GetRuntimeVersion(guidApplication) < new Version(3, 5, 5, 0))
			{
				return false;
			}
			IOnlineApplication22 onlineApplication = APEnvironmentFacade.Instance.GetOnlineApplication(guidApplication);
			if (onlineApplication == null || !onlineApplication.IsLoggedIn)
			{
				return false;
			}
			try
			{
				areaStartAddress = onlineApplication.GetAreaStartAddress(pointerSize, area);
				return result;
			}
			catch
			{
				return false;
			}
		}

		internal static ICompiledType CreateType(string stType)
		{
			IScanner scanner = APEnvironmentFacade.Instance.LMServiceProvider.CreatorService.CreateScanner(stType, bIncludeComments: false, bIncludeEndOfLines: false, bIncludePragmas: false, bIncludeWhitespaces: false);
			return (APEnvironmentFacade.Instance.LMServiceProvider.CreatorService.CreateParser(scanner) as IParser2).ParseTypeDeclaration();
		}
	}
}

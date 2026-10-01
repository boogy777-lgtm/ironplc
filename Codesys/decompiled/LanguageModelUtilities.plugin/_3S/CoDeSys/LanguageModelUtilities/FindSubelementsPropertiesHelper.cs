using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class FindSubelementsPropertiesHelper
	{
		internal FindSubelementsFlags Flags { get; set; }

		internal LDictionary<string, string> HiddenMethods { get; set; }

		internal SubelementItemCollection Result { get; set; }

		internal bool IncludeLocalVars { get; set; }

		internal bool IsBaseSignature { get; set; }

		internal Guid ApplicationGuid { get; set; }

		internal ISignature Signature { get; set; }

		internal ISignature[] SubSignatures { get; set; }

		internal ISignature SignatureAccessing { get; set; }

		private static bool VarIsProperty(IVariable var)
		{
			if (var.HasAttribute(CompileAttributes.GET_ACCESS))
			{
				return true;
			}
			if (var.HasAttribute(CompileAttributes.SET_ACCESS))
			{
				return true;
			}
			if (var.HasAttribute(CompileAttributes.DEVICE_PARAMETER))
			{
				return true;
			}
			return var.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY);
		}

		internal void CollectProperties(IVariable[] varLocals)
		{
			if (varLocals != null)
			{
				foreach (IVariable var in varLocals)
				{
					CollectProperty(var);
				}
			}
		}

		private void CollectProperty(IVariable var)
		{
			bool flag = (Flags & FindSubelementsFlags.ExcludeProperties) == FindSubelementsFlags.ExcludeProperties;
			bool bExcludePropertyGetter = (Flags & FindSubelementsFlags.ExcludePropertyGetter) == FindSubelementsFlags.ExcludePropertyGetter;
			bool bExcludePropertySetter = (Flags & FindSubelementsFlags.ExcludePropertySetter) == FindSubelementsFlags.ExcludePropertySetter;
			bool flag2 = VarIsProperty(var);
			if (!(flag2 && flag) && (!flag2 || ExistsAtLeastOneVisiblePropertyAccessor(var, bExcludePropertyGetter, bExcludePropertySetter)) && (IncludeLocalVars || flag2 || Signature.POUType == Operator.Program))
			{
				if (!Result.Contains(var.OrgName) && !APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.IsHiddenVariable(Signature as ISignature6, var, GUIHidingFlags.EvaluateAttributes, SignatureAccessing) && (!flag2 || !HiddenMethods.ContainsKey(var.OrgName)))
				{
					Result.Add(new SubelementItem(var.OrgName, new IdentifierInfo(var, Signature, var.OrgName, var.Comment, IdentifierInfoFlag.Variable | IdentifierInfoFlag.Local, var.Type)));
				}
				else if (flag2 && !HiddenMethods.ContainsKey(var.OrgName))
				{
					HiddenMethods.Add(var.OrgName, "");
				}
			}
		}

		private void DeterminePropertyGetterAndSetter(IVariable var, bool bExcludePropertyGetter, bool bExcludePropertySetter, out ISignature signSubGet, out ISignature signSubSet)
		{
			signSubGet = null;
			signSubSet = null;
			if (SubSignatures == null)
			{
				return;
			}
			string a = "__get" + var.OrgName;
			string a2 = "__set" + var.OrgName;
			ISignature[] subSignatures = SubSignatures;
			foreach (ISignature signature in subSignatures)
			{
				if (string.Equals(a, signature.OrgName, StringComparison.OrdinalIgnoreCase))
				{
					signSubGet = signature;
				}
				else if (string.Equals(a2, signature.OrgName, StringComparison.OrdinalIgnoreCase))
				{
					signSubSet = signature;
				}
			}
			if (bExcludePropertyGetter)
			{
				signSubGet = null;
			}
			if (bExcludePropertySetter)
			{
				signSubSet = null;
			}
		}

		private bool IsAccessFromDerivedFunctionblock()
		{
			ISignature2 baseSignature = APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.GetBaseSignature((ISignature2)SignatureAccessing, ApplicationGuid);
			if (baseSignature == null)
			{
				return false;
			}
			return baseSignature.ObjectGuid == Signature.ObjectGuid;
		}

		private bool IsPropertyAccessorInvisible(ISignature signSubAccessor)
		{
			if (signSubAccessor == null)
			{
				return true;
			}
			if (!string.IsNullOrEmpty(signSubAccessor.LibraryPath) && signSubAccessor.GetFlag(SignatureFlag.Internal))
			{
				return true;
			}
			if (signSubAccessor.GetFlag(SignatureFlag.Private))
			{
				return Signature.ObjectGuid != SignatureAccessing.ObjectGuid;
			}
			if (signSubAccessor.GetFlag(SignatureFlag.Protected))
			{
				if (IsAccessFromDerivedFunctionblock())
				{
					return false;
				}
				if (IsBaseSignature)
				{
					return false;
				}
				return Signature.ObjectGuid != SignatureAccessing.ObjectGuid;
			}
			return false;
		}

		private bool AllPropertyAccessorsInvisible(ISignature signSubGet, ISignature signSubSet)
		{
			if (IsPropertyAccessorInvisible(signSubGet))
			{
				return IsPropertyAccessorInvisible(signSubSet);
			}
			return false;
		}

		private bool ExistsAtLeastOneVisiblePropertyAccessor(IVariable var, bool bExcludePropertyGetter, bool bExcludePropertySetter)
		{
			DeterminePropertyGetterAndSetter(var, bExcludePropertyGetter, bExcludePropertySetter, out var signSubGet, out var signSubSet);
			return !AllPropertyAccessorsInvisible(signSubGet, signSubSet);
		}
	}
}

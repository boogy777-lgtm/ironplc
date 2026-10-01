using System;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class FindSubelementsHelper
	{
		private readonly Guid _guidSignature;

		private readonly IPreCompileContext11 _precom;

		private readonly string _stAccessPathOrType;

		private static bool ShowSystemSymbolsProgrammingFeatures => APEnvironmentFacade.Instance.GetFeatureSettingValue("smart-coding", "show-system-symbols", bDefaultValue: false);

		internal FindSubelementsHelper(Guid guidSignature, IPreCompileContext11 precom, string stAccessPathOrType)
		{
			_guidSignature = guidSignature;
			_precom = precom;
			_stAccessPathOrType = stAccessPathOrType;
		}

		internal IIdentifierInfo[] FindSubelements(FindSubelementsFlags flags, out bool bError)
		{
			bError = false;
			IIdentifierInfo[] array = FindSubelementsThisOrSuper(out var stTrimmedUppercaseAccessPathOrType);
			if (array != null)
			{
				return array;
			}
			IPrecompileScope6 pscope = _precom.CreatePrecompileScope(_guidSignature) as IPrecompileScope6;
			ISignature signature = _precom.GetSignature(_guidSignature);
			flags = EnrichFlagsForThisOrSuper(flags, signature, stTrimmedUppercaseAccessPathOrType);
			if (_stAccessPathOrType == string.Empty || _stAccessPathOrType == ".")
			{
				return GetAllDeclarations(flags, pscope);
			}
			IExpression expression = ParseAccessPathOrType();
			ISignature signHelp;
			ICompiledType ctype;
			if ((flags & FindSubelementsFlags.SearchForType) == 0)
			{
				array = FindSubelementsNotSearchForType(flags, _precom.ApplicationGuid, ref pscope, out bError, out signHelp, out ctype);
				if (array != null)
				{
					return array;
				}
			}
			else
			{
				ctype = ((ILanguageModelBuilder5)APEnvironmentFacade.Instance.LanguageModelMgr.CreateLanguageModelBuilder()).ParseType(_stAccessPathOrType);
				if (ctype == null)
				{
					bError = true;
				}
				signHelp = ((expression != null) ? pscope.FindSignatureGlobal(expression) : null);
			}
			if (signHelp == null)
			{
				return FindSubelementsArrayHelper.FindSubelementsArrayType(flags, pscope, ctype);
			}
			SubelementItemsCollector subelementItemsCollector = new SubelementItemsCollector(_precom);
			subelementItemsCollector.FillSubelements(signHelp, signature, pscope, flags, bIsBaseSignature: false);
			return subelementItemsCollector.GetCollectedSubElements();
		}

		private IExpression ParseAccessPathOrType()
		{
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(_stAccessPathOrType, bIncludeComments: false, bIncludeEndOfLines: false, bIncludePragmas: false, bIncludeWhitespaces: false);
			return ((IParser4)APEnvironmentFacade.Instance.LanguageModelMgr.CreateParser(scanner)).ParseOperand();
		}

		private IIdentifierInfo[] FindSubelementsNotSearchForType(FindSubelementsFlags flags, Guid guidApplication, ref IPrecompileScope6 pscope, out bool bError, out ISignature signHelp, out ICompiledType ctype)
		{
			signHelp = null;
			ctype = null;
			_precom.DeriveAccessPathInformation(_guidSignature, APEnvironmentFacade.Instance.PrimaryProjectHandle, _stAccessPathOrType, out bError, out var derivedScope, out var derivedSignature, out var derivedVariable, out var derivedType, out var searchScope);
			if (bError)
			{
				return Array.Empty<IIdentifierInfo>();
			}
			if (derivedScope != null)
			{
				EWhichDeclarations eWhichDeclaration = ((!SmartCodingOptionsHelper.ShowSymbolsOfSubLibraries) ? EWhichDeclarations.Locals : (EWhichDeclarations.Locals | EWhichDeclarations.Namespaces | EWhichDeclarations.POUsFromSubLibraries));
				IIdentifierInfo[] array = ((!(derivedScope is IPrecompileScope8 precompileScope)) ? derivedScope.GetAllDeclarations() : precompileScope.GetAllDeclarations(eWhichDeclaration).ToArray());
				if (_stAccessPathOrType.Equals("VisuElems", StringComparison.OrdinalIgnoreCase))
				{
					return WorkaroundAddVisuElemBaseIdentifiersIfNecessary(array);
				}
				return array;
			}
			ctype = derivedType as ICompiledType;
			return new SingatureFinder(guidApplication, _precom, _stAccessPathOrType, flags, ctype, searchScope).FindSignature(ref pscope, ref signHelp, derivedSignature, derivedVariable);
		}

		private IIdentifierInfo[] GetAllDeclarations(FindSubelementsFlags flags, IPrecompileScope6 pscope)
		{
			if ((flags & FindSubelementsFlags.SearchForType) != 0)
			{
				return Array.Empty<IIdentifierInfo>();
			}
			if (_stAccessPathOrType == ".")
			{
				return pscope.GetAllDeclarations(bLocals: false, ShowSystemSymbolsProgrammingFeatures);
			}
			return pscope.GetAllDeclarations();
		}

		private IIdentifierInfo[] FindSubelementsThisOrSuper(out string stTrimmedUppercaseAccessPathOrType)
		{
			stTrimmedUppercaseAccessPathOrType = _stAccessPathOrType.Trim().ToUpperInvariant();
			if ("THIS" == stTrimmedUppercaseAccessPathOrType || "SUPER" == stTrimmedUppercaseAccessPathOrType)
			{
				return Array.Empty<IIdentifierInfo>();
			}
			return null;
		}

		private FindSubelementsFlags EnrichFlagsForThisOrSuper(FindSubelementsFlags flags, ISignature sign, string stTrimmedUppercaseAccessPathOrType)
		{
			if (sign != null && (sign.POUType == Operator.FunctionBlock || sign.POUType == Operator.Method || Operator.Action == sign.POUType) && (stTrimmedUppercaseAccessPathOrType == "THIS^" || stTrimmedUppercaseAccessPathOrType == "SUPER^"))
			{
				flags |= FindSubelementsFlags.IncludeLocalVars;
			}
			return flags;
		}

		private IIdentifierInfo[] WorkaroundAddVisuElemBaseIdentifiersIfNecessary(IIdentifierInfo[] orgResult)
		{
			if (orgResult == null || orgResult.Length == 0)
			{
				return orgResult;
			}
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionGreaterEq(3, 5, 14, 0))
			{
				return orgResult;
			}
			_precom.DeriveAccessPathInformation(_guidSignature, APEnvironmentFacade.Instance.PrimaryProjectHandle, "VisuElems.VisuElemBase", out var bError, out var derivedScope, out var _, out var _, out var _, out var _);
			if (derivedScope == null || bError)
			{
				return orgResult;
			}
			return orgResult.Concat(derivedScope.GetAllDeclarations()).ToArray();
		}
	}
}

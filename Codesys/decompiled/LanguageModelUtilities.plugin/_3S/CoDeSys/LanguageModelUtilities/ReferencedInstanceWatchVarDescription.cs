using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	public abstract class ReferencedInstanceWatchVarDescription : IReferencedInstanceWatchVarDescription
	{
		private const string WATCHVAR_GVL = "__WatchVars";

		private static readonly Dictionary<char, object> s_htInvalidCharacters;

		private string _stQualifiedApplicationName;

		private ISignature _signInstance;

		private ulong _ulAddressInstance;

		private string _stTempVar;

		private ILMCompiledApplicationDebugging _lmCompiledApplicationDebugging;

		public string FullyQualifiedWatchExpression => $"{_stQualifiedApplicationName}.{PointerTempVar}^";

		public string PointerTempVar => string.Format("{0}.{1}", "__WatchVars", _stTempVar);

		public virtual string InstanceType => _signInstance?.OrgName;

		public ulong AddressInstance => _ulAddressInstance;

		static ReferencedInstanceWatchVarDescription()
		{
			s_htInvalidCharacters = new Dictionary<char, object>();
			s_htInvalidCharacters.Add('.', null);
			s_htInvalidCharacters.Add('^', null);
			s_htInvalidCharacters.Add('[', null);
			s_htInvalidCharacters.Add(']', null);
			s_htInvalidCharacters.Add('{', null);
			s_htInvalidCharacters.Add('}', null);
			s_htInvalidCharacters.Add('-', null);
			s_htInvalidCharacters.Add(' ', null);
			s_htInvalidCharacters.Add('(', null);
			s_htInvalidCharacters.Add(')', null);
			s_htInvalidCharacters.Add(',', null);
			s_htInvalidCharacters.Add('#', null);
		}

		protected void Initialize(string stQualifiedApplicationName, ulong ulAddressInstance, ISignature signInstance)
		{
			_stQualifiedApplicationName = stQualifiedApplicationName;
			_signInstance = signInstance;
			_ulAddressInstance = ulAddressInstance;
		}

		protected void Initialize(string stDevice, string stApplication, ulong ulAddressInstance, ISignature signInstance)
		{
			string stQualifiedApplicationName = $"{stDevice}.{stApplication}";
			Initialize(stQualifiedApplicationName, ulAddressInstance, signInstance);
		}

		protected string DetermineDereferencedPointerExpression(string stPointerExpression, string stInstancePath)
		{
			if (stPointerExpression.StartsWith(stInstancePath))
			{
				int num = stPointerExpression.IndexOf("__CAST");
				int num2 = 0;
				num2 = ((0 <= num) ? (stPointerExpression.Substring(0, num).LastIndexOf('.') + 1) : ((!string.IsNullOrEmpty(stInstancePath)) ? (stInstancePath.Length + 1) : 0));
				return stPointerExpression.Substring(num2) + "^";
			}
			return string.Empty;
		}

		private string MaskInvalidCharacters(string stVarName)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Expected O, but got Unknown
			bool flag = false;
			LStringBuilder val = new LStringBuilder(stVarName);
			int length = val.get_Length();
			for (int i = 0; i < length; i++)
			{
				if (s_htInvalidCharacters.ContainsKey(val.get_Item(i)))
				{
					val.set_Item(i, '_');
					flag = true;
				}
			}
			if (flag)
			{
				return ((object)val).ToString();
			}
			return stVarName;
		}

		private ICompiledType CreateType(string stType)
		{
			IScanner scanner = APEnvironmentFacade.Instance.LMServiceProvider.CreatorService.CreateScanner(stType, bIncludeComments: false, bIncludeEndOfLines: false, bIncludePragmas: false, bIncludeWhitespaces: false);
			scanner.AllowMultipleUnderlines = true;
			return (APEnvironmentFacade.Instance.LMServiceProvider.CreatorService.CreateParser(scanner) as IParser2).ParseTypeDeclaration();
		}

		protected void Initialize(Guid gdApplication, bool bAddressChanged, ISignature signInstance, string stReferencingExpression, int iTempVarCounter)
		{
			string arg = string.Empty;
			if (!string.IsNullOrEmpty(signInstance.LibraryPath) && APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(gdApplication) is ICompileContext8 compileContext)
			{
				arg = compileContext.GetLibraryNamespace(signInstance.LibraryPath) + ".";
			}
			string stType = $"POINTER TO {arg}{signInstance.OrgName}";
			ICompiledType pointerType = CreateType(stType);
			Initialize(gdApplication, bAddressChanged, pointerType, stReferencingExpression, iTempVarCounter);
		}

		protected void Initialize(Guid gdApplication, bool bAddressChanged, ICompiledType pointerType, string stReferencingExpression, int iTempVarCounter)
		{
			_lmCompiledApplicationDebugging = APEnvironmentFacade.Instance.LMServiceProvider.CompileService.GetApplicationDebugger(gdApplication);
			if (bAddressChanged && _lmCompiledApplicationDebugging is ILMCompiledApplicationDebugging2 iLMCompiledApplicationDebugging)
			{
				iLMCompiledApplicationDebugging.RemoveWatchVariable(_stTempVar);
			}
			string stVarName = stReferencingExpression + "__" + gdApplication.ToString() + "__" + iTempVarCounter;
			_stTempVar = MaskInvalidCharacters(stVarName);
			_lmCompiledApplicationDebugging.AddWatchVariable(_stTempVar, pointerType);
		}
	}
}

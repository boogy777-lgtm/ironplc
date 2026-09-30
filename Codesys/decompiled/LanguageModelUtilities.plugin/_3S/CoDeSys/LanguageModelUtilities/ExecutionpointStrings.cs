using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class ExecutionpointStrings
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (resourceMan == null)
				{
					resourceMan = new ResourceManager("_3S.CoDeSys.LanguageModelUtilities.ExecutionpointStrings", typeof(ExecutionpointStrings).Assembly);
				}
				return resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return resourceCulture;
			}
			set
			{
				resourceCulture = value;
			}
		}

		internal static string BPCodeFuncUnavailable => ResourceManager.GetString("BPCodeFuncUnavailable", resourceCulture);

		internal static string BPCodeInterfaceCallNotSupported => ResourceManager.GetString("BPCodeInterfaceCallNotSupported", resourceCulture);

		internal static string EPCode_POUNotAvailable => ResourceManager.GetString("EPCode_POUNotAvailable", resourceCulture);

		internal static string EPCode_POUNotCallable => ResourceManager.GetString("EPCode_POUNotCallable", resourceCulture);

		internal static string EPCode_PropertyNotUsable => ResourceManager.GetString("EPCode_PropertyNotUsable", resourceCulture);

		internal static string ExternalFunctionForConversionUnavailable => ResourceManager.GetString("ExternalFunctionForConversionUnavailable", resourceCulture);

		internal static string InterpreterOperatorIsNotSupported => ResourceManager.GetString("InterpreterOperatorIsNotSupported", resourceCulture);

		internal static string InterpreterOperatorNotSupportedForType => ResourceManager.GetString("InterpreterOperatorNotSupportedForType", resourceCulture);

		internal static string NoArgumentsOnTheInterpreterStack => ResourceManager.GetString("NoArgumentsOnTheInterpreterStack", resourceCulture);

		internal static string UnsupportedBreakpointCode => ResourceManager.GetString("UnsupportedBreakpointCode", resourceCulture);

		internal static string UnsupportedSizeOfVariable => ResourceManager.GetString("UnsupportedSizeOfVariable", resourceCulture);

		internal static string UnsupportedStatementInBreakpointCode => ResourceManager.GetString("UnsupportedStatementInBreakpointCode", resourceCulture);

		internal static string ValueSizeCannotBeUsed => ResourceManager.GetString("ValueSizeCannotBeUsed", resourceCulture);

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		internal ExecutionpointStrings()
		{
		}
	}
}

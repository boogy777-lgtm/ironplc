using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class XAddressInfoCreator : IAddressInfoCreator
	{
		public static IVarRef GetVarRefBase(string stInstancePath, string stExp, out string stErrorMsg)
		{
			try
			{
				IVarRef varReference = APEnvironmentFacade.Instance.LanguageModelMgr.GetVarReference(stExp, stInstancePath);
				if (varReference == null || varReference.GetFlag(VarRefFlag.Invalid))
				{
					stErrorMsg = "Invalid var reference";
					return null;
				}
				if (varReference.GetFlag(VarRefFlag.Extensible) && (varReference.WatchExpression.Type == null || varReference.WatchExpression.Type.Class != TypeClass.Pointer))
				{
					stErrorMsg = Strings.NonPrimitiveType_Error;
					return null;
				}
				stErrorMsg = "";
				return varReference;
			}
			catch (ArgumentException ex)
			{
				stErrorMsg = ex.ToString();
				return null;
			}
		}

		private static Pair<IAddressInfo, string> GetAddressInfoBase(string stInstancePath, IExpression exp)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			string stErrorMsg;
			return Fun.Pair<IAddressInfo, string>(GetVarRefBase(stInstancePath, exp.ToString(), out stErrorMsg)?.AddressInfo, stErrorMsg);
		}

		public IAddressInfo GetAddressInfo(string stInstancePath, IExpression exp, out string stErrorMsg)
		{
			AddressInfoCreator addressInfoCreator = new AddressInfoCreator(GetAddressInfoBase, stInstancePath);
			exp.AcceptVisitor(addressInfoCreator);
			return addressInfoCreator.GetResult(out stErrorMsg);
		}
	}
}

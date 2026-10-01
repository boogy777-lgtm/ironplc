using System;
using System.Linq;
using \u0011;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.PreCompile
{
	// Token: 0x02000165 RID: 357
	internal static class ParserHelper
	{
		// Token: 0x06001896 RID: 6294 RVA: 0x0004C528 File Offset: 0x0004A728
		internal static _ISignature \u0001(string \u0002, bool \u0003 = false)
		{
			return new \u0006(\u0002, \u0003).\u0001(null, null);
		}

		// Token: 0x06001897 RID: 6295 RVA: 0x0004C538 File Offset: 0x0004A738
		internal static _ISignature \u0001(string \u0002, string \u0003, bool \u0004)
		{
			\u0006 u = new \u0006(\u0003, \u0004);
			_ISignature isignature = (_ISignature)u.\u0001(\u0002);
			isignature.MessageGuid = u.MessageGuid;
			return isignature;
		}

		// Token: 0x06001898 RID: 6296 RVA: 0x0004C568 File Offset: 0x0004A768
		internal static bool \u0001()
		{
			IOEMCustomization oemcustomization = APEnvironmentFacade.Instance.OEMCustomization;
			if (oemcustomization != null && oemcustomization.HasValue("LanguageModelManager", "AllowDefinesInInterface"))
			{
				string stringValue = oemcustomization.GetStringValue("LanguageModelManager", "AllowDefinesInInterface");
				if (!string.IsNullOrEmpty(stringValue))
				{
					bool result;
					bool.TryParse(stringValue, out result);
					return result;
				}
			}
			return false;
		}

		// Token: 0x06001899 RID: 6297 RVA: 0x0004C5BC File Offset: 0x0004A7BC
		internal static bool \u0001(_IExpression \u0002)
		{
			if (\u0002 == null)
			{
				return true;
			}
			ErrorVisitor errorVisitor = new ErrorVisitor();
			\u0002.Accept(errorVisitor);
			if (errorVisitor.Messages == null)
			{
				return false;
			}
			return errorVisitor.MessageList.Any(new Func<_ICompilerMessage, bool>(ParserHelper.<>c.<>9.\u0001));
		}

		// Token: 0x04000453 RID: 1107
		private const string \u0001 = "LanguageModelManager";

		// Token: 0x04000454 RID: 1108
		private const string \u0002 = "AllowDefinesInInterface";
	}
}

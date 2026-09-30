using System;
using \u0007;
using \u0011;
using \u0014;
using \u0019;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u001C
{
	// Token: 0x020003B4 RID: 948
	internal static class \u0014
	{
		// Token: 0x0600368F RID: 13967 RVA: 0x000DD000 File Offset: 0x000DB200
		internal static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003)
		{
			\u001C.\u0014.\u0002(\u0002, \u0003);
			\u001C.\u0014.\u0003(\u0002, \u0003);
		}

		// Token: 0x06003690 RID: 13968 RVA: 0x000DD010 File Offset: 0x000DB210
		private static bool \u0001(_ICompileContext \u0002)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.HasDownloadCode() && !\u0002.MinimalSystem;
		}

		// Token: 0x06003691 RID: 13969 RVA: 0x000DD030 File Offset: 0x000DB230
		private static void \u0002(_ICompileContext \u0002, _ICompileContext \u0003)
		{
			int[] array;
			\u0002.SlotPOUs.GetDownloadGuidsSortedBySlot(out array);
			if (array.Length == 0 && !\u001C.\u0014.\u0001(\u0002))
			{
				return;
			}
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.Append("{implicit on}");
			lstringBuilder.AppendFormat("FUNCTION {0}", new object[]
			{
				IdentifierConstants.DownloadPOUName
			});
			lstringBuilder.Append("{implicit off}");
			_ISignature isignature = ParserHelper.\u0001(lstringBuilder.ToString(), true);
			isignature.SetFlag(SignatureFlag.Generated, true);
			isignature = isignature.CreateCompiledSignature(null, \u0002.HasByteSupport());
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002, isignature.Id);
			scope.LocalSignature = isignature;
			global::\u0014.\u0013.\u0002(isignature, scope, \u0002);
			_ISignature signRef = null;
			if (\u0003 != null)
			{
				signRef = \u0003[isignature.Name];
			}
			\u0002.AddSignature(isignature, signRef, \u0003, true);
		}

		// Token: 0x06003692 RID: 13970 RVA: 0x000DD0F4 File Offset: 0x000DB2F4
		private static void \u0003(_ICompileContext \u0002, _ICompileContext \u0003)
		{
			int[] array;
			Guid[] downloadGuidsSortedBySlot = \u0002.SlotPOUs.GetDownloadGuidsSortedBySlot(out array);
			if (array.Length == 0 && !\u001C.\u0014.\u0001(\u0002))
			{
				return;
			}
			_ISignature isignature = \u0002[IdentifierConstants.DownloadPOUName];
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.Append("{implicit on}");
			lstringBuilder.Append("{nobp}");
			foreach (Guid guidObject in downloadGuidsSortedBySlot)
			{
				_ISignature isignature2 = \u0002[guidObject];
				if (isignature2 != null)
				{
					lstringBuilder.AppendLine(isignature2.Name + "();");
				}
			}
			lstringBuilder.Append(APEnvironmentFacade.Instance.LanguageModelMgr.GetDownloadCode(\u0002, isignature));
			lstringBuilder.Append("{bp}");
			lstringBuilder.Append("{implicit off}");
			_ICompiledPOU icompiledPOU = \u0019.\u0003.\u0001(isignature.Name);
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002, isignature.Id);
			_IStatement istatement = new global::\u0011.\u0006(lstringBuilder.ToString(), true).\u0001();
			icompiledPOU.SetParseTree(istatement);
			\u0002.AddCompiledPOU(icompiledPOU, isignature, \u0003);
			ExpressionTypifierWithSpecialTasks ivisit = new ExpressionTypifierWithSpecialTasks(scope, \u0002, false, icompiledPOU);
			istatement.Accept(ivisit);
			TypeCheckerVisitor ivisit2 = new TypeCheckerVisitor(scope, \u0002, true);
			istatement.Accept(ivisit2);
			ErrorVisitor errorVisitor = new ErrorVisitor();
			istatement.Accept(errorVisitor);
			Debug.\u0001(errorVisitor.MessageList.Count == 0);
			icompiledPOU.SignatureId = isignature.Id;
			icompiledPOU.SetFlag(CompiledPOUFlags.TopLevel, true);
			icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile, true);
			icompiledPOU.SetFlag(CompiledPOUFlags.Typified, true);
			Locator.\u0001(isignature, null, \u0002, null);
		}
	}
}

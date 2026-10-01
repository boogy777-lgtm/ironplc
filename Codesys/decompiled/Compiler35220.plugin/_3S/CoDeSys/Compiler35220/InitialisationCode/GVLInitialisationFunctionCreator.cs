using System;
using System.Collections.Generic;
using System.Linq;
using \u0004;
using \u0007;
using \u0011;
using \u0014;
using \u0018;
using \u0019;
using _3S.CoDeSys.Compiler35220.Compile.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.ImplicitCode;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0084;

namespace _3S.CoDeSys.Compiler35220.InitialisationCode
{
	// Token: 0x020003AD RID: 941
	internal sealed class GVLInitialisationFunctionCreator
	{
		// Token: 0x06003645 RID: 13893 RVA: 0x000DA0E8 File Offset: 0x000D82E8
		internal GVLInitialisationFunctionCreator(_ICompileContext comconRef)
		{
			if (comconRef != null)
			{
				this.\u0001 = GVLInitialisationFunctionCreator.\u0001(comconRef);
				return;
			}
			this.\u0001 = new Dictionary<int, _ICompiledPOU[]>();
		}

		// Token: 0x06003646 RID: 13894 RVA: 0x000DA10C File Offset: 0x000D830C
		internal static IDictionary<int, _ICompiledPOU[]> \u0001(_ICompileContext \u0002)
		{
			return \u0002.CompiledPOUList.Where(new Func<_ICompiledPOU, bool>(GVLInitialisationFunctionCreator.<>c.<>9.\u0001)).GroupBy(new Func<_ICompiledPOU, int>(GVLInitialisationFunctionCreator.<>c.<>9.\u0001)).ToDictionary(new Func<IGrouping<int, _ICompiledPOU>, int>(GVLInitialisationFunctionCreator.<>c.<>9.\u0001), new Func<IGrouping<int, _ICompiledPOU>, _ICompiledPOU[]>(GVLInitialisationFunctionCreator.<>c.<>9.\u0001));
		}

		// Token: 0x06003647 RID: 13895 RVA: 0x000DA1AC File Offset: 0x000D83AC
		internal void \u0001(_ICompileContext \u0002, bool \u0003, bool \u0004, _ICompileContext \u0005, InitExitSignatureInfo \u0006)
		{
			IEnumerable<_ISignature> enumerable = \u0006.NormalSignaturesWithoutSlotCalls;
			global::\u0014.\u0005 u = (\u0002.CompiledSymbolTables as \u0084.\u0007).\u0001(string.Empty);
			foreach (_ISignature isignature in enumerable)
			{
				if ((isignature.HasAttribute(CompileAttributes.ATTRIBUTE_GEN_IMPLICIT_INIT_FUN) || string.IsNullOrEmpty(isignature.LibraryPath)) && Helper.\u0001(\u0002, isignature))
				{
					isignature.AddAttribute(CompileAttributes.ATTRIBUTE_GEN_IMPLICIT_INIT_FUN, null);
					_ISignature isignature2 = this.\u0001(\u0002, isignature, \u0003, \u0004, \u0005);
					if (u != null)
					{
						u.\u0001(isignature2.GetSearchName(\u0002), null, isignature2, null, global::\u0011.\u0005.\u0006, false);
					}
				}
			}
		}

		// Token: 0x06003648 RID: 13896 RVA: 0x000DA25C File Offset: 0x000D845C
		internal static _ICompiledPOU \u0001(_ICompileContext \u0002, _ISignature \u0003, _ISignature \u0004, _ISignature \u0005, _ICompileContext \u0006, out bool \u0007)
		{
			_ICompiledPOU icompiledPOU = global::\u0019.\u0003.\u0001(\u0005.Name);
			icompiledPOU.SetFlag(CompiledPOUFlags.TopLevel, true);
			icompiledPOU.SetFlagInternal(InternalCompiledPOUFlags.IsImplicitInitFunction, true);
			icompiledPOU.MessageGuid = \u0003.ObjectGuid;
			\u0005.MessageGuid = \u0003.ObjectGuid;
			global::\u0007.\u0013.\u0001(\u0002, icompiledPOU, \u0005, null);
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002, \u0003.Id);
			scope.LocalSignature = \u0003;
			scope.MethodSignature = \u0005;
			ConstantInit u = ConstantInit.NoConstants;
			_ISequenceStatement isequenceStatement = global::\u0004.\u0018.\u0001(\u0002, \u0003, scope, u, true, false, true, false, \u0005, null, "__bInitRetains", "__bInCopyCode");
			if (isequenceStatement == null)
			{
				isequenceStatement = global::\u0019.\u0003.\u0001();
				isequenceStatement.Add(global::\u0019.\u0003.\u0001());
			}
			_ISignature u2 = null;
			if (\u0006 != null)
			{
				u2 = \u0006[\u0005.Id];
			}
			Locator.\u0001(\u0005, u2, \u0002, \u0006);
			_IWarningDisableRestorePragmaStatement state = global::\u0019.\u0003.\u0001(Token.Empty, false, "C0357");
			isequenceStatement.InsertStatement(0, state);
			_IWarningDisableRestorePragmaStatement state2 = global::\u0019.\u0003.\u0001(Token.Empty, true, "C0357");
			isequenceStatement.AddStatement(state2);
			_ILocalSignatureIdPragma state3 = global::\u0019.\u0003.\u0001("localsignature " + \u0003.Id.ToString(), \u0003.Id);
			isequenceStatement.InsertStatement(0, state3);
			_IStatement istatement = isequenceStatement;
			(icompiledPOU.ParseTree as _ISequenceStatement).AddStatement(istatement);
			icompiledPOU.SignatureId = \u0005.Id;
			istatement.Accept(new ExpressionTypifierWithSpecialTasks(scope, \u0002, false, icompiledPOU)
			{
				TreatReferenceAsPointer = true,
				InterfaceAsInterface = true
			});
			ConstantFolder.ReplaceFoldedConstants(istatement, \u0002, scope, icompiledPOU);
			global::\u0018.\u000E.\u0001(istatement, \u0002);
			istatement.Accept(new TypeCheckerVisitor(scope, \u0002, true)
			{
				TreatReferenceAsPointer = true,
				InterfaceAsInterface = true,
				InImplicitCode = true
			});
			ErrorVisitor errorVisitor = new ErrorVisitor();
			istatement.Accept(errorVisitor);
			int num = 0;
			foreach (IMessage message in errorVisitor.MessageList)
			{
				if (message.Severity == Severity.Error || message.Severity == Severity.FatalError)
				{
					num++;
				}
			}
			icompiledPOU.SetMessages(errorVisitor.MessageList);
			\u0007 = (num > 0);
			icompiledPOU.SetFlag(CompiledPOUFlags.Typified, true);
			icompiledPOU.SetFlag(CompiledPOUFlags.ToGenerate, true);
			Guid guid;
			if (\u0003.HasAttribute("init_related_code") && Guid.TryParse(\u0003.GetAttributeValue("init_related_code"), out guid))
			{
				_ICompiledPOU icompiledPOU2 = \u0002.GetCompiledPOU(Guid.Parse(\u0003.GetAttributeValue("init_related_code"))) as _ICompiledPOU;
				if (icompiledPOU2 != null)
				{
					icompiledPOU2.SetFlag(CompiledPOUFlags.ToGenerate, true);
				}
			}
			return icompiledPOU;
		}

		// Token: 0x06003649 RID: 13897 RVA: 0x000DA4E4 File Offset: 0x000D86E4
		private _ISignature \u0001(_ICompileContext \u0002, _ISignature \u0003, bool \u0004, bool \u0005, _ICompileContext \u0006)
		{
			string text = GVLInitialisationFunctionCreator.\u0002(\u0003);
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.AppendFormat("{{attribute '{0}'}}", new object[]
			{
				CompileAttributes.ATTRIBUTE_IMPLICIT_INIT_FUN
			});
			lstringBuilder.AppendLine("FUNCTION " + text + " : BOOL");
			lstringBuilder.AppendLine("VAR_INPUT");
			lstringBuilder.AppendLine("\t__bInitRetains: BOOL;");
			lstringBuilder.AppendLine("END_VAR");
			lstringBuilder.AppendLine("VAR");
			lstringBuilder.AppendLine("\t__Index: DINT := 0; __bInCopyCode : BOOL := FALSE;");
			lstringBuilder.AppendLine("END_VAR");
			_ISignature isignature = null;
			if (\u0006 != null)
			{
				isignature = \u0006[text];
			}
			_ISignature isignature2 = ParserHelper.\u0001(lstringBuilder.ToString(), true);
			isignature2.SetFlag(SignatureFlag.Generated, true);
			isignature2 = isignature2.CreateCompiledSignature(isignature, \u0002.HasByteSupport());
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002, isignature2.Id);
			scope.LocalSignature = isignature2;
			global::\u0014.\u0013.\u0002(isignature2, scope, \u0002);
			\u0002.AddSignature(isignature2, isignature, \u0006, true);
			Locator.\u0001(\u0002, isignature2, isignature);
			Locator.\u0001(isignature2, isignature, \u0002, \u0006);
			_ISignature isignature3 = null;
			if (\u0006 != null)
			{
				isignature3 = \u0006[\u0003.Id];
			}
			_ICompiledPOU icompiledPOU;
			if (\u0005)
			{
				icompiledPOU = this.\u0001(\u0002, \u0006, \u0003, isignature3, \u0004, isignature2, isignature);
			}
			else
			{
				icompiledPOU = null;
			}
			if (icompiledPOU == null && !isignature2.HasErrors)
			{
				bool u;
				icompiledPOU = GVLInitialisationFunctionCreator.\u0001(\u0002, \u0003, isignature3, isignature2, \u0006, out u);
				\u0002.AddCompiledPOU(icompiledPOU, isignature2, true, \u0006);
				Debug.\u0003(u);
			}
			return isignature2;
		}

		// Token: 0x0600364A RID: 13898 RVA: 0x000DA64C File Offset: 0x000D884C
		private _ICompiledPOU \u0001(_ICompileContext \u0002, _ICompileContext \u0003, _ISignature \u0004, _ISignature \u0005, bool \u0006, _ISignature \u0007, _ISignature \u0008)
		{
			_ICompiledPOU icompiledPOU = null;
			if (!GVLInitialisationFunctionCreator.\u0001(\u0002, \u0004, \u0005, \u0008) && !\u0002.DataManager._MemorySettings.OnlineChangeInOwnSegment)
			{
				icompiledPOU = global::\u0019.\u0003.\u0001(\u0007.Name);
				GVLInitialisationFunctionCreator.\u0001(\u0002, \u0003, \u0006, \u0007, \u0008, \u0005, icompiledPOU);
				\u0002.AddCompiledPOU(icompiledPOU, \u0007, true, \u0003);
				CompilerServicesInternal.\u0001(\u0004.Id, \u0006, this.\u0001, \u0002);
				CompilerServicesInternal.\u0001(\u0007.Id, \u0006, this.\u0001, \u0002);
			}
			return icompiledPOU;
		}

		// Token: 0x0600364B RID: 13899 RVA: 0x000DA6CC File Offset: 0x000D88CC
		internal static bool \u0001(_ICompileContext \u0002, _ISignature \u0003, _ISignature \u0004, _ISignature \u0005)
		{
			if (\u0004 == null || \u0005 == null || \u0004.Checksum != \u0003.Checksum)
			{
				return true;
			}
			bool flag = \u0003.GetFlag(SignatureFlag.OnlineChanged);
			if (!flag)
			{
				IScope5 scope = global::\u0007.\u0005.\u0001(\u0002);
				foreach (int nId in \u0003.AllUsedIds)
				{
					if (scope[nId].GetFlag(SignatureFlag.OnlineChanged))
					{
						flag = true;
						break;
					}
				}
			}
			return \u0003.AllVariables.Any(new Func<_IVariable, bool>(GVLInitialisationFunctionCreator.<>c.<>9.\u0001)) || flag;
		}

		// Token: 0x0600364C RID: 13900 RVA: 0x000DA768 File Offset: 0x000D8968
		internal static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003, bool \u0004, _ISignature \u0005, _ISignature \u0006, _ISignature \u0007, _ICompiledPOU \u0008)
		{
			_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
			isequenceStatement.Add(global::\u0019.\u0003.\u0001());
			\u0008.SetParseTree(isequenceStatement);
			_ICompiledPOU icompiledPOU = \u0003._GetCompiledPOUById(\u0006.Id);
			\u0008.CompiledCode = icompiledPOU.CompiledCode;
			MemoryCompiler.\u0002(\u0002.DataManager, \u0008.CompiledCode.Location.Area, \u0008.CompiledCode.Location.Offset, \u0008.CompiledCode.CodeSize, DataSegmentFlags.Code);
			if (!\u0004)
			{
				\u0008.SetFlag(CompiledPOUFlags.ToCompile, false);
			}
			if (!\u0004)
			{
				\u0008.SetFlag(CompiledPOUFlags.NoCompile, true);
			}
			\u0008.SetFlag(CompiledPOUFlags.NoCompile, true);
			\u0008.Checksum = icompiledPOU.Checksum;
			\u0008.SetFlag(CompiledPOUFlags.TopLevel, true);
			foreach (int nId in \u0007.CalleeIds)
			{
				if (\u0002[nId] != null)
				{
					\u0002[nId].AddCaller(\u0007.Id);
				}
			}
		}

		// Token: 0x0600364D RID: 13901 RVA: 0x000DA860 File Offset: 0x000D8A60
		private static string \u0001(_ISignature \u0002)
		{
			int num = \u0002.Name.IndexOf("<");
			if (num >= 0)
			{
				return \u0002.Name.Remove(num);
			}
			return \u0002.Name;
		}

		// Token: 0x0600364E RID: 13902 RVA: 0x000DA898 File Offset: 0x000D8A98
		public static string \u0002(_ISignature \u0002)
		{
			string str;
			if (\u0002.ParentSignatureId != Helper.InvalidId)
			{
				str = GVLInitialisationFunctionCreator.\u0001(\u0002) + "__" + \u0002.ParentSignatureId.ToString();
			}
			else
			{
				str = GVLInitialisationFunctionCreator.\u0001(\u0002) + "__" + \u0002.Id.ToString();
			}
			if (\u0002.GetFlag(SignatureFlag.SystemNamespaceForced))
			{
				str += "__SYSTEM__";
			}
			return str + "__GVL__Init";
		}

		// Token: 0x04000A93 RID: 2707
		private readonly IDictionary<int, _ICompiledPOU[]> \u0001;
	}
}

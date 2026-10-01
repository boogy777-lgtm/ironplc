using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0001;
using \u0004;
using \u0007;
using \u000E;
using \u0011;
using \u0014;
using \u0018;
using \u0019;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Compile.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Features;
using _3S.CoDeSys.Compiler35220.ImplicitCode;
using _3S.CoDeSys.Compiler35220.InitialisationCode;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0083;

namespace \u0080
{
	// Token: 0x020003A7 RID: 935
	internal static class \u0019
	{
		// Token: 0x170008CC RID: 2252
		// (get) Token: 0x06003607 RID: 13831 RVA: 0x000D8570 File Offset: 0x000D6770
		private static string GeneratedTaskConfigStructNameUppercase
		{
			get
			{
				if (\u0080.\u0019.\u0001 == null)
				{
					\u0080.\u0019.\u0001 = APEnvironmentFacade.Instance.LMServiceProvider.TaskLMService.GeneratedTaskConfigStruct.ToLowerInvariant();
				}
				return \u0080.\u0019.\u0001;
			}
		}

		// Token: 0x06003608 RID: 13832 RVA: 0x000D859C File Offset: 0x000D679C
		private static _ISequenceStatement \u0001(_ICompileContext \u0002)
		{
			_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
			Guid parentApplicationGuid = APEnvironmentFacade.Instance.LMServiceProvider.LanguageModelProviderService.GetParentApplicationGuid(\u0002.ApplicationGuid);
			if (parentApplicationGuid == Guid.Empty)
			{
				return isequenceStatement;
			}
			string text = APEnvironmentFacade.Instance.LanguageModelMgr.GetApplicationNameByGuid(parentApplicationGuid);
			if (string.IsNullOrEmpty(text))
			{
				return isequenceStatement;
			}
			int num = text.LastIndexOf('.');
			text = text.Substring(num + 1);
			for (int i = 0; i < \u0002.TaskList.Count; i++)
			{
				ITaskInfo2 taskInfo = \u0002.TaskList[i] as ITaskInfo2;
				if (taskInfo != null && !string.IsNullOrEmpty(taskInfo.ParentTaskName))
				{
					int[] slotArray = \u0002.SlotPOUs.GetSlotArray(taskInfo.TaskGuid);
					for (int j = 0; j < slotArray.Length; j++)
					{
						foreach (_ICompiledPOU icompiledPOU in \u0002.SlotPOUs.GetTaskSlotPOUs(taskInfo.TaskGuid, slotArray[j], \u0002))
						{
							_IVariableExpression u = global::\u0019.\u0003.\u0001("__sys__register__slot__pou");
							_ILiteralExpression exp = global::\u0019.\u0003.\u0001(taskInfo.ParentTaskName);
							_ILiteralExpression exp2 = global::\u0019.\u0003.\u0001(text);
							_ILiteralExpression exp3 = global::\u0019.\u0003.\u0001((long)((ulong)(slotArray[j] + 10)));
							_IVariableExpression u2 = global::\u0019.\u0003.\u0001(icompiledPOU.Name);
							_IOperatorExpression exp4 = global::\u0019.\u0003.\u0001(Operator.Adr, u2);
							_ICallExpression icallExpression = global::\u0019.\u0003.\u0001(u);
							icallExpression.AddParam(exp);
							icallExpression.AddParam(exp2);
							icallExpression.AddParam(exp3);
							icallExpression.AddParam(exp4);
							_IExpressionStatement sm = global::\u0019.\u0003.\u0001(icallExpression);
							isequenceStatement.Add(sm);
						}
					}
				}
			}
			return isequenceStatement;
		}

		// Token: 0x06003609 RID: 13833 RVA: 0x000D8738 File Offset: 0x000D6938
		internal static _ISequenceStatement \u0002(_ICompileContext \u0002)
		{
			Guid parentApplicationGuid = APEnvironmentFacade.Instance.LMServiceProvider.LanguageModelProviderService.GetParentApplicationGuid(\u0002.ApplicationGuid);
			if (parentApplicationGuid == Guid.Empty)
			{
				return null;
			}
			string text = APEnvironmentFacade.Instance.LanguageModelMgr.GetApplicationNameByGuid(parentApplicationGuid);
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			int num = text.LastIndexOf('.');
			text = text.Substring(num + 1);
			_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
			for (int i = 0; i < \u0002.TaskList.Count; i++)
			{
				ITaskInfo2 taskInfo = \u0002.TaskList[i] as ITaskInfo2;
				if (taskInfo != null && !string.IsNullOrEmpty(taskInfo.ParentTaskName))
				{
					int[] slotArray = \u0002.SlotPOUs.GetSlotArray(taskInfo.TaskGuid);
					for (int j = 0; j < slotArray.Length; j++)
					{
						foreach (_ICompiledPOU icompiledPOU in \u0002.SlotPOUs.GetTaskSlotPOUs(taskInfo.TaskGuid, slotArray[j], \u0002))
						{
							_IVariableExpression u = global::\u0019.\u0003.\u0001("__sys__unregister__slot__pou");
							_ILiteralExpression exp = global::\u0019.\u0003.\u0001(taskInfo.ParentTaskName);
							_ILiteralExpression exp2 = global::\u0019.\u0003.\u0001(text);
							_ILiteralExpression exp3 = global::\u0019.\u0003.\u0001((long)((ulong)(slotArray[j] + 10)));
							_IVariableExpression u2 = global::\u0019.\u0003.\u0001(icompiledPOU.Name);
							_IOperatorExpression exp4 = global::\u0019.\u0003.\u0001(Operator.Adr, u2);
							_ICallExpression icallExpression = global::\u0019.\u0003.\u0001(u);
							icallExpression.AddParam(exp);
							icallExpression.AddParam(exp2);
							icallExpression.AddParam(exp3);
							icallExpression.AddParam(exp4);
							_IExpressionStatement sm = global::\u0019.\u0003.\u0001(icallExpression);
							isequenceStatement.Add(sm);
						}
					}
				}
			}
			return isequenceStatement;
		}

		// Token: 0x0600360A RID: 13834 RVA: 0x000D88D4 File Offset: 0x000D6AD4
		internal static IEnumerable<_ICompiledPOU> \u0001(_ICompileContext \u0002, InitExitSignatureInfo \u0003)
		{
			foreach (KeyValuePair<string, LList<_ISignature>> keyValuePair in \u0003.ExplicitSignatures)
			{
				yield return \u0080.\u0019.\u0001(\u0002, keyValuePair.Key, keyValuePair.Value);
			}
			IEnumerator<KeyValuePair<string, LList<_ISignature>>> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x0600360B RID: 13835 RVA: 0x000D88EC File Offset: 0x000D6AEC
		private static _ICompiledPOU \u0001(_ICompileContext \u0002, string \u0003, LList<_ISignature> \u0004)
		{
			_ISignature isignature = \u0002[IdentifierConstants.GetExplicitInitPOUName(\u0003)];
			Debug.\u0001(isignature != null, "signExplicitInit != null");
			_ICompiledPOU icompiledPOU = global::\u0019.\u0003.\u0001(isignature.Name);
			_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
			for (int i = 0; i < 2; i++)
			{
				foreach (_ISignature u in \u0004)
				{
					\u0080.\u0019.\u0001(\u0002, isequenceStatement, u, i == 0, false, isignature, icompiledPOU);
				}
			}
			icompiledPOU.SetParseTree(isequenceStatement);
			icompiledPOU.SignatureId = isignature.Id;
			icompiledPOU.SetFlag(CompiledPOUFlags.Typified, true);
			Locator.\u0001(isignature, null, \u0002, null);
			return icompiledPOU;
		}

		// Token: 0x0600360C RID: 13836 RVA: 0x000D89A4 File Offset: 0x000D6BA4
		internal static _ISignature \u0001(_ICompileContext \u0002, _ICompileContext \u0003)
		{
			return \u0080.\u0019.\u0001(\u0002, \u0003, false);
		}

		// Token: 0x0600360D RID: 13837 RVA: 0x000D89B0 File Offset: 0x000D6BB0
		internal static _ISignature \u0001(_ICompileContext \u0002, _ICompileContext \u0003, bool \u0004)
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			if (\u0004)
			{
				lstringBuilder.AppendLine("FUNCTION global__init__x : BOOL");
				lstringBuilder.AppendLine("VAR_INPUT");
				lstringBuilder.AppendLine("\t__bInitRetains: BOOL;ptaskinfo : POINTER TO _IMPLICIT_TASK_INFO;papplicationinfo : POINTER TO _IMPLICIT_APPLICATION_INFO;");
				lstringBuilder.AppendLine("END_VAR");
			}
			else
			{
				lstringBuilder.AppendLine("FUNCTION global__init : BOOL");
				lstringBuilder.AppendLine("VAR_INPUT");
				lstringBuilder.AppendLine("\t__bInitRetains: BOOL;");
				lstringBuilder.AppendLine("END_VAR");
			}
			lstringBuilder.AppendLine("VAR");
			lstringBuilder.AppendLine("\t__Index: DINT := 0; __bInCopyCode : BOOL := FALSE;");
			lstringBuilder.AppendLine("END_VAR");
			_ISignature isignature = null;
			if (\u0003 != null)
			{
				isignature = \u0003[IdentifierConstants.GlobalInitName];
			}
			_ISignature isignature2 = ParserHelper.\u0001(lstringBuilder.ToString(), true);
			isignature2.SetFlag(SignatureFlag.Generated, true);
			isignature2 = isignature2.CreateCompiledSignature(isignature, \u0002.HasByteSupport());
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002, isignature2.Id);
			scope.LocalSignature = isignature2;
			global::\u0014.\u0013.\u0002(isignature2, scope, \u0002);
			\u0002.AddSignature(isignature2, isignature, \u0003, true);
			Locator.\u0001(\u0002, isignature2, isignature);
			Locator.\u0001(isignature2, null, \u0002, null);
			return isignature2;
		}

		// Token: 0x0600360E RID: 13838 RVA: 0x000D8AC0 File Offset: 0x000D6CC0
		private static bool \u0001(_ICompileContext \u0002, _ISignature \u0003, bool \u0004)
		{
			if (\u0003.GetFlag(SignatureFlag.NoInit))
			{
				return true;
			}
			if (\u0002.IsDefined("global_init_in_cycle"))
			{
				if (\u0003.Name == \u0080.\u0019.GeneratedTaskConfigStructNameUppercase || \u0003.Name == "_IMPLICIT_KINDOFTASK" || \u0003.Name == "_IMPLICIT_TARGET_INFO_VARIABLES")
				{
					if (!\u0004)
					{
						return true;
					}
				}
				else if (\u0004)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600360F RID: 13839 RVA: 0x000D8B28 File Offset: 0x000D6D28
		internal static void \u0001(_ICompileContext \u0002, _ISequenceStatement \u0003, _ISignature \u0004, bool \u0005, bool \u0006, _ISignature \u0007, _ICompiledPOU \u0008)
		{
			if (\u0080.\u0019.\u0001(\u0002, \u0004, \u0006))
			{
				return;
			}
			bool flag = \u0004.HasAttribute(CompileAttributes.ATTRIBUTE_GEN_IMPLICIT_INIT_FUN);
			IScope5 scope;
			if (\u0005 || !flag)
			{
				if (\u0004.POUType == Operator.Program || \u0004.POUType == Operator.VarGlobal || \u0004.Statics.Length != 0)
				{
					if (\u0005 && \u0004.NonReplacedConstants.Count == 0)
					{
						return;
					}
					scope = global::\u0007.\u0005.\u0001(\u0002, \u0004.Id);
					scope.LocalSignature = \u0004;
					scope.MethodSignature = \u0007;
					ConstantInit u = ConstantInit.Constants;
					if (!\u0005)
					{
						u = ConstantInit.NoConstants;
					}
					_ISequenceStatement isequenceStatement = global::\u0004.\u0018.\u0001(\u0002, \u0004, scope, u, true, false, true, false, \u0007, null, "__bInitRetains", "__bInCopyCode");
					if (isequenceStatement == null || isequenceStatement._StatementList.Count == 0)
					{
						return;
					}
					_ILocalSignatureIdPragma state = global::\u0019.\u0003.\u0001("localsignature " + \u0004.Id.ToString(), \u0004.Id);
					isequenceStatement.InsertStatement(0, state);
					_IMessageGuidPragmaStatement state2 = global::\u0019.\u0003.\u0001(Token.Empty, \u0004.ObjectGuid);
					isequenceStatement.InsertStatement(0, state2);
					_IStatement istatement = isequenceStatement;
					istatement.Accept(new ExpressionTypifierWithSpecialTasks(scope, \u0002, false, \u0008)
					{
						TreatReferenceAsPointer = true,
						InterfaceAsInterface = true,
						NoCrossReferences = true
					});
					ConstantFolder.ReplaceFoldedConstants(istatement, \u0002, scope, \u0008);
					global::\u0018.\u000E.\u0001(istatement, \u0002);
					istatement.Accept(new TypeCheckerVisitor(scope, \u0002, true)
					{
						TreatReferenceAsPointer = true,
						InterfaceAsInterface = true
					});
					global::\u0001.\u0012.\u0001(istatement, scope, \u0004);
					\u0003.Add(istatement);
				}
				return;
			}
			if (\u0004.HasAttribute(CompileAttributes.ATTRIBUTE_NO_INIT_CALL))
			{
				return;
			}
			_ICallExpression icallExpression = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(GVLInitialisationFunctionCreator.\u0002(\u0004)), Token.Empty);
			_IExpressionStatement iexpressionStatement = global::\u0019.\u0003.\u0001(icallExpression, Token.Empty);
			icallExpression.AddParam(global::\u0019.\u0003.\u0001("__bInitRetains"), global::\u0019.\u0003.\u0001("__bInitRetains"));
			scope = global::\u0007.\u0005.\u0001(\u0002, \u0004.Id);
			scope.LocalSignature = \u0004;
			scope.MethodSignature = \u0007;
			iexpressionStatement.Accept(new ExpressionTypifierWithSpecialTasks(scope, \u0002, false, \u0008)
			{
				TreatReferenceAsPointer = true,
				InterfaceAsInterface = true,
				NoCrossReferences = true
			});
			iexpressionStatement.Accept(new TypeCheckerVisitor(scope, \u0002, true)
			{
				TreatReferenceAsPointer = true,
				InterfaceAsInterface = true
			});
			\u0003.Add(iexpressionStatement);
			global::\u0001.\u0012.\u0001(\u0004, scope);
		}

		// Token: 0x06003610 RID: 13840 RVA: 0x000D8D58 File Offset: 0x000D6F58
		internal static void \u0001(_ICompileContext \u0002, _ISequenceStatement \u0003, _ISignature \u0004, _ISignature \u0005, bool \u0006)
		{
			bool flag = \u0004["bInitRetains"] != null;
			IVariable variable = \u0004["bInExitCode"];
			LList<string> llist = new LList<string>();
			if (flag)
			{
				if (\u0006)
				{
					llist.Add("bInitRetains := FALSE");
				}
				else
				{
					llist.Add("bInitRetains := __bInitRetains");
				}
			}
			if (variable != null)
			{
				if (\u0006)
				{
					llist.Add("bInExitCode := TRUE");
				}
				else
				{
					llist.Add("bInExitCode := FALSE");
				}
			}
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.AppendFormat("{0}({1});", new object[]
			{
				Helper.\u0001(\u0002, \u0004).ToString(),
				string.Join(", ", llist)
			});
			_IStatement istatement = new global::\u0011.\u0006(lstringBuilder.ToString(), true).\u0001();
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002, \u0005.Id);
			istatement.Accept(new ExpressionTypifierWithSpecialTasks(scope, \u0002, false, null)
			{
				TreatReferenceAsPointer = true,
				InterfaceAsInterface = true
			});
			istatement.Accept(new TypeCheckerVisitor(scope, \u0002, true)
			{
				TreatReferenceAsPointer = true,
				InterfaceAsInterface = true
			});
			ErrorVisitor errorVisitor = new ErrorVisitor();
			istatement.Accept(errorVisitor);
			Debug.\u0001(errorVisitor.MessageList.Count == 0);
			\u0003.Add(istatement);
		}

		// Token: 0x06003611 RID: 13841 RVA: 0x000D8E80 File Offset: 0x000D7080
		internal static void \u0001(_ICompileContext \u0002, string \u0003, LStringBuilder \u0004)
		{
			ReflectionAssignmentCoder.\u0001(\u0004, \u0002);
		}

		// Token: 0x06003612 RID: 13842 RVA: 0x000D8E8C File Offset: 0x000D708C
		private static bool \u0001(IEnumerable<IVariable> \u0002, string \u0003)
		{
			\u0080.\u0019.\u0002 u = new \u0080.\u0019.\u0002();
			u.\u0001 = \u0003;
			return \u0002.Any(new Func<IVariable, bool>(u.\u0001));
		}

		// Token: 0x06003613 RID: 13843 RVA: 0x000D8EB8 File Offset: 0x000D70B8
		private static bool \u0001(_ISignature \u0002, IExpression \u0003, LStringBuilder \u0004)
		{
			bool flag = \u0002.Name == "IOGLOBALINIT__POU" && \u0080.\u0019.\u0001(\u0002.Inputs, "__BNOIOMGRUPDATEMAPPING");
			if (flag)
			{
				string str = \u0003.ToString();
				\u0004.AppendLine(str + "(__BNOIOMGRUPDATEMAPPING := FALSE);");
			}
			return flag;
		}

		// Token: 0x06003614 RID: 13844 RVA: 0x000D8F08 File Offset: 0x000D7108
		internal static _ISequenceStatement \u0001(_ICompileContext \u0002, _ISignature \u0003)
		{
			_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
			IList<IExpression> list2;
			IList<_ISignature> list = \u0083.\u000F.\u0002(\u0002, out list2);
			if (list.Count > 0 || APEnvironmentFacade.Instance.LanguageModelMgr.HasGlobalInitCode())
			{
				LStringBuilder lstringBuilder = new LStringBuilder();
				lstringBuilder.AppendLine("{implicit on}");
				for (int i = 0; i < list.Count; i++)
				{
					_ISignature isignature = list[i];
					IExpression expression = list2[i];
					IVariable variable = isignature["bInitRetains"];
					if (!\u0080.\u0019.\u0001(isignature, expression, lstringBuilder))
					{
						if (isignature.POUType == Operator.Method)
						{
							IEnumerable<string> enumerable = InstancePathService.\u0001(\u0002, isignature, new \u0080.\u0005.\u0001
							{
								\u0001 = true,
								\u0002 = false
							});
							if (enumerable == null || !enumerable.Any<string>())
							{
								goto IL_157;
							}
							using (IEnumerator<string> enumerator = enumerable.GetEnumerator())
							{
								while (enumerator.MoveNext())
								{
									string str = enumerator.Current;
									if (variable != null && variable.GetFlag(VarFlag.Input))
									{
										lstringBuilder.AppendLine(str + "(bInitRetains := __bInitRetains);");
									}
									else
									{
										lstringBuilder.AppendLine(str + "();");
									}
								}
								goto IL_157;
							}
						}
						string str2 = expression.ToString();
						if (variable != null && variable.GetFlag(VarFlag.Input))
						{
							lstringBuilder.AppendLine(str2 + "(bInitRetains := __bInitRetains);");
						}
						else
						{
							lstringBuilder.AppendLine(str2 + "();");
						}
					}
					IL_157:;
				}
				lstringBuilder.Append(APEnvironmentFacade.Instance.LanguageModelMgr.GetGlobalInitCode(\u0002, \u0003));
				lstringBuilder.AppendLine("{implicit off}");
				_IStatement sm = new global::\u0011.\u0006(lstringBuilder.ToString(), true).\u0001();
				isequenceStatement.Add(sm);
			}
			return isequenceStatement;
		}

		// Token: 0x06003615 RID: 13845 RVA: 0x000D90D0 File Offset: 0x000D72D0
		internal static _ICompiledPOU \u0001(_ICompileContext \u0002, bool \u0003, _ISignature \u0004, _ICompileContext \u0005, InitExitSignatureInfo \u0006, bool \u0007, bool \u0008, bool \u000E)
		{
			_ICompiledPOU icompiledPOU = global::\u0019.\u0003.\u0001(\u0004.Name);
			icompiledPOU.SetFlag(CompiledPOUFlags.TopLevel, true);
			if (\u0003)
			{
				_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
				isequenceStatement.Add(global::\u0019.\u0003.\u0001());
				icompiledPOU.SetParseTree(isequenceStatement);
				IDictionary<int, _ICompiledPOU[]> u = GVLInitialisationFunctionCreator.\u0001(\u0005);
				CompilerServicesInternal.\u0001(\u0004.Id, \u0007, u, \u0002);
				icompiledPOU.SetFlag(CompiledPOUFlags.Typified, true);
				icompiledPOU.SignatureId = \u0004.Id;
				return icompiledPOU;
			}
			global::\u0007.\u0013.\u0001(\u0002, icompiledPOU, \u0004, null);
			if (icompiledPOU.ParseTree == null)
			{
				icompiledPOU.SetParseTree(global::\u0019.\u0003.\u0001());
			}
			_ISequenceStatement isequenceStatement2 = icompiledPOU.ParseTree as _ISequenceStatement;
			LStringBuilder lstringBuilder = new LStringBuilder();
			string applicationNameByGuid = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetApplicationNameByGuid(\u0002.ApplicationGuid, \u0002.SimulationMode);
			string applicationName = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetApplicationName(\u0002.ApplicationGuid, \u0002.SimulationMode);
			Guid parentApplication = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetParentApplication(\u0002.ApplicationGuid);
			bool flag = \u0004.Name == "GLOBAL__INIT__X";
			IScope5 scope = global::\u0007.\u0005.\u0002(\u0002);
			if (flag)
			{
				scope.LocalSignature = \u0004;
			}
			if (flag)
			{
				lstringBuilder.AppendLine("if papplicationinfo^.udState = 2 then RETURN; end_if");
			}
			if (!\u0002.MinimalSystem)
			{
				\u0080.\u0019.\u0001(\u0002, \u000E, lstringBuilder, applicationName, parentApplication);
			}
			\u0080.\u0019.\u0001(\u0002, applicationNameByGuid, lstringBuilder);
			if (lstringBuilder.Length > 0)
			{
				_IStatement istatement = new global::\u0011.\u0006(lstringBuilder.ToString(), true).\u0001();
				\u0080.\u0019.\u0001(scope, \u0002, icompiledPOU, istatement);
				isequenceStatement2.Add(istatement);
			}
			\u0080.\u0019.\u0001(\u0002, \u0004, \u0006, \u0008, isequenceStatement2, icompiledPOU);
			_ISequenceStatement isequenceStatement3 = \u0080.\u0019.\u0001(\u0002, \u0004);
			\u0080.\u0019.\u0001(global::\u0007.\u0005.\u0001(\u0002, \u0004.Id), \u0002, icompiledPOU, isequenceStatement3);
			isequenceStatement2.Add(isequenceStatement3);
			_IWarningDisableRestorePragmaStatement state = global::\u0019.\u0003.\u0001(Token.Empty, false, "C0357");
			isequenceStatement2.InsertStatement(0, state);
			_IWarningDisableRestorePragmaStatement state2 = global::\u0019.\u0003.\u0001(Token.Empty, true, "C0357");
			isequenceStatement2.AddStatement(state2);
			_ISequenceStatement isequenceStatement4 = \u0080.\u0019.\u0001(\u0002);
			\u0080.\u0019.\u0001(scope, \u0002, icompiledPOU, isequenceStatement4);
			isequenceStatement2.Add(isequenceStatement4);
			icompiledPOU.SetParseTree(isequenceStatement2);
			icompiledPOU.SignatureId = \u0004.Id;
			icompiledPOU.SetFlag(CompiledPOUFlags.Typified, true);
			Locator.\u0001(\u0004, null, \u0002, null);
			return icompiledPOU;
		}

		// Token: 0x06003616 RID: 13846 RVA: 0x000D92F0 File Offset: 0x000D74F0
		private static void \u0001(_ICompileContext \u0002, _ISignature \u0003, InitExitSignatureInfo \u0004, bool \u0005, _ISequenceStatement \u0006, _ICompiledPOU \u0007)
		{
			IEnumerable<global::\u000E.\u0019> enumerable = \u0004.NormalSignatures;
			for (int i = 0; i < 2; i++)
			{
				if (i == 0)
				{
					\u0006.Add(global::\u0019.\u0003.\u0001(" --- Initialization of not replaced constants --- ", Token.Empty));
				}
				else
				{
					\u0006.Add(global::\u0019.\u0003.\u0001(" --- Initialization of all other variables --- ", Token.Empty));
				}
				foreach (global::\u000E.\u0019 u in enumerable)
				{
					\u0083.\u0010 u2 = u as \u0083.\u0010;
					if (u2 != null)
					{
						\u0080.\u0019.\u0001(\u0002, \u0006, u2._ISignature, i == 0, \u0005, \u0003, \u0007);
					}
					else if (i == 1)
					{
						\u0083.\u0011 u3 = u as \u0083.\u0011;
						if (u3 != null)
						{
							\u0080.\u0019.\u0001(\u0002, \u0006, u3.CalleeSignature, \u0003, false);
						}
					}
				}
			}
		}

		// Token: 0x06003617 RID: 13847 RVA: 0x000D93C4 File Offset: 0x000D75C4
		private static void \u0001(_ICompileContext \u0002, bool \u0003, LStringBuilder \u0004, string \u0005, Guid \u0006)
		{
			if (\u0003)
			{
				\u0004.AppendFormat("__ApplicationName.__ApplicationName := '{0}';", new object[]
				{
					\u0005
				});
			}
			else
			{
				\u0004.AppendFormat("__ApplicationName.__ApplicationName := \"{0}\";", new object[]
				{
					\u0005
				});
			}
			if (\u0006 != Guid.Empty)
			{
				string applicationName = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetApplicationName(\u0006, \u0002.SimulationMode);
				if (\u0003)
				{
					\u0004.AppendFormat("__ApplicationName.__ParentApplicationName := '{0}';", new object[]
					{
						applicationName
					});
					return;
				}
				\u0004.AppendFormat("__ApplicationName.__ParentApplicationName := \"{0}\";", new object[]
				{
					applicationName
				});
			}
		}

		// Token: 0x06003618 RID: 13848 RVA: 0x000D9460 File Offset: 0x000D7660
		private static void \u0001(IScope5 \u0002, _ICompileContext \u0003, _ICompiledPOU \u0004, _IStatement \u0005)
		{
			\u0005.Accept(new ExpressionTypifierWithSpecialTasks(\u0002, \u0003, false, \u0004)
			{
				TreatReferenceAsPointer = true,
				InterfaceAsInterface = true
			});
			global::\u0018.\u000E.\u0001(\u0005, \u0003);
			\u0005.Accept(new TypeCheckerVisitor(\u0002, \u0003, true)
			{
				TreatReferenceAsPointer = true,
				InterfaceAsInterface = true
			});
			ErrorVisitor ivisit = new ErrorVisitor();
			\u0005.Accept(ivisit);
		}

		// Token: 0x04000A82 RID: 2690
		private static string \u0001;

		// Token: 0x020003A9 RID: 937
		[CompilerGenerated]
		private sealed class \u0002
		{
			// Token: 0x06003623 RID: 13859 RVA: 0x000D9668 File Offset: 0x000D7868
			internal bool \u0001(IVariable \u0002)
			{
				return \u0002.Name == this.\u0001 && \u0002.Type.Class == TypeClass.Bool;
			}

			// Token: 0x04000A8B RID: 2699
			public string \u0001;
		}
	}
}

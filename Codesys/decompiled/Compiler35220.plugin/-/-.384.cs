using System;
using System.Collections.Generic;
using System.Linq;
using \u0007;
using \u000E;
using \u0011;
using \u0014;
using \u0019;
using _3S.CoDeSys.Compiler35220.ImplicitCode;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.OnlineChange;
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
using \u0080;
using \u0083;
using \u0084;

namespace \u001D
{
	// Token: 0x020003D5 RID: 981
	internal static class \u000F
	{
		// Token: 0x0600371F RID: 14111 RVA: 0x000E1EA8 File Offset: 0x000E00A8
		internal static void \u0001(IVariable \u0002, IScope \u0003, _ISequenceStatement \u0004, _IExpression \u0005, ref int \u0006)
		{
			if (\u001D.\u000F.\u0001(\u0002))
			{
				return;
			}
			if (\u0002.GetFlag(VarFlag.Static))
			{
				return;
			}
			if (\u0002.HasFlag(VarFlag.Inout | VarFlag.External | VarFlag.ReplacedConstant))
			{
				return;
			}
			if (\u0002.Type.Class == TypeClass.Userdef)
			{
				ISignature signature = \u0003[(\u0002.Type as IUserdefType).SignatureId];
				if (signature != null && signature.GetSubSignature(IdentifierConstants.ExitMethodName) != null)
				{
					_ILanguageModelBuilder ilanguageModelBuilder = global::\u0019.\u0003.Builder;
					IVariableExpression2 expLeft = ilanguageModelBuilder.CreateVariableExpression(null, \u0002.OrgName);
					IVariableExpression2 expRight = ilanguageModelBuilder.CreateVariableExpression(null, "FB_Exit");
					ICompoAccessExpression expCallee = ilanguageModelBuilder.CreateCompoAccessExpression(null, expLeft, expRight);
					List<IAssignmentExpression> list = new List<IAssignmentExpression>();
					List<IAssignmentExpression> outputassignments = new List<IAssignmentExpression>();
					IVariableExpression2 expLeft2 = ilanguageModelBuilder.CreateVariableExpression(null, "bInCopyCode");
					IAssignmentExpression item = ilanguageModelBuilder.CreateAssignmentExpression(null, expLeft2, \u0005);
					list.Add(item);
					\u0004.AddStatement(ilanguageModelBuilder.CreateCallStatement(null, expCallee, null, null, list, outputassignments));
					return;
				}
			}
			else if (\u0002.Type.Class == TypeClass.Array)
			{
				int num = Helper.\u0001(\u0002.Type as ICompiledType);
				\u0006 = ((num > \u0006) ? num : \u0006);
				IArrayType arrayType = \u0002.Type as _IArrayType;
				ICompiledType compiledType = \u0084.\u0004.\u0001(arrayType as _IArrayType);
				if (compiledType.Class != TypeClass.Userdef)
				{
					return;
				}
				ISignature signature2 = \u0003[(compiledType as IUserdefType).SignatureId];
				if (signature2 != null && signature2.GetSubSignature(IdentifierConstants.ExitMethodName) != null)
				{
					new LStringBuilder().Append(\u0002.OrgName);
					_ILanguageModelBuilder7 ilanguageModelBuilder2 = global::\u0019.\u0003.Builder;
					_IExpression expLeft3 = Helper.\u0001(ilanguageModelBuilder2.CreateVariableExpression(null, \u0002.OrgName) as _IExpression, arrayType as ICompiledType, 0);
					ICompoAccessExpression expCallee2 = ilanguageModelBuilder2.CreateCompoAccessExpression(null, expLeft3, global::\u0019.\u0003.\u0001("FB_Exit"));
					List<IAssignmentExpression> list2 = new List<IAssignmentExpression>();
					List<IAssignmentExpression> outputassignments2 = new List<IAssignmentExpression>();
					IVariableExpression2 expLeft4 = ilanguageModelBuilder2.CreateVariableExpression(null, "bInCopyCode");
					IAssignmentExpression item2 = ilanguageModelBuilder2.CreateAssignmentExpression(null, expLeft4, \u0005);
					list2.Add(item2);
					IStatement statement = ilanguageModelBuilder2.CreateCallStatement(null, expCallee2, null, null, list2, outputassignments2);
					_IStatement state = Helper.\u0001(arrayType as _IArrayType, statement as _IStatement, \u0003 as IScope5, 0);
					\u0004.AddStatement(state);
				}
			}
		}

		// Token: 0x06003720 RID: 14112 RVA: 0x000E20C4 File Offset: 0x000E02C4
		internal static void \u0001(IVariable \u0002, IScope \u0003, LStringBuilder \u0004, string \u0005, ref int \u0006)
		{
			if (\u001D.\u000F.\u0001(\u0002))
			{
				return;
			}
			if (\u0002.Type.Class == TypeClass.Userdef)
			{
				ISignature signature = \u0003[(\u0002.Type as IUserdefType).SignatureId];
				if (signature != null && signature.GetSubSignature(IdentifierConstants.ExitMethodName) != null)
				{
					\u0004.AppendLine(\u0002.OrgName + ".FB_Exit(bInCopyCode := " + \u0005 + ");");
					return;
				}
			}
			else if (\u0002.Type.Class == TypeClass.Array)
			{
				int num = Helper.\u0001(\u0002.Type as ICompiledType);
				\u0006 = ((num > \u0006) ? num : \u0006);
				IArrayType arrayType = \u0002.Type as _IArrayType;
				ICompiledType compiledType = \u0084.\u0004.\u0001(arrayType as _IArrayType);
				if (compiledType.Class != TypeClass.Userdef)
				{
					return;
				}
				ISignature signature2 = \u0003[(compiledType as IUserdefType).SignatureId];
				if (signature2 != null && signature2.GetSubSignature(IdentifierConstants.ExitMethodName) != null)
				{
					LStringBuilder lstringBuilder = new LStringBuilder();
					lstringBuilder.Append(\u0002.OrgName);
					Helper.\u0001(arrayType as _IArrayType, lstringBuilder, 0);
					string u = string.Concat(new string[]
					{
						lstringBuilder.ToString(),
						".FB_Exit(bInCopyCode := ",
						\u0005,
						");",
						Environment.NewLine
					});
					lstringBuilder = new LStringBuilder();
					Helper.\u0001(arrayType as _IArrayType, u, lstringBuilder, \u0003 as IScope5, 0);
					\u0004.Append(lstringBuilder.ToString());
				}
			}
		}

		// Token: 0x06003721 RID: 14113 RVA: 0x000E2238 File Offset: 0x000E0438
		internal static void \u0001(_ICompileContext \u0002, LStringBuilder \u0003, IScope \u0004, _ISignature \u0005, bool \u0006, string \u0007, _IVariable \u0008, bool \u000E, int \u000F, ref bool \u0010, OnlineChangeDetails \u0011)
		{
			if (\u001D.\u000F.\u0001(\u0008))
			{
				return;
			}
			if (\u0084.\u0004.\u0001(\u0008.CompiledType).Class != TypeClass.Userdef)
			{
				return;
			}
			string text = \u0008.VersionedName;
			if (\u0007 != string.Empty)
			{
				text = \u0007 + "." + \u0008.VersionedName;
			}
			if (\u0008.Type.Class == TypeClass.Array)
			{
				_IArrayType u = \u0008.Type as _IArrayType;
				int u2 = Helper.\u0001(u);
				Helper.\u0001(\u000F, u2, \u0005, null);
				LStringBuilder lstringBuilder = new LStringBuilder();
				lstringBuilder.Append(text);
				Helper.\u0001(u, lstringBuilder, \u000F);
			}
			if (\u0006 && !\u0008.GetFlag(VarFlag.OnlChangeNoExit) && \u0011 != null)
			{
				\u0011.AddDeletedVariable(\u0008, \u0004.LocalSignature, null);
			}
			int num = 0;
			int length = \u0003.Length;
			if (!\u0008.GetFlag(VarFlag.OnlChangeNoExit) || !\u0006)
			{
				\u001D.\u000F.\u0001(\u0008, \u0004, \u0003, "FALSE", ref num);
			}
			else if (\u0008.GetFlag(VarFlag.OnlChangeCopy) || \u000E)
			{
				\u001D.\u000F.\u0001(\u0008, \u0004, \u0003, "TRUE", ref num);
			}
			if (length < \u0003.Length)
			{
				\u0010 = true;
				\u0008.SetFlag(VarFlag.OnlChangeExit, true);
			}
		}

		// Token: 0x06003722 RID: 14114 RVA: 0x000E2368 File Offset: 0x000E0568
		internal static IEnumerable<_ICompiledPOU> \u0001(_ICompileContext \u0002, InitExitSignatureInfo \u0003)
		{
			foreach (KeyValuePair<string, LList<_ISignature>> keyValuePair in \u0003.ExplicitSignatures)
			{
				yield return \u001D.\u000F.\u0001(\u0002, keyValuePair.Key, keyValuePair.Value);
			}
			IEnumerator<KeyValuePair<string, LList<_ISignature>>> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x06003723 RID: 14115 RVA: 0x000E2380 File Offset: 0x000E0580
		internal static _ICompiledPOU \u0001(_ICompileContext \u0002, string \u0003, LList<_ISignature> \u0004)
		{
			_ISignature isignature = \u0002[IdentifierConstants.GetExplicitExitPOUName(\u0003)];
			Debug.\u0001(isignature != null, "signExplicitExit != null");
			_ICompiledPOU icompiledPOU = global::\u0019.\u0003.\u0001(isignature.Name);
			_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
			\u0004.Reverse();
			foreach (_ISignature u in \u0004)
			{
				\u001D.\u000F.\u0001(\u0002, isequenceStatement, u, isignature, false, null);
			}
			icompiledPOU.SetParseTree(isequenceStatement);
			icompiledPOU.SignatureId = isignature.Id;
			icompiledPOU.SetFlag(CompiledPOUFlags.Typified, true);
			Locator.\u0001(isignature, null, \u0002, null);
			return icompiledPOU;
		}

		// Token: 0x06003724 RID: 14116 RVA: 0x000E2428 File Offset: 0x000E0628
		internal static void \u0001(_ICompileContext \u0002, _ISignature \u0003, _ISequenceStatement \u0004, InitExitSignatureInfo \u0005)
		{
			LList<global::\u000E.\u0019> llist = \u0005.NormalSignatures;
			_IStatement istatement = null;
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002, \u0003.Id);
			scope.LocalSignature = \u0003;
			IList<IExpression> list = null;
			IList<_ISignature> list2 = \u0083.\u000F.\u0001(\u0002, out list);
			if (list2.Count > 0)
			{
				LStringBuilder lstringBuilder = new LStringBuilder();
				lstringBuilder.Append("{implicit on}");
				\u0080.\u0005.\u0001 u = new \u0080.\u0005.\u0001
				{
					\u0001 = true
				};
				int i = 0;
				while (i < list2.Count)
				{
					_ISignature isignature = list2[i];
					IExpression expression = list[i];
					if (isignature.POUType == Operator.Method)
					{
						using (IEnumerator<string> enumerator = InstancePathService.\u0001(\u0002, scope[isignature.ParentSignatureId] as _ISignature, u).GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								string str = enumerator.Current;
								lstringBuilder.AppendLine(str + "." + isignature.Name + "();");
							}
							goto IL_11E;
						}
						goto IL_EF;
					}
					goto IL_EF;
					IL_11E:
					i++;
					continue;
					IL_EF:
					if (isignature.POUType == Operator.Program || isignature.POUType == Operator.Function)
					{
						lstringBuilder.AppendLine(expression.ToString() + "();");
						goto IL_11E;
					}
					goto IL_11E;
				}
				IScope5 scope2 = global::\u0007.\u0005.\u0001(\u0002);
				istatement = new global::\u0011.\u0006(lstringBuilder.ToString(), true).\u0001();
				istatement.Accept(new ExpressionTypifierWithSpecialTasks(scope2, \u0002, false, null)
				{
					TreatReferenceAsPointer = true,
					InterfaceAsInterface = true
				});
				istatement.Accept(new TypeCheckerVisitor(scope2, \u0002, true)
				{
					TreatReferenceAsPointer = true,
					InterfaceAsInterface = true
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
				Debug.\u0001(num == 0);
				\u0004.Add(istatement);
			}
			for (int j = llist.Count - 1; j >= 0; j--)
			{
				\u0083.\u0010 u2 = llist[j] as \u0083.\u0010;
				if (u2 != null)
				{
					\u001D.\u000F.\u0001(\u0002, \u0004, u2._ISignature, \u0003, false, null);
				}
				else
				{
					\u0083.\u0011 u3 = llist[j] as \u0083.\u0011;
					if (u3 != null)
					{
						\u0080.\u0019.\u0001(\u0002, \u0004, u3.CalleeSignature, \u0003, true);
					}
				}
			}
			_ISequenceStatement isequenceStatement = \u0080.\u0019.\u0002(\u0002);
			if (isequenceStatement != null)
			{
				IScope5 scope3 = global::\u0007.\u0005.\u0001(\u0002);
				isequenceStatement.Accept(new ExpressionTypifierWithSpecialTasks(scope3, \u0002, false, null)
				{
					TreatReferenceAsPointer = true,
					InterfaceAsInterface = true
				});
				isequenceStatement.Accept(new TypeCheckerVisitor(scope3, \u0002, true)
				{
					TreatReferenceAsPointer = true,
					InterfaceAsInterface = true
				});
				ErrorVisitor errorVisitor2 = new ErrorVisitor();
				isequenceStatement.Accept(errorVisitor2);
				int num2 = 0;
				foreach (IMessage message2 in errorVisitor2.MessageList)
				{
					if (message2.Severity == Severity.Error || message2.Severity == Severity.FatalError)
					{
						num2++;
					}
				}
				if (\u0004.StatementList.Count<IStatement>() > 0)
				{
					\u0004.InsertStatement(0, isequenceStatement);
					return;
				}
				\u0004.Add(isequenceStatement);
			}
		}

		// Token: 0x06003725 RID: 14117 RVA: 0x000E278C File Offset: 0x000E098C
		internal static void \u0001(_ICompileContext \u0002, _ISignature \u0003, _ISequenceStatement \u0004, InitExitSignatureInfo \u0005, OnlineChangeDetails \u0006)
		{
			LList<global::\u000E.\u0019> llist = \u0005.NormalSignatures;
			for (int i = llist.Count - 1; i >= 0; i--)
			{
				\u0083.\u0010 u = llist[i] as \u0083.\u0010;
				if (u != null)
				{
					\u001D.\u000F.\u0001(\u0002, \u0004, u._ISignature, \u0003, true, \u0006);
				}
			}
		}

		// Token: 0x06003726 RID: 14118 RVA: 0x000E27D4 File Offset: 0x000E09D4
		internal static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003, _ISignature \u0004, _ISequenceStatement \u0005, OnlineChangeDetails \u0006)
		{
			foreach (_ISignature isignature in \u0003.AllFlat)
			{
				if (isignature.POUType == Operator.FunctionBlock)
				{
					_ISignature isignature2 = (\u0002 != null) ? \u0002[isignature.Id] : null;
					if (isignature2 != null)
					{
						\u001D.\u000F.\u0001(\u0003, isignature2, isignature, \u0004, \u0005, \u0006);
					}
				}
			}
		}

		// Token: 0x06003727 RID: 14119 RVA: 0x000E2848 File Offset: 0x000E0A48
		private static void \u0001(_ICompileContext \u0002, _ISequenceStatement \u0003, _ISignature \u0004, _ISignature \u0005, bool \u0006, OnlineChangeDetails \u0007)
		{
			if (\u0004.POUType != Operator.Program && \u0004.POUType != Operator.VarGlobal && \u0004.Statics.Length == 0)
			{
				return;
			}
			IList<_IVariable> allVariables = \u0004.AllVariables;
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002, \u0004.Id);
			scope.LocalSignature = \u0004;
			scope.MethodSignature = \u0005;
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.Append("{localsignature " + \u0004.Id.ToString() + "}");
			bool flag = false;
			for (int i = allVariables.Count - 1; i >= 0; i--)
			{
				IVariable variable = allVariables[i];
				if (!\u001D.\u000F.\u0001(variable) && variable.HasFlag(VarFlag.Absolut))
				{
					\u001D.\u000F.\u0001(\u0002, lstringBuilder, scope, \u0005, \u0006, string.Empty, variable as _IVariable, false, 0, ref flag, \u0007);
				}
			}
			if (!flag)
			{
				return;
			}
			_IStatement istatement = new global::\u0011.\u0006(lstringBuilder.ToString(), true).\u0001();
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

		// Token: 0x06003728 RID: 14120 RVA: 0x000E29A4 File Offset: 0x000E0BA4
		private static void \u0001(_ICompileContext \u0002, _ISignature \u0003, _ISignature \u0004, _ISignature \u0005, _ISequenceStatement \u0006, OnlineChangeDetails \u0007)
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002, \u0004.Id);
			scope.MethodSignature = \u0005;
			IList<_IVariable> allVariables = \u0004.AllVariables;
			bool flag = false;
			for (int i = allVariables.Count - 1; i >= 0; i--)
			{
				_IVariable ivariable = allVariables[i];
				flag |= \u001D.\u000F.\u0001(\u0002, \u0004, \u0005, \u0007, scope, lstringBuilder, ivariable, \u0003[ivariable.Name] as _IVariable);
			}
			if (flag)
			{
				lstringBuilder.Insert(0, "{implicit on}");
				lstringBuilder.AppendLine("{implicit off}");
				_IStatement sm = \u001D.\u000F.\u0001(\u0002, \u0005, lstringBuilder.ToString());
				\u0006.Add(sm);
			}
		}

		// Token: 0x06003729 RID: 14121 RVA: 0x000E2A4C File Offset: 0x000E0C4C
		private static bool \u0001(_ICompileContext \u0002, _ISignature \u0003, _ISignature \u0004, OnlineChangeDetails \u0005, IScope5 \u0006, LStringBuilder \u0007, _IVariable \u0008, _IVariable \u000E)
		{
			if (\u001D.\u000F.\u0001(\u0008))
			{
				return false;
			}
			if (\u0008.HasFlag(VarFlag.Absolut))
			{
				return false;
			}
			if (\u000E != null)
			{
				return false;
			}
			ICompiledType compiledType = \u0084.\u0004.\u0001(\u0008.CompiledType);
			if (compiledType.Class != TypeClass.Userdef)
			{
				return false;
			}
			ISignature signature = \u0006[((IUserdefType)compiledType).SignatureId];
			ISignature signature2 = (signature != null) ? signature.GetSubSignature(IdentifierConstants.ExitMethodName) : null;
			if (signature2 == null && \u0005 == null)
			{
				return false;
			}
			LList<string> llist = InstancePathService.\u0001(\u0002, \u0003, Array.Empty<int>(), null, null, true);
			if (\u0005 != null)
			{
				\u0005.AddDeletedVariable(\u0008, \u0003, llist);
			}
			if (signature2 == null)
			{
				return false;
			}
			if (\u0008.CompiledType.Class == TypeClass.Userdef)
			{
				foreach (string text in llist)
				{
					\u0007.AppendFormat("{0}.{1}.FB_Exit(bInCopyCode := FALSE);", new object[]
					{
						text,
						\u0008.Name
					});
				}
				return llist.Any<string>();
			}
			return \u0008.CompiledType.Class == TypeClass.Array && \u001D.\u000F.\u0001(\u0004, \u0006, \u0007, \u0008, llist);
		}

		// Token: 0x0600372A RID: 14122 RVA: 0x000E2B70 File Offset: 0x000E0D70
		private static bool \u0001(_ISignature \u0002, IScope5 \u0003, LStringBuilder \u0004, _IVariable \u0005, LList<string> \u0006)
		{
			_IArrayType iarrayType = (_IArrayType)\u0005.CompiledType;
			int num = Helper.\u0001(iarrayType);
			if (num == 0)
			{
				return false;
			}
			bool flag;
			int numOfElements = iarrayType.GetNumOfElements(\u0003, out flag);
			if (flag && numOfElements == 0)
			{
				return false;
			}
			Helper.\u0001(num, \u0002, null);
			LStringBuilder lstringBuilder = new LStringBuilder();
			foreach (string text in \u0006)
			{
				lstringBuilder.Clear();
				lstringBuilder.AppendFormat("{0}.{1}", new object[]
				{
					text,
					\u0005.Name
				});
				Helper.\u0001(iarrayType, lstringBuilder, 0);
				string u = string.Format("{0}.FB_Exit(bInCopyCode:= FALSE);", lstringBuilder);
				lstringBuilder.Clear();
				Helper.\u0001(iarrayType, u, lstringBuilder, \u0003, 0);
				\u0004.Append(lstringBuilder);
			}
			return \u0006.Any<string>();
		}

		// Token: 0x0600372B RID: 14123 RVA: 0x000E2C58 File Offset: 0x000E0E58
		private static _IStatement \u0001(_ICompileContext \u0002, _ISignature \u0003, string \u0004)
		{
			_IStatement istatement = new global::\u0011.\u0006(\u0004, true).\u0001();
			IScope5 scope = \u0002.CreateGlobalIScope() as IScope5;
			scope.MethodSignature = \u0003;
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
			ErrorVisitor ivisit = new ErrorVisitor();
			istatement.Accept(ivisit);
			return istatement;
		}

		// Token: 0x0600372C RID: 14124 RVA: 0x000E2CD0 File Offset: 0x000E0ED0
		internal static _ISignature \u0001(_ICompileContext \u0002, bool \u0003, _ICompileContext \u0004)
		{
			string text = "GLOBAL__EXIT";
			if (\u0003)
			{
				text = "GLOBAL__EXIT__COPY";
			}
			_ISignature isignature = ParserHelper.\u0001("FUNCTION " + text + " : BOOL" + Environment.NewLine, true);
			isignature.SetFlag(SignatureFlag.Generated, true);
			_ISignature isignature2 = null;
			if (\u0004 != null)
			{
				isignature2 = \u0004[text];
			}
			isignature = isignature.CreateCompiledSignature(isignature2, \u0002.HasByteSupport());
			if (\u0003)
			{
				isignature.SetFlag(SignatureFlag.ToRemoveAfterDownload | SignatureFlag.NoCompareWithNew, true);
			}
			_ISignature signRef = isignature2;
			\u0002.AddSignature(isignature, signRef, null, true);
			IScope5 u = global::\u0007.\u0005.\u0001(\u0002, isignature.Id);
			global::\u0014.\u0013.\u0002(isignature, u, \u0002);
			Locator.\u0001(\u0002, isignature, isignature2);
			Locator.\u0001(isignature, isignature2, \u0002, \u0004);
			return isignature;
		}

		// Token: 0x0600372D RID: 14125 RVA: 0x000E2D78 File Offset: 0x000E0F78
		internal static _ICompiledPOU \u0001(_ICompileContext \u0002, _ICompileContext \u0003, InitExitSignatureInfo \u0004)
		{
			string stName = "GLOBAL__EXIT";
			_ISignature isignature = \u0003[stName];
			_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
			\u001D.\u000F.\u0001(\u0002, isignature, isequenceStatement, \u0004);
			_ICompiledPOU icompiledPOU = global::\u0019.\u0003.\u0001(isignature.Name);
			icompiledPOU.SetFlag(CompiledPOUFlags.TopLevel, true);
			Locator.\u0001(isignature, null, \u0002, null);
			icompiledPOU.SetParseTree(isequenceStatement);
			icompiledPOU.SignatureId = isignature.Id;
			icompiledPOU.SetFlag(CompiledPOUFlags.Typified, true);
			return icompiledPOU;
		}

		// Token: 0x0600372E RID: 14126 RVA: 0x000E2DDC File Offset: 0x000E0FDC
		internal static _ICompiledPOU \u0001(_ICompileContext \u0002, _ICompileContext \u0003, InitExitSignatureInfo \u0004, OnlineChangeDetails \u0005)
		{
			string stName = "GLOBAL__EXIT__COPY";
			_ISignature isignature = \u0003[stName];
			_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
			\u001D.\u000F.\u0001(\u0003, isignature, isequenceStatement, \u0004, \u0005);
			\u001D.\u000F.\u0001(\u0002, \u0003, isignature, isequenceStatement, \u0005);
			_ICompiledPOU icompiledPOU = global::\u0019.\u0003.\u0001(isignature.Name);
			icompiledPOU.SetFlag(CompiledPOUFlags.TopLevel, true);
			Locator.\u0001(isignature, null, \u0002, null);
			icompiledPOU.SetParseTree(isequenceStatement);
			icompiledPOU.SignatureId = isignature.Id;
			icompiledPOU.SetFlag(CompiledPOUFlags.Typified, true);
			return icompiledPOU;
		}

		// Token: 0x0600372F RID: 14127 RVA: 0x000E2E48 File Offset: 0x000E1048
		internal static bool \u0001(IVariable \u0002)
		{
			return \u0002.HasFlag(VarFlag.Inout | VarFlag.External | VarFlag.ReplacedConstant) || (\u0002.Address != null && \u0002.Address.Location == DirectVariableLocation.Input) || \u0002.Type.Class == TypeClass.Reference || (\u0002 as _IVariable).IsProperty || \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_NO_EXIT);
		}
	}
}

using System;
using System.Collections.Generic;
using \u0003;
using \u0006;
using \u0007;
using \u0011;
using \u0014;
using \u0019;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.OnlineChange;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.PreCompile.Typification.MessageSuppression;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0080;
using \u0083;
using \u0084;

namespace \u0004
{
	// Token: 0x0200035A RID: 858
	internal static class \u0014
	{
		// Token: 0x06003382 RID: 13186 RVA: 0x000C8214 File Offset: 0x000C6414
		internal static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003, _ISignature \u0004, bool \u0005, bool \u0006, bool \u0007, bool \u0008, Codegeneration \u000E, IMessageCategory \u000F)
		{
			IList<IExpression> list = new LList<IExpression>();
			IList<_ISignature> list2;
			if (\u0008)
			{
				list2 = \u0083.\u000F.\u0004(\u0002, out list);
			}
			else
			{
				list2 = \u0083.\u000F.\u0005(\u0002, out list);
			}
			if (list2.Count == 0)
			{
				return;
			}
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002, \u0004.Id);
			scope.LocalSignature = \u0004;
			_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
			_ILanguageModelBuilder ilanguageModelBuilder = global::\u0019.\u0003.Builder;
			for (int i = 0; i < list2.Count; i++)
			{
				_ISignature isignature = list2[i];
				IExpression expCallee = list[i];
				if (\u0008)
				{
					string stName = string.Format(IdentifierConstants.POUStartAddressTemplate, isignature.Id);
					IVariable variable = \u0004[stName];
					if (variable != null)
					{
						_IVariableExpression exp = global::\u0019.\u0003.\u0001(variable as _IVariable, \u0004);
						_IOperatorExpression ioperatorExpression = global::\u0019.\u0003.\u0001(Operator.Adr);
						ioperatorExpression.AddOperand(exp);
						string stName2 = string.Format(IdentifierConstants.SignFPAddressTemplate, isignature.Id);
						ISignature signature = \u0002[IdentifierConstants.GlobalImplicitFunctionPointers];
						_IAssignmentExpression iassignmentExpression = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(signature[stName2] as _IVariable, signature as _ISignature));
						iassignmentExpression._RValue = ioperatorExpression;
						isequenceStatement.Add(global::\u0019.\u0003.\u0001(iassignmentExpression, Token.Empty));
					}
				}
				List<IAssignmentExpression> list3 = new List<IAssignmentExpression>();
				if (isignature["bInitRetains"] != null)
				{
					IVariableExpression expLeft = ilanguageModelBuilder.CreateVariableExpression(null, "bInitRetains");
					list3.Add(ilanguageModelBuilder.CreateAssignmentExpression(null, expLeft, global::\u0019.\u0003.\u0001(true)));
				}
				if (isignature["bInCopyCode"] != null)
				{
					IVariableExpression expLeft2 = ilanguageModelBuilder.CreateVariableExpression(null, "bInCopyCode");
					list3.Add(ilanguageModelBuilder.CreateAssignmentExpression(null, expLeft2, global::\u0019.\u0003.\u0001(false)));
				}
				if (isignature["bOnlyCodeChanged"] != null)
				{
					IVariableExpression expLeft3 = ilanguageModelBuilder.CreateVariableExpression(null, "bOnlyCodeChanged");
					_ILiteralExpression expRight = global::\u0019.\u0003.\u0001(\u0007);
					list3.Add(ilanguageModelBuilder.CreateAssignmentExpression(null, expLeft3, expRight));
				}
				isequenceStatement.AddStatement(ilanguageModelBuilder.CreateCallStatement(null, expCallee, null, null, list3, new List<IAssignmentExpression>()));
			}
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.Append("{implicit on}");
			lstringBuilder.AppendFormat("FUNCTION {0}", new object[]
			{
				IdentifierConstants.GetOnlineChangeConcurrentPOUName(\u0008)
			});
			lstringBuilder.Append("{implicit off}");
			_ISignature isignature2 = ParserHelper.\u0001(lstringBuilder.ToString(), true);
			isignature2.SetFlag(SignatureFlag.Generated, true);
			isignature2 = isignature2.CreateCompiledSignature(null, \u0002.HasByteSupport());
			IScope5 scope2 = global::\u0007.\u0005.\u0001(\u0002, isignature2.Id);
			scope2.LocalSignature = isignature2;
			global::\u0014.\u0013.\u0002(isignature2, scope2, \u0002);
			_ISignature signRef = null;
			if (\u0003 != null)
			{
				signRef = \u0003[isignature2.Name];
			}
			\u0002.AddSignature(isignature2, signRef, \u0003, true);
			_IStatement parseTree = isequenceStatement;
			_ICompiledPOU icompiledPOU = global::\u0019.\u0003.\u0001(IdentifierConstants.OnlineChange1ConcurrentPOUName);
			icompiledPOU.SetFlag(CompiledPOUFlags.TopLevel, true);
			icompiledPOU.SignatureId = isignature2.Id;
			icompiledPOU.SetParseTree(parseTree);
			\u0002.AddCompiledPOU(icompiledPOU, isignature2, \u0003);
			ExpressionTypifierWithSpecialTasks visitor = new ExpressionTypifierWithSpecialTasks(scope, \u0002, false, icompiledPOU);
			icompiledPOU.Accept(visitor);
			TypeCheckerVisitor visitor2 = new TypeCheckerVisitor(scope, \u0002);
			icompiledPOU.Accept(visitor2);
			ErrorVisitor errorVisitor = new ErrorVisitor();
			icompiledPOU.Accept(errorVisitor);
			Debug.\u0001(!Helper.\u0001(errorVisitor.MessageList, Severity.Error));
			icompiledPOU.SignatureId = isignature2.Id;
			icompiledPOU.SetFlag(CompiledPOUFlags.TopLevel, true);
			icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile, true);
			icompiledPOU.SetFlag(CompiledPOUFlags.Typified, true);
			Locator.\u0001(isignature2, null, \u0002, null);
			\u000E.\u0001(icompiledPOU, \u0004, isignature2);
			ushort u = 0;
			int u2 = 0;
			if (!MemoryCompiler.\u0003(\u0002.DataManager, ref u, ref u2, \u0002.DataManager.PackMode, icompiledPOU.CompiledCode.CodeSize, \u0002.DataManager._MemorySettings.CodeSegmentSize, DataSegmentFlags.Code))
			{
				string u3 = global::\u0003.\u0006.\u0001(MessageId.Err_OutOfCodeMemory, new object[]
				{
					icompiledPOU.Name,
					icompiledPOU.CompiledCode.CodeSize
				});
				_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u3, Severity.Error, MessageId.Err_OutOfCodeMemory);
				APEnvironmentFacade.Instance.AddMessage(\u000F, message);
			}
			else
			{
				icompiledPOU.CompiledCode.Location = global::\u0019.\u0003.\u0001(u, u2);
			}
			icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile, true);
			icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile | CompiledPOUFlags.ToRemoveAfterDownload, true);
			isignature2.SetFlag(SignatureFlag.ToRemoveAfterDownload, true);
			isignature2.AddAttribute("ReallyReallyRemoveMe", "");
			if (\u0005)
			{
				icompiledPOU.SetFlag(CompiledPOUFlags.BootProjectRelevant, true);
			}
		}

		// Token: 0x06003383 RID: 13187 RVA: 0x000C8664 File Offset: 0x000C6864
		internal static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003, bool \u0004)
		{
			int[] array;
			Guid[] onlineChangeGuidsSortedBySlot = \u0002.SlotPOUs.GetOnlineChangeGuidsSortedBySlot(out array);
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.Append("{implicit on}");
			lstringBuilder.AppendFormat("FUNCTION {0}", new object[]
			{
				IdentifierConstants.OnlineChangePOUName
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
			LStringBuilder lstringBuilder2 = new LStringBuilder();
			lstringBuilder2.Append("{implicit on}");
			lstringBuilder2.Append("{nobp}");
			foreach (Guid guidObject in onlineChangeGuidsSortedBySlot)
			{
				_ISignature isignature2 = \u0002[guidObject];
				if (isignature2 != null)
				{
					if (isignature2.Name == "IOGLOBALINIT__POU" && isignature2.Inputs.Length == 1 && isignature2.Inputs[0].Name == "__BNOIOMGRUPDATEMAPPING")
					{
						bool flag = false;
						string applicationName = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetApplicationName(\u0002.ApplicationGuid, \u0002.SimulationMode);
						string stName = string.Format("IoConfig_{0}_Mappings", applicationName);
						_ISignature isignature3 = \u0002[stName];
						_ISignature isignature4 = null;
						if (\u0003 != null)
						{
							isignature4 = \u0003[stName];
						}
						if (isignature3 != null && isignature4 != null && isignature3.Checksum == isignature4.Checksum && !\u0002.ContainsCopyCode)
						{
							isignature3.AddAttribute("mapping_unchanged", null);
							flag = true;
						}
						if (flag)
						{
							lstringBuilder2.AppendLine(isignature2.Name + "(__bNoIoMgrUpdateMapping := NOT(__SYSTEM.__COMPILE_CONSTANTS.__CONTAINS_COPY_CODE));");
						}
						else
						{
							lstringBuilder2.AppendLine(isignature2.Name + "(__bNoIoMgrUpdateMapping := FALSE);");
						}
					}
					else
					{
						lstringBuilder2.AppendLine(isignature2.Name + "();");
					}
				}
			}
			IList<IExpression> list = null;
			IList<_ISignature> list2 = \u0083.\u000F.\u0003(\u0002, out list);
			if (list2.Count > 0)
			{
				\u0080.\u0005.\u0001 u = new \u0080.\u0005.\u0001
				{
					\u0001 = true,
					\u0002 = false
				};
				int j = 0;
				while (j < list2.Count)
				{
					_ISignature isignature5 = list2[j];
					IExpression expression = list[j];
					if (isignature5.POUType == Operator.Method)
					{
						using (IEnumerator<string> enumerator = InstancePathService.\u0001(\u0002, scope[isignature5.ParentSignatureId] as _ISignature, u).GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								string str = enumerator.Current;
								lstringBuilder2.AppendLine(str + "." + isignature5.Name + "();");
							}
							goto IL_303;
						}
						goto IL_2D4;
					}
					goto IL_2D4;
					IL_303:
					j++;
					continue;
					IL_2D4:
					if (isignature5.POUType == Operator.Program || isignature5.POUType == Operator.Function)
					{
						lstringBuilder2.AppendLine(expression.ToString() + "();");
						goto IL_303;
					}
					goto IL_303;
				}
			}
			lstringBuilder2.Append(APEnvironmentFacade.Instance.LanguageModelMgr.GetOnlineChangeCode(\u0002, isignature));
			lstringBuilder2.Append("{bp}");
			lstringBuilder2.Append("{implicit off}");
			_ICompiledPOU icompiledPOU = global::\u0019.\u0003.\u0001(isignature.Name);
			IScope5 scope2 = global::\u0007.\u0005.\u0001(\u0002, isignature.Id);
			_IStatement istatement = new global::\u0011.\u0006(lstringBuilder2.ToString(), true).\u0001();
			icompiledPOU.SetParseTree(istatement);
			\u0002.AddCompiledPOU(icompiledPOU, isignature, \u0003);
			ExpressionTypifierWithSpecialTasks ivisit = new ExpressionTypifierWithSpecialTasks(scope2, \u0002, false, icompiledPOU);
			istatement.Accept(ivisit);
			TypeCheckerVisitor ivisit2 = new TypeCheckerVisitor(scope2, \u0002, true);
			istatement.Accept(ivisit2);
			ErrorVisitor errorVisitor = new ErrorVisitor();
			istatement.Accept(errorVisitor);
			Debug.\u0001(!Helper.\u0001(errorVisitor.MessageList, Severity.Error));
			icompiledPOU.SignatureId = isignature.Id;
			icompiledPOU.SetFlag(CompiledPOUFlags.TopLevel, true);
			icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile, true);
			icompiledPOU.SetFlag(CompiledPOUFlags.Typified, true);
			Locator.\u0001(isignature, null, \u0002, null);
		}

		// Token: 0x06003384 RID: 13188 RVA: 0x000C8A88 File Offset: 0x000C6C88
		internal static void \u0001(_ICompileContext \u0002, LStringBuilder \u0003, IVariable \u0004, ICompiledType \u0005, _ISignature \u0006, IScope5 \u0007, string \u0008, int \u000E)
		{
			global::\u0004.\u0014.\u0001(\u0002, \u0003, \u0007, \u0006, \u0008, \u0004, \u0005, \u000E);
		}

		// Token: 0x06003385 RID: 13189 RVA: 0x000C8A9C File Offset: 0x000C6C9C
		internal static bool \u0001(_ICompileContext \u0002, LStringBuilder \u0003, IScope5 \u0004, _ISignature \u0005, string \u0006, IVariable \u0007, ICompiledType \u0008, int \u000E)
		{
			ICompiledType compiledType = \u0084.\u0004.\u0001(\u0008);
			if (compiledType.Class != TypeClass.Userdef)
			{
				return false;
			}
			_ISignature isignature = (_ISignature)((_IUserdefType)compiledType).GetSignature(\u0004);
			ISignature subSignature = isignature.GetSubSignature(IdentifierConstants.ReInitMethodName);
			int num = \u000E;
			if (\u0008.Class == TypeClass.Array)
			{
				_IArrayType u = (_IArrayType)\u0007.Type;
				int u2 = Helper.\u0001(u);
				Helper.\u0001(\u000E, u2, \u0005, null);
				LStringBuilder lstringBuilder = new LStringBuilder();
				lstringBuilder.Append(\u0006);
				Helper.\u0001(u, lstringBuilder, \u000E);
				\u0006 = lstringBuilder.ToString();
				num++;
			}
			string text;
			if (SignatureCheckHelper.\u0001(subSignature))
			{
				text = string.Concat(new string[]
				{
					\u0006,
					".",
					IdentifierConstants.ReInitMethodName,
					"();",
					Environment.NewLine
				});
			}
			else
			{
				text = string.Empty;
			}
			string copyFunctionName = IdentifierConstants.GetCopyFunctionName(isignature);
			if (\u0002[copyFunctionName] == null)
			{
				IEnumerable<_IVariable> enumerable = Helper.\u0001(\u0004, isignature);
				LStringBuilder lstringBuilder2 = new LStringBuilder();
				bool flag = false;
				foreach (_IVariable ivariable in enumerable)
				{
					string u3 = \u0006 + "." + ivariable.VersionedName;
					flag |= global::\u0004.\u0014.\u0001(\u0002, lstringBuilder2, \u0004, \u0005, u3, ivariable, ivariable.CompiledType, num);
				}
				if (flag)
				{
					text += lstringBuilder2.ToString();
				}
			}
			if (text != string.Empty)
			{
				if (\u0008.Class == TypeClass.Array)
				{
					Helper.\u0001((_IArrayType)\u0008, text, \u0003, \u0004, \u000E);
				}
				else
				{
					\u0003.Append(text);
				}
				return true;
			}
			return false;
		}

		// Token: 0x06003386 RID: 13190 RVA: 0x000C8C44 File Offset: 0x000C6E44
		private static ICompiledType \u0001(_ICompileContext \u0002, ICompiledType \u0003, ICompiledType \u0004, _ICompileContext \u0005)
		{
			_IArrayType iarrayType = \u0003 as _IArrayType;
			_IArrayType iarrayType2 = \u0004 as _IArrayType;
			if (iarrayType == null || iarrayType2 == null)
			{
				return \u0003;
			}
			ICompiledType compiledType = global::\u0004.\u0014.\u0001(\u0002, iarrayType.BaseType, iarrayType2.BaseType, \u0005);
			if (compiledType == null)
			{
				return null;
			}
			_IArrayType iarrayType3 = global::\u0019.\u0003.\u0001(compiledType as _IType);
			IList<_IArrayDimension> dimensions = iarrayType._Dimensions;
			IList<_IArrayDimension> dimensions2 = iarrayType2._Dimensions;
			if (dimensions.Count != dimensions2.Count)
			{
				return null;
			}
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002);
			IScope5 scope2 = global::\u0007.\u0005.\u0001(\u0005);
			for (int i = 0; i < dimensions.Count; i++)
			{
				_IArrayDimension iarrayDimension = dimensions[i];
				_IArrayDimension iarrayDimension2 = dimensions2[i];
				bool flag;
				int val = iarrayDimension.LowerBorderInt(out flag, scope);
				if (!flag)
				{
					return null;
				}
				int val2 = iarrayDimension2.LowerBorderInt(out flag, scope2);
				if (!flag)
				{
					return null;
				}
				int val3 = iarrayDimension.UpperBorderInt(out flag, scope);
				if (!flag)
				{
					return null;
				}
				int val4 = iarrayDimension2.UpperBorderInt(out flag, scope2);
				if (!flag)
				{
					return null;
				}
				int num = Math.Max(val, val2);
				int num2 = Math.Min(val3, val4);
				if (num > num2)
				{
					return null;
				}
				iarrayType3.AddDimension(global::\u0019.\u0003.\u0001((long)num, TypeClass.Int), global::\u0019.\u0003.\u0001((long)num2, TypeClass.Int));
			}
			return iarrayType3;
		}

		// Token: 0x06003387 RID: 13191 RVA: 0x000C8D74 File Offset: 0x000C6F74
		internal static void \u0001(_ICompileContext \u0002, _IVariable \u0003, _ISignature \u0004, LStringBuilder \u0005, _ISignature \u0006, IScope5 \u0007, string \u0008, string \u000E, ICompiledType \u000F, ICompiledType \u0010, _ICompileContext \u0011, int \u0012 = 0)
		{
			TypeClass @class = \u000F.Class;
			string text;
			if (@class == TypeClass.Reference)
			{
				text = string.Format("{0} REF= __COPY.{1};", \u0008, \u000E);
				\u0005.Append(text);
				return;
			}
			if (@class != TypeClass.Array)
			{
				if (@class == TypeClass.Userdef)
				{
					_IUserdefType iuserdefType = \u000F as _IUserdefType;
					ISignature signature = iuserdefType.GetSignature(\u0007);
					Debug.\u0001(signature != null);
					string copyFunctionName = IdentifierConstants.GetCopyFunctionName((_ISignature)signature);
					if (\u0002[copyFunctionName] != null)
					{
						text = string.Format("COPY__FB__{0}__{1}(pinstNew := ADR({2}), pinstOld := ADR(__COPY.{3}));", new object[]
						{
							signature.Name,
							iuserdefType.SignatureId,
							\u0008,
							\u000E
						});
						\u0005.Append(text);
						global::\u0004.\u0014.\u0001(\u0002, \u0005, \u0003, \u000F, \u0006, \u0007, \u0008, \u0012);
						return;
					}
				}
				text = string.Format("{0} := __COPY.{1};", \u0008, \u000E);
				\u0005.Append(text);
				global::\u0004.\u0014.\u0001(\u0002, \u0005, \u0003, \u000F, \u0006, \u0007, \u0008, \u0012);
				return;
			}
			_IArrayType u = \u000F as _IArrayType;
			_IArrayType u2 = \u0010 as _IArrayType;
			_IArrayType iarrayType = global::\u0004.\u0014.\u0001(\u0002, \u000F, \u0010, \u0011) as _IArrayType;
			if (iarrayType == null)
			{
				return;
			}
			int u3 = Helper.\u0001(iarrayType);
			_IType u000F = \u0084.\u0004.\u0001(u) as _IType;
			_IType u4 = \u0084.\u0004.\u0001(u2) as _IType;
			string text2 = string.Empty;
			Helper.\u0001(u3, \u0006, null);
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.Append(\u0008);
			Helper.\u0001(iarrayType, lstringBuilder, \u0012);
			string u5 = lstringBuilder.ToString();
			lstringBuilder = new LStringBuilder();
			lstringBuilder.Append(\u000E);
			Helper.\u0001(iarrayType, lstringBuilder, \u0012);
			string u000E = lstringBuilder.ToString();
			lstringBuilder = new LStringBuilder();
			global::\u0004.\u0014.\u0001(\u0002, \u0003, \u0004, lstringBuilder, \u0006, \u0007, u5, u000E, u000F, u4, \u0011, \u0012 + 1);
			text = lstringBuilder.ToString();
			lstringBuilder = new LStringBuilder();
			Helper.\u0001(iarrayType, text, lstringBuilder, \u0007, \u0012);
			text2 = lstringBuilder.ToString();
			\u0005.AppendLine();
			\u0005.Append(text2);
		}

		// Token: 0x06003388 RID: 13192 RVA: 0x000C8F68 File Offset: 0x000C7168
		internal static void \u0002(_ICompileContext \u0002, _ICompileContext \u0003, bool \u0004)
		{
			IList<_ISignature> allSignatureList = \u0002.AllSignatureList;
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
			foreach (_ISignature isignature in allSignatureList)
			{
				if (isignature.HasAttribute("contains_no_copy") && !dictionary2.ContainsKey(isignature.Id))
				{
					dictionary2[isignature.Id] = isignature.Id;
				}
				if (dictionary2.ContainsKey(isignature.Id))
				{
					foreach (int num in isignature.DeclarerIds)
					{
						dictionary2[num] = num;
					}
				}
			}
			if (dictionary2.Count > 0)
			{
				for (int j = allSignatureList.Count - 1; j >= 0; j--)
				{
					_ISignature isignature2 = allSignatureList[j];
					_ISignature isignature3 = null;
					if (\u0003 != null)
					{
						isignature3 = \u0003[isignature2.Id];
					}
					if ((isignature2.POUType == Operator.FunctionBlock || isignature2.GetFlag(SignatureFlag.Structure)) && isignature3 != null && isignature2.GetFlag(SignatureFlag.OnlineChanged))
					{
						dictionary[isignature2.Id] = isignature2.Id;
					}
					if (dictionary2.ContainsKey(isignature2.Id))
					{
						foreach (int key in isignature2.DeclarerIds)
						{
							if (dictionary.ContainsKey(key) && !dictionary.ContainsKey(isignature2.Id))
							{
								dictionary[isignature2.Id] = isignature2.Id;
								break;
							}
						}
					}
				}
			}
			foreach (_ISignature isignature4 in allSignatureList)
			{
				_ISignature isignature5 = null;
				if (\u0003 != null)
				{
					isignature5 = \u0003[isignature4.Id];
				}
				if ((isignature4.POUType == Operator.FunctionBlock || isignature4.GetFlag(SignatureFlag.Structure)) && isignature5 != null && (isignature4.GetFlag(SignatureFlag.OnlineChanged) || dictionary.ContainsKey(isignature4.Id)))
				{
					global::\u0004.\u0014.\u0001(\u0002, isignature4, isignature5, \u0003);
				}
			}
		}

		// Token: 0x06003389 RID: 13193 RVA: 0x000C91A0 File Offset: 0x000C73A0
		internal static void \u0001(_ICompileContext \u0002, LStringBuilder \u0003, _ISignature \u0004, _ISignature \u0005, _ISignature \u0006, IScope5 \u0007, _ICompileContext \u0008)
		{
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0008);
			IScope5 scope2 = global::\u0007.\u0005.\u0001(\u0002);
			_ISignature isignature = \u0007[\u0004.BaseSignatureId] as _ISignature;
			_ISignature isignature2 = scope[\u0005.BaseSignatureId] as _ISignature;
			if (isignature != null && isignature2 != null)
			{
				global::\u0004.\u0014.\u0001(\u0002, \u0003, isignature, isignature2, \u0006, \u0007, \u0008);
			}
			foreach (_IVariable ivariable in \u0004.AllVariables)
			{
				if (!ivariable.GetFlag(VarFlag.Temp) && !ivariable.GetFlag(VarFlag.Absolut) && !ivariable.GetFlag(VarFlag.NoCopy) && !ivariable.GetFlag(VarFlag.ReplacedConstant) && !ivariable.GetFlag(VarFlag.Constant) && !ivariable.IsProperty && !ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_NO_COPY) && !ivariable.GetFlag(VarFlag.Inout) && !ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY))
				{
					IVariable variable = \u0005[ivariable.Id];
					if (variable != null && global::\u0006.\u0011.\u0001(variable.CompiledType, ivariable.CompiledType, scope, scope2) && (!(variable.CompiledType is _IUserdefType) || (variable.CompiledType as _IUserdefType).GetSignature(scope2) != null) && (ivariable.CompiledType.Class != TypeClass.Reference || variable.CompiledType.Class == TypeClass.Reference) && (variable.CompiledType.Class != TypeClass.Reference || ivariable.CompiledType.Class == TypeClass.Reference))
					{
						if (ivariable.CompiledType is _IUserdefType && variable.CompiledType is _IUserdefType)
						{
							ISignature signature = (ivariable.CompiledType as _IUserdefType).GetSignature(scope2);
							ISignature signature2 = (variable.CompiledType as _IUserdefType).GetSignature(scope2);
							if ((signature.POUType == Operator.Interface || signature.GetFlag(SignatureFlag.ImplicitInterfaceUnion)) && signature2.POUType != Operator.Interface && !signature2.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
							{
								continue;
							}
						}
						string u = "pinstNew^." + ivariable.OrgName;
						string u000E = "pinstOld^." + variable.OrgName;
						global::\u0004.\u0014.\u0001(\u0002, ivariable, \u0004, \u0003, \u0006, \u0007, u, u000E, ivariable.CompiledType, variable.CompiledType, \u0008, 0);
					}
				}
			}
		}

		// Token: 0x0600338A RID: 13194 RVA: 0x000C9434 File Offset: 0x000C7634
		internal static _ICompiledPOU \u0001(_ICompileContext \u0002, _ISignature \u0003, _ISignature \u0004, _ICompileContext \u0005)
		{
			string str;
			if (!string.IsNullOrEmpty(\u0003.LibraryPath))
			{
				_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(\u0003.LibraryPath);
				str = Helper.\u0001(\u0002, libraryContext) + "." + \u0003.Name;
			}
			else
			{
				str = \u0003.Name;
			}
			string copyFunctionName = IdentifierConstants.GetCopyFunctionName(\u0003);
			_ISignature isignature = ParserHelper.\u0001("FUNCTION " + copyFunctionName + " : BOOL" + Environment.NewLine + "VAR_INPUT" + Environment.NewLine + "\tpinstNew : POINTER TO " + str + ";" + Environment.NewLine + "\tpinstOld : POINTER TO " + str + ";" + Environment.NewLine + "END_VAR", true);
			isignature.SetFlag(SignatureFlag.Generated, true);
			isignature = isignature.CreateCompiledSignature(null, \u0002.HasByteSupport());
			SignatureFlag signatureFlag = SignatureFlag.ToRemoveAfterDownload | SignatureFlag.NoCompareWithNew;
			signatureFlag |= SignatureFlag.SuperGlobal;
			isignature.SetFlag(signatureFlag, true);
			\u0002.AddSignature(isignature, null, null, true);
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002, isignature.Id);
			global::\u0014.\u0013.\u0002(isignature, scope, \u0002);
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.AppendLine("{implicit on}");
			lstringBuilder.Append("{nobp}");
			global::\u0004.\u0014.\u0001(\u0002, lstringBuilder, \u0003, \u0004, isignature, scope, \u0005);
			lstringBuilder.Append("{bp}");
			lstringBuilder.AppendLine("{implicit off}");
			Locator.\u0001(isignature, null, \u0002, null);
			_IStatement parseTree = new global::\u0011.\u0006(lstringBuilder.ToString(), true).\u0001();
			_ICompiledPOU icompiledPOU = global::\u0019.\u0003.\u0001(copyFunctionName);
			icompiledPOU.SetParseTree(parseTree);
			\u0002.AddCompiledPOU(icompiledPOU, isignature, null);
			ExpressionTypifierWithSpecialTasks visitor = new ExpressionTypifierWithSpecialTasks(scope, \u0002, false, icompiledPOU);
			icompiledPOU.Accept(visitor);
			TypeCheckerVisitor visitor2 = new TypeCheckerVisitor(scope, \u0002)
			{
				MessageSuppressionController = OnlineChangeSuppressions.Instance
			};
			icompiledPOU.Accept(visitor2);
			icompiledPOU.SignatureId = isignature.Id;
			icompiledPOU.SetFlag(CompiledPOUFlags.ToRemoveAfterDownload, true);
			icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile, true);
			icompiledPOU.SetFlag(CompiledPOUFlags.Typified, true);
			return icompiledPOU;
		}

		// Token: 0x0600338B RID: 13195 RVA: 0x000C9638 File Offset: 0x000C7838
		internal static _ISignature \u0001(_ICompileContext \u0002, _ICompileContext \u0003)
		{
			string text = "GLOBAL__COPY__CODE";
			_ISignature isignature = ParserHelper.\u0001(string.Concat(new string[]
			{
				"FUNCTION ",
				text,
				" : BOOL",
				Environment.NewLine,
				"VAR bInitRetains : BOOL := FALSE; bInCopyCode : BOOL := TRUE; END_VAR"
			}), true);
			isignature.SetFlag(SignatureFlag.Generated, true);
			isignature = isignature.CreateCompiledSignature(null, \u0002.HasByteSupport());
			isignature.SetFlag(SignatureFlag.ToRemoveAfterDownload | SignatureFlag.NoCompareWithNew, true);
			\u0002.AddSignature(isignature, null, null, true);
			IScope5 u = global::\u0007.\u0005.\u0001(\u0002, isignature.Id);
			global::\u0014.\u0013.\u0002(isignature, u, \u0002);
			Locator.\u0001(\u0002, isignature, null);
			return isignature;
		}

		// Token: 0x0600338C RID: 13196 RVA: 0x000C96D4 File Offset: 0x000C78D4
		internal static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003, _ISequenceStatement \u0004, _ISignature \u0005, _ISignature \u0006)
		{
			if (\u0006 != null && (\u0005.HasAttribute("init_inputs_on_onlchange") || \u0002.IsDefined("init_inputs_on_onlchange")))
			{
				if (\u0005.POUType == Operator.FunctionBlock)
				{
					global::\u0004.\u0014.\u0003(\u0002, \u0003, \u0004, \u0005, \u0006);
					return;
				}
				if (\u0005.POUType == Operator.Program)
				{
					global::\u0004.\u0014.\u0002(\u0002, \u0003, \u0004, \u0005, \u0006);
				}
			}
		}

		// Token: 0x0600338D RID: 13197 RVA: 0x000C972C File Offset: 0x000C792C
		private static IExpression \u0001(string \u0002)
		{
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(\u0002, false, false, false, false);
			return (APEnvironmentFacade.Instance.LanguageModelMgr.CreateParser(scanner) as IParser4).ParseExpression();
		}

		// Token: 0x0600338E RID: 13198 RVA: 0x000C9768 File Offset: 0x000C7968
		private static void \u0002(_ICompileContext \u0002, _ICompileContext \u0003, _ISequenceStatement \u0004, _ISignature \u0005, _ISignature \u0006)
		{
			List<IVariable> list = new List<IVariable>();
			string attributeValue = \u0005.GetAttributeValue("@callattribute");
			string attributeValue2 = \u0006.GetAttributeValue("@callattribute");
			if (attributeValue != attributeValue2 && attributeValue != null && attributeValue2 != null)
			{
				_ICallExpression icallExpression = global::\u0004.\u0014.\u0001(attributeValue) as _ICallExpression;
				_ICallExpression icallExpression2 = global::\u0004.\u0014.\u0001(attributeValue2) as _ICallExpression;
				if (icallExpression == null || icallExpression2 == null)
				{
					return;
				}
				IList<_IExpression> inputs = icallExpression.Inputs;
				IList<_IExpression> inputs2 = icallExpression2.Inputs;
				foreach (_IExpression iexpression in global::\u0004.\u0014.\u0001(icallExpression2.Inputs, icallExpression.Inputs))
				{
					IVariable variable = \u0005[iexpression.ToString()];
					if (variable != null && variable.GetFlag(VarFlag.Input))
					{
						list.Add(variable);
					}
				}
				if (list.Count > 0)
				{
					IScope5 u = global::\u0007.\u0005.\u0001(\u0002);
					foreach (IVariable variable2 in list)
					{
						_IVariable ivariable = (_IVariable)variable2;
						string u2 = \u0005.Name + "." + ivariable.Name;
						_IStatement istatement = global::\u0004.\u0014.\u0001(ivariable, u2, u, \u0002);
						if (istatement != null)
						{
							\u0004.Add(istatement);
						}
					}
				}
			}
		}

		// Token: 0x0600338F RID: 13199 RVA: 0x000C98D0 File Offset: 0x000C7AD0
		private static IEnumerable<_IExpression> \u0001(IEnumerable<_IExpression> \u0002, IEnumerable<_IExpression> \u0003)
		{
			foreach (_IExpression iexpression in \u0002)
			{
				if (iexpression != null)
				{
					string text = iexpression.ToString();
					bool flag = false;
					foreach (_IExpression iexpression2 in \u0003)
					{
						if (iexpression2 != null)
						{
							string value = iexpression2.ToString();
							if (text.Equals(value, StringComparison.InvariantCultureIgnoreCase))
							{
								flag = true;
								break;
							}
						}
					}
					if (!flag)
					{
						yield return iexpression;
					}
				}
			}
			IEnumerator<_IExpression> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x06003390 RID: 13200 RVA: 0x000C98E8 File Offset: 0x000C7AE8
		private static _IStatement \u0001(_IVariable \u0002, string \u0003, IScope5 \u0004, _ICompileContext \u0005)
		{
			IExpression expression = \u0002.Initial;
			_IExprement iexprement = null;
			if (\u0002.Initial == null)
			{
				if (\u0002.Type.Class == TypeClass.Reference)
				{
					expression = global::\u0019.\u0003.\u0001(0L, TypeClass.UDInt);
				}
				else
				{
					IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(\u0003, false, false, false, false);
					_IExpression u = APEnvironmentFacade.Instance.LanguageModelMgr.CreateParser(scanner).ParseOperand() as _IExpression;
					bool flag;
					iexprement = \u0080.\u001A.\u0001(\u0002, false, \u0002._Type, u, \u0004, out flag, null, null, true, false, 0, \u0005);
					if (iexprement == null)
					{
						return null;
					}
					expression = (iexprement as _IExpression);
				}
			}
			_IStatement istatement;
			if (iexprement is _IStatement)
			{
				istatement = (iexprement as _IStatement);
			}
			else
			{
				if (expression == null)
				{
					return null;
				}
				IScanner scanner2 = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(\u0003, false, false, false, false);
				_IExpression iexpression = APEnvironmentFacade.Instance.LanguageModelMgr.CreateParser(scanner2).ParseOperand() as _IExpression;
				if (iexpression == null)
				{
					return null;
				}
				_IAssignmentExpression iassignmentExpression = global::\u0019.\u0003.\u0001(iexpression, Token.Empty);
				iassignmentExpression._RValue = (expression as _IExpression);
				if (\u0002.Type.Class == TypeClass.Reference)
				{
					iassignmentExpression.KindOf = Operator.RefAssign;
				}
				istatement = global::\u0019.\u0003.\u0001(iassignmentExpression, Token.Empty);
			}
			istatement.Accept(new ExpressionTypifierWithSpecialTasks(\u0004, \u0005, false, null)
			{
				TreatReferenceAsPointer = true,
				InterfaceAsInterface = true
			});
			istatement.Accept(new TypeCheckerVisitor(\u0004, \u0005, true)
			{
				TreatReferenceAsPointer = true,
				InterfaceAsInterface = true
			});
			ErrorVisitor errorVisitor = new ErrorVisitor();
			istatement.Accept(errorVisitor);
			Debug.\u0001(!Helper.\u0001(errorVisitor.MessageList, Severity.Error));
			return istatement;
		}

		// Token: 0x06003391 RID: 13201 RVA: 0x000C9A78 File Offset: 0x000C7C78
		private static void \u0003(_ICompileContext \u0002, _ICompileContext \u0003, _ISequenceStatement \u0004, _ISignature \u0005, _ISignature \u0006)
		{
			foreach (int nId in \u0005.DeclarerIds)
			{
				_ISignature isignature = \u0002[nId];
				if (isignature.POUType == Operator.FunctionBlock || isignature.POUType == Operator.Program || isignature.POUType == Operator.VarGlobal || isignature.POUType == Operator.Type)
				{
					_ISignature isignature2 = \u0003[nId];
					if (isignature2 != null)
					{
						List<IVariable> list = new List<IVariable>();
						foreach (IVariable variable in isignature.AllVariables)
						{
							if (variable.Type is _IUserdefType && (variable.Type as _IUserdefType).SignatureId == \u0005.Id)
							{
								IVariable variable2 = isignature2[variable.Id];
								if (variable2 != null)
								{
									string attributeValue = variable.GetAttributeValue("@callattribute");
									string attributeValue2 = variable2.GetAttributeValue("@callattribute");
									if (attributeValue != attributeValue2 && attributeValue != null && attributeValue2 != null)
									{
										_ICallExpression icallExpression = global::\u0004.\u0014.\u0001(attributeValue) as _ICallExpression;
										_ICallExpression icallExpression2 = global::\u0004.\u0014.\u0001(attributeValue2) as _ICallExpression;
										if (icallExpression != null && icallExpression2 != null)
										{
											foreach (_IExpression iexpression in global::\u0004.\u0014.\u0001(icallExpression2.Inputs, icallExpression.Inputs))
											{
												IVariable variable3 = \u0005[iexpression.ToString()];
												if (variable3 != null && variable3.GetFlag(VarFlag.Input))
												{
													list.Add(variable3);
												}
											}
											if (list.Count > 0)
											{
												IEnumerable<string> enumerable = InstancePathService.\u0001(\u0002, isignature, new \u0080.\u0005.\u0001
												{
													\u0001 = true,
													\u0002 = false
												});
												IScope5 u = global::\u0007.\u0005.\u0001(\u0002);
												foreach (string text in enumerable)
												{
													foreach (IVariable variable4 in list)
													{
														_IVariable ivariable = (_IVariable)variable4;
														string u2 = string.Concat(new string[]
														{
															text,
															".",
															variable.Name,
															".",
															ivariable.Name
														});
														_IStatement istatement = global::\u0004.\u0014.\u0001(ivariable, u2, u, \u0002);
														if (istatement != null)
														{
															\u0004.Add(istatement);
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06003392 RID: 13202 RVA: 0x000C9D70 File Offset: 0x000C7F70
		internal static _ICompiledPOU \u0001(_ICompileContext \u0002, _ICompileContext \u0003, Codegeneration \u0004, bool \u0005, OnlineChangeDetails \u0006)
		{
			return new OnlineChangeCopyCodeGenerator(\u0002, \u0003, \u0005, \u0006, \u0004).\u0001();
		}
	}
}

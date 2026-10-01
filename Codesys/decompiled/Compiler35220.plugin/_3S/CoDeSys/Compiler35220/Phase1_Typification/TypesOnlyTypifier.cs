using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0004;
using \u0007;
using \u000E;
using \u0011;
using \u0019;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0084;

namespace _3S.CoDeSys.Compiler35220.Phase1_Typification
{
	// Token: 0x02000345 RID: 837
	internal sealed class TypesOnlyTypifier
	{
		// Token: 0x17000845 RID: 2117
		// (get) Token: 0x060032A9 RID: 12969 RVA: 0x000C2F10 File Offset: 0x000C1110
		private _ISignature Signature { get; }

		// Token: 0x17000846 RID: 2118
		// (get) Token: 0x060032AA RID: 12970 RVA: 0x000C2F18 File Offset: 0x000C1118
		private _ICompileContext Comcon
		{
			get
			{
				return this.CompileInformation.ComconNew;
			}
		}

		// Token: 0x17000847 RID: 2119
		// (get) Token: 0x060032AB RID: 12971 RVA: 0x000C2F28 File Offset: 0x000C1128
		private global::\u0004.\u0013 VarTypifier { get; }

		// Token: 0x17000848 RID: 2120
		// (get) Token: 0x060032AC RID: 12972 RVA: 0x000C2F30 File Offset: 0x000C1130
		private TypesOnlyTypifier.\u0001 EnumerationTypifier { get; }

		// Token: 0x17000849 RID: 2121
		// (get) Token: 0x060032AD RID: 12973 RVA: 0x000C2F38 File Offset: 0x000C1138
		private global::\u000E.\u001B CompileInformation { get; }

		// Token: 0x1700084A RID: 2122
		// (get) Token: 0x060032AE RID: 12974 RVA: 0x000C2F40 File Offset: 0x000C1140
		private TypesOnlyTypifier.\u0002 BaseExpressionTypifier { get; }

		// Token: 0x1700084B RID: 2123
		// (get) Token: 0x060032AF RID: 12975 RVA: 0x000C2F48 File Offset: 0x000C1148
		private TypesOnlyTypifier.\u0003 InterfaceExpressionTypifier { get; }

		// Token: 0x060032B0 RID: 12976 RVA: 0x000C2F50 File Offset: 0x000C1150
		internal TypesOnlyTypifier(_ISignature sign, global::\u000E.\u001B ci)
		{
			this.Signature = sign;
			this.CompileInformation = ci;
			this.VarTypifier = new global::\u0004.\u0013(sign, ci);
			this.EnumerationTypifier = new TypesOnlyTypifier.\u0001(sign, this.VarTypifier);
			this.BaseExpressionTypifier = new TypesOnlyTypifier.\u0002(sign, this.VarTypifier);
			this.InterfaceExpressionTypifier = new TypesOnlyTypifier.\u0003(sign);
		}

		// Token: 0x060032B1 RID: 12977 RVA: 0x000C2FB0 File Offset: 0x000C11B0
		internal bool \u0001(IScope5 \u0002)
		{
			if (this.Signature.GetFlagInternal(SignatureFlagInternal.VariablesTypified))
			{
				return true;
			}
			global::\u0007.\u0012.\u0001(this.Signature, this.Comcon);
			this.\u0001(\u0002);
			IList<_IVariable> allVariables = this.Signature.AllVariables;
			if (this.\u0001(allVariables, \u0002))
			{
				return true;
			}
			this.BaseExpressionTypifier.\u0001(\u0002);
			this.InterfaceExpressionTypifier.\u0001(\u0002);
			if (this.Signature.HasAttribute(CompileAttributes.ATTRIBUTE_CALL_AFTER_INIT) && this.Signature.POUType == Operator.Method && this.Signature.AllInputs.Length > 1)
			{
				this.Signature.AddMessage(Severity.Error, MessageId.Err_CallAfterInitHasInputs, new object[]
				{
					CompileAttributes.ATTRIBUTE_CALL_AFTER_INIT
				});
			}
			this.\u0001(\u0002, allVariables);
			_IScope u = \u0002 as _IScope;
			if (this.Signature.HasAttribute(InternalAttributes.INHERITED_FBINIT_LIBPATH))
			{
				string attributeValue = this.Signature.GetAttributeValue(InternalAttributes.INHERITED_FBINIT_LIBPATH);
				u = ((_IScope2)\u0002).CreateLibraryScope(attributeValue);
			}
			this.EnumerationTypifier.\u0001(allVariables, u, \u0002);
			this.\u0001(allVariables, u, \u0002);
			this.Signature.SetFlag(SignatureFlag.Typified, true);
			this.Signature.SetFlagInternal(SignatureFlagInternal.VariablesTypified, true);
			return true;
		}

		// Token: 0x060032B2 RID: 12978 RVA: 0x000C30E8 File Offset: 0x000C12E8
		private void \u0001(IList<_IVariable> \u0002, _IScope \u0003, IScope5 \u0004)
		{
			if (this.Signature.GetFlag(SignatureFlag.Enum))
			{
				return;
			}
			IVariable variable = this.Signature[IdentifierConstants.InstancePointer];
			if (variable != null)
			{
				int signatureId = ((IUserdefType)((IPointerType)variable.Type).Base).SignatureId;
				_ISignature isignature = (_ISignature)this.Comcon.GetSignatureById(signatureId);
				if (isignature != null)
				{
					isignature.AddReferencer(this.Signature.Id);
				}
			}
			foreach (_IVariable ivariable in \u0002.Where(new Func<_IVariable, bool>(TypesOnlyTypifier.<>c.<>9.\u0001)))
			{
				this.VarTypifier.\u0001(ivariable, \u0003);
				if (this.Signature.GetFlag(SignatureFlag.Structure))
				{
					ICompiledType compiledType = ivariable.CompiledType;
					compiledType = \u0084.\u0004.\u0001(compiledType);
					if (compiledType != null && compiledType.Class == TypeClass.Userdef)
					{
						_ISignature isignature2 = \u0004[((_IUserdefType)compiledType).SignatureId] as _ISignature;
						if (isignature2 != null && (isignature2.POUType == Operator.FunctionBlock || isignature2.GetFlagInternal(SignatureFlagInternal.StructureContainsFBInstances)))
						{
							this.Signature.SetFlagInternal(SignatureFlagInternal.StructureContainsFBInstances, true);
						}
					}
				}
			}
		}

		// Token: 0x060032B3 RID: 12979 RVA: 0x000C3240 File Offset: 0x000C1440
		private void \u0001(IScope5 \u0002, IList<_IVariable> \u0003)
		{
			if (!this.Signature.GetFlag(SignatureFlag.Enum))
			{
				foreach (_IVariable ivariable in \u0003)
				{
					if (ivariable.GetFlag(VarFlag.Constant))
					{
						this.VarTypifier.\u0001(ivariable, \u0002);
					}
				}
			}
		}

		// Token: 0x060032B4 RID: 12980 RVA: 0x000C32A8 File Offset: 0x000C14A8
		private bool \u0001(IList<_IVariable> \u0002, IScope5 \u0003)
		{
			if (!this.Signature.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
			{
				return false;
			}
			Debug.\u0001(this.Signature.POUType == Operator.Type && this.Signature.GetFlag(SignatureFlag.Union));
			Debug.\u0001(\u0002.Count == 2);
			_IVariable ivariable = this.Signature["__Interface"] as _IVariable;
			_IVariable ivariable2 = this.Signature["__vfTablePointer"] as _IVariable;
			if (ivariable == null || ivariable2 == null || ivariable.Type == null || ivariable.CompiledType.Class != TypeClass.Pointer || ivariable.CompiledType.BaseType.Class != TypeClass.Userdef)
			{
				throw new InvalidOperationException();
			}
			IUserdefType2 userdefType = (IUserdefType2)ivariable.CompiledType.BaseType;
			ISignature[] array = \u0003.FindSignature(userdefType.NameExpression);
			Debug.\u0001(array != null && array.Length == 1);
			if (array != null)
			{
				((_IUserdefType)userdefType).SignatureId = array[0].Id;
			}
			this.VarTypifier.\u0001(ivariable2, \u0003);
			this.Signature.SetFlag(SignatureFlag.Typified, true);
			return true;
		}

		// Token: 0x060032B5 RID: 12981 RVA: 0x000C33CC File Offset: 0x000C15CC
		private void \u0001(IScope5 \u0002)
		{
			foreach (_IVariable ivariable in this.Signature.AllExternals.OfType<_IVariable>())
			{
				_IExpression iexpression = global::\u0019.\u0003.Builder.ParseExpression(ivariable.VersionedName) as _IExpression;
				if (iexpression != null)
				{
					iexpression.PositionIntern = global::\u0019.\u0003.\u0001(ivariable._SourcePosition);
					ExpressionTypifierWithSpecialTasks ivisit = new ExpressionTypifierWithSpecialTasks(\u0002, this.Comcon, true, this.Signature, ivariable)
					{
						ContributeToCompile = true
					};
					TypeCheckerVisitor ivisit2 = new TypeCheckerVisitor(\u0002, this.Comcon);
					ErrorVisitor errorVisitor = new ErrorVisitor();
					iexpression.Accept(ivisit);
					iexpression.Accept(ivisit2);
					iexpression.Accept(errorVisitor);
					using (IEnumerator<_ICompilerMessage> enumerator2 = errorVisitor.MessageList.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							_ICompilerMessage icompilerMessage = enumerator2.Current;
							uint? number = icompilerMessage.Number;
							uint num = 46U;
							if (number.GetValueOrDefault() == num & number != null)
							{
								this.Signature.\u0001(ivariable.SourcePosition, Severity.Error, MessageId.Err_NoVarForExternal, new object[]
								{
									ivariable.OrgName
								});
							}
							else
							{
								this.Signature.AddMessage(icompilerMessage);
							}
						}
						goto IL_144;
					}
					goto IL_11E;
				}
				goto IL_11E;
				IL_144:
				if (ivariable.Initial != null)
				{
					this.Signature.\u0001(ivariable.SourcePosition, Severity.Error, MessageId.Err_NoInitialForExternal, new object[]
					{
						ivariable.OrgName
					});
					continue;
				}
				continue;
				IL_11E:
				this.Signature.\u0001(ivariable.SourcePosition, Severity.Error, MessageId.Err_NoVarForExternal, new object[]
				{
					ivariable.OrgName
				});
				goto IL_144;
			}
		}

		// Token: 0x04000987 RID: 2439
		[CompilerGenerated]
		private readonly _ISignature \u0001;

		// Token: 0x04000988 RID: 2440
		[CompilerGenerated]
		private readonly global::\u0004.\u0013 \u0001;

		// Token: 0x04000989 RID: 2441
		[CompilerGenerated]
		private readonly TypesOnlyTypifier.\u0001 \u0001;

		// Token: 0x0400098A RID: 2442
		[CompilerGenerated]
		private readonly global::\u000E.\u001B \u0001;

		// Token: 0x0400098B RID: 2443
		[CompilerGenerated]
		private readonly TypesOnlyTypifier.\u0002 \u0001;

		// Token: 0x0400098C RID: 2444
		[CompilerGenerated]
		private readonly TypesOnlyTypifier.\u0003 \u0001;

		// Token: 0x02000346 RID: 838
		private sealed class \u0001
		{
			// Token: 0x1700084C RID: 2124
			// (get) Token: 0x060032B6 RID: 12982 RVA: 0x000C3598 File Offset: 0x000C1798
			private _ISignature Signature { get; }

			// Token: 0x1700084D RID: 2125
			// (get) Token: 0x060032B7 RID: 12983 RVA: 0x000C35A0 File Offset: 0x000C17A0
			private global::\u0004.\u0013 VarTypifier { get; }

			// Token: 0x060032B8 RID: 12984 RVA: 0x000C35A8 File Offset: 0x000C17A8
			internal \u0001(_ISignature \u001C\u0002, global::\u0004.\u0013 \u000E\u0005)
			{
				this.Signature = \u001C\u0002;
				this.VarTypifier = \u000E\u0005;
			}

			// Token: 0x060032B9 RID: 12985 RVA: 0x000C35C0 File Offset: 0x000C17C0
			internal void \u0001(IList<_IVariable> \u0002, _IScope \u0003, IScope5 \u0004)
			{
				if (!this.Signature.GetFlag(SignatureFlag.Enum))
				{
					return;
				}
				this.\u0001(\u0002);
				IntegerUnion u = new IntegerUnion
				{
					m_long = 0L
				};
				_IExpression u2 = null;
				foreach (_IVariable ivariable in \u0002)
				{
					bool u3 = TypeTable.IsSigned(ivariable.CompiledType.BaseType.Class);
					bool u4 = ivariable.Initial == null && TypesOnlyTypifier.\u0001.\u0001(u, u2, ivariable, u3, false);
					this.VarTypifier.\u0001(ivariable, \u0003);
					if (ivariable.Initial != null)
					{
						TypesOnlyTypifier.\u0001.\u0001(\u0004, ref u, ref u2, ivariable, u4);
					}
					u.m_long += 1L;
				}
			}

			// Token: 0x060032BA RID: 12986 RVA: 0x000C3694 File Offset: 0x000C1894
			private void \u0001(IList<_IVariable> \u0002)
			{
				if (\u0002.Count > 0)
				{
					_IEnumType ienumType = (_IEnumType)\u0002[0]._Type;
					bool flag = TypeTable.IsInteger(ienumType._Base.Class);
					bool flag2 = ienumType._Base.Class == TypeClass.Pointer;
					if (!flag && !flag2)
					{
						this.Signature.AddMessage(Severity.Error, MessageId.Err_InvalidEnumBaseType, Array.Empty<object>());
					}
				}
			}

			// Token: 0x060032BB RID: 12987 RVA: 0x000C36F8 File Offset: 0x000C18F8
			private static void \u0001(IScope5 \u0002, ref IntegerUnion \u0003, ref _IExpression \u0004, _IVariable \u0005, bool \u0006)
			{
				ILiteralValue literalValue = ((_IExpression)\u0005.Initial).Literal(\u0002, true);
				if (literalValue != null)
				{
					if (literalValue.KindOf == KindOfLiteral.SignedInteger)
					{
						\u0003.m_long = literalValue.SignedLong;
					}
					else if (literalValue.KindOf == KindOfLiteral.UnsignedInteger)
					{
						\u0003.m_ulong = literalValue.UnsignedLong;
					}
					\u0004 = null;
					return;
				}
				if (!\u0006)
				{
					\u0004 = (\u0005.Initial as _IExpression);
					\u0003.m_long = 0L;
				}
			}

			// Token: 0x060032BC RID: 12988 RVA: 0x000C3764 File Offset: 0x000C1964
			private static bool \u0001(IntegerUnion \u0002, _IExpression \u0003, _IVariable \u0004, bool \u0005, bool \u0006)
			{
				_IExpression iexpression;
				if (\u0005)
				{
					iexpression = global::\u0019.\u0003.\u0001(\u0002.m_long);
				}
				else
				{
					iexpression = global::\u0019.\u0003.\u0001(\u0002.m_ulong);
				}
				_IExpression initial = iexpression;
				if (\u0003 != null)
				{
					initial = global::\u0019.\u0003.\u0001(Operator.Plus, \u0003, iexpression);
					\u0006 = true;
				}
				\u0004.SetInitial(initial);
				return \u0006;
			}

			// Token: 0x0400098D RID: 2445
			[CompilerGenerated]
			private readonly _ISignature \u0001;

			// Token: 0x0400098E RID: 2446
			[CompilerGenerated]
			private readonly global::\u0004.\u0013 \u0001;
		}

		// Token: 0x02000347 RID: 839
		private sealed class \u0002
		{
			// Token: 0x1700084E RID: 2126
			// (get) Token: 0x060032BD RID: 12989 RVA: 0x000C37AC File Offset: 0x000C19AC
			private _ISignature Signature { get; }

			// Token: 0x1700084F RID: 2127
			// (get) Token: 0x060032BE RID: 12990 RVA: 0x000C37B4 File Offset: 0x000C19B4
			private global::\u0004.\u0013 VarTypifier { get; }

			// Token: 0x060032BF RID: 12991 RVA: 0x000C37BC File Offset: 0x000C19BC
			internal \u0002(_ISignature \u001C\u0002, global::\u0004.\u0013 \u008F\u0005)
			{
				this.Signature = \u001C\u0002;
				this.VarTypifier = \u008F\u0005;
			}

			// Token: 0x060032C0 RID: 12992 RVA: 0x000C37D4 File Offset: 0x000C19D4
			internal void \u0001(IScope5 \u0002)
			{
				if (this.Signature.BaseExpression != null)
				{
					ErrorVisitor errorVisitor = new ErrorVisitor();
					_IVariable ivariable = this.\u0001();
					this.VarTypifier.\u0001(ivariable, \u0002, errorVisitor, this.Signature.BaseExpression.Position as _ISourcePosition);
					if (errorVisitor.MessageList.Any<_ICompilerMessage>())
					{
						foreach (_ICompilerMessage message in errorVisitor.MessageList)
						{
							this.Signature.AddMessage(message);
						}
					}
					ISignature signature = \u0002[((_IUserdefType)ivariable.Type).SignatureId];
					_ISignature isignature = signature as _ISignature;
					if (isignature != null)
					{
						isignature.AddDeclarer(this.Signature.Id);
					}
					signature = Helper.\u0001(signature as _ISignature, \u0002);
					if (signature != null && signature.POUType == this.Signature.POUType)
					{
						this.\u0003(signature);
						this.Signature.SetBaseSignatureId(signature.Id);
						this.\u0001(signature);
						if (this.Signature.GetFlag(SignatureFlag.Structure) && (signature as _ISignature).GetFlagInternal(SignatureFlagInternal.StructureContainsFBInstances))
						{
							this.Signature.SetFlagInternal(SignatureFlagInternal.StructureContainsFBInstances, true);
						}
					}
					else
					{
						this.Signature.AddMessage(Severity.Error, MessageId.Err_BaseClassNotFound, new object[]
						{
							this.Signature.BaseExpression
						});
					}
					this.\u0002(\u0002);
					this.\u0004(signature);
				}
			}

			// Token: 0x060032C1 RID: 12993 RVA: 0x000C3954 File Offset: 0x000C1B54
			private _IVariable \u0001()
			{
				_IVariable ivariable = global::\u0019.\u0003.\u0001(this.Signature.BaseExpression.Position);
				ivariable.Name = "__BaseClass";
				ITypeExpression typeExpression = this.Signature.BaseExpression as ITypeExpression;
				_IUserdefType type;
				if (typeExpression != null)
				{
					type = (typeExpression.ExpressionType as _IUserdefType);
				}
				else
				{
					type = global::\u0019.\u0003.\u0001(this.Signature.BaseExpression as _IExpression);
				}
				ivariable.SetType(type);
				return ivariable;
			}

			// Token: 0x060032C2 RID: 12994 RVA: 0x000C39C0 File Offset: 0x000C1BC0
			private void \u0001(ISignature \u0002)
			{
				if (this.Signature.POUType != Operator.Type)
				{
					return;
				}
				if (!this.Signature.GetFlag(SignatureFlag.Structure) || this.Signature.GetFlag(SignatureFlag.Union))
				{
					this.\u0002(this.Signature);
					return;
				}
				if (!\u0002.GetFlag(SignatureFlag.Structure) || \u0002.GetFlag(SignatureFlag.Union))
				{
					this.\u0002(\u0002);
				}
			}

			// Token: 0x060032C3 RID: 12995 RVA: 0x000C3A2C File Offset: 0x000C1C2C
			private void \u0002(ISignature \u0002)
			{
				if (\u0002.GetFlag(SignatureFlag.Union))
				{
					Severity severity = this.\u0001(this.Signature);
					this.Signature.AddMessage(severity, MessageId.Wrn_NoInheritanceForUnions, new object[]
					{
						this.Signature.BaseExpression
					});
					return;
				}
				this.Signature.AddMessage(Severity.Error, MessageId.Err_CantHaveBaseClass, new object[]
				{
					this.Signature.BaseExpression
				});
			}

			// Token: 0x060032C4 RID: 12996 RVA: 0x000C3AA0 File Offset: 0x000C1CA0
			private Severity \u0001(ISignature \u0002)
			{
				if (Messages.\u0001((_ISignature)\u0002, MessageId.Wrn_NoInheritanceForUnions) == Severity.SuppressedWarning)
				{
					return Severity.SuppressedWarning;
				}
				IVariable[] all = \u0002.All;
				for (int i = 0; i < all.Length; i++)
				{
					if (Messages.\u0001((_IVariable)all[i], MessageId.Wrn_NoInheritanceForUnions) == Severity.SuppressedWarning)
					{
						return Severity.SuppressedWarning;
					}
				}
				return Severity.Warning;
			}

			// Token: 0x060032C5 RID: 12997 RVA: 0x000C3AF4 File Offset: 0x000C1CF4
			private void \u0003(ISignature \u0002)
			{
				string attributeValue = \u0002.GetAttributeValue(CompileAttributes.ATTRIBUTE_OBSOLETE);
				if (!string.IsNullOrEmpty(attributeValue))
				{
					this.Signature.AddMessage(Severity.Warning, MessageId.Wrn_Obsolete, new object[]
					{
						\u0002.OrgName,
						attributeValue
					});
				}
			}

			// Token: 0x060032C6 RID: 12998 RVA: 0x000C3B3C File Offset: 0x000C1D3C
			private void \u0004(ISignature \u0002)
			{
				if (\u0002 != null && \u0002.GetFlag(SignatureFlag.External) != this.Signature.GetFlag(SignatureFlag.External) && !\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_IGNORE_ERROR_361))
				{
					this.Signature.ResetBaseSignatureId();
					this.Signature.AddMessage(Severity.Error, MessageId.Err_NoMixExternalIECInheritance, Array.Empty<object>());
				}
			}

			// Token: 0x060032C7 RID: 12999 RVA: 0x000C3B94 File Offset: 0x000C1D94
			private void \u0002(IScope5 \u0002)
			{
				int baseSignatureId = this.Signature.BaseSignatureId;
				LStringBuilder lstringBuilder = new LStringBuilder();
				lstringBuilder.Append(this.Signature.OrgName);
				while (baseSignatureId != Helper.InvalidId)
				{
					_ISignature isignature = \u0002[baseSignatureId] as _ISignature;
					if (isignature == null)
					{
						break;
					}
					lstringBuilder.Append(" -> " + isignature.OrgName);
					if (baseSignatureId == this.Signature.Id)
					{
						this.Signature.ResetBaseSignatureId();
						this.Signature.AddMessage(Severity.Error, MessageId.Err_SelfInheritanceBase, new object[]
						{
							lstringBuilder.ToString()
						});
						return;
					}
					baseSignatureId = isignature.BaseSignatureId;
				}
			}

			// Token: 0x0400098F RID: 2447
			[CompilerGenerated]
			private readonly _ISignature \u0001;

			// Token: 0x04000990 RID: 2448
			[CompilerGenerated]
			private readonly global::\u0004.\u0013 \u0001;
		}

		// Token: 0x02000348 RID: 840
		private sealed class \u0003
		{
			// Token: 0x17000850 RID: 2128
			// (get) Token: 0x060032C8 RID: 13000 RVA: 0x000C3C34 File Offset: 0x000C1E34
			private _ISignature Signature { get; }

			// Token: 0x060032C9 RID: 13001 RVA: 0x000C3C3C File Offset: 0x000C1E3C
			internal \u0003(_ISignature \u001C\u0002)
			{
				this.Signature = \u001C\u0002;
			}

			// Token: 0x060032CA RID: 13002 RVA: 0x000C3C4C File Offset: 0x000C1E4C
			internal void \u0001(IScope5 \u0002)
			{
				if (this.Signature.InterfaceExpressions.Length != 0)
				{
					int[] array = new int[this.Signature.InterfaceExpressions.Length];
					int num = 0;
					bool flag = false;
					foreach (IExpression expression in this.Signature.InterfaceExpressions)
					{
						ISignature[] array2 = \u0002.FindSignature(expression);
						if (array2 != null && array2.Length > 1)
						{
							this.\u0001(expression, array2);
						}
						else if (array2 == null || array2.Length == 0)
						{
							this.Signature.AddMessage(Severity.Error, MessageId.Err_InterfaceNotFound, new object[]
							{
								expression.ToString()
							});
							flag = true;
						}
						else
						{
							_ISignature u = Helper.\u0001(array2[0] as _ISignature, \u0002);
							flag = !this.\u0001(array, ref num, expression, u);
						}
					}
					if (!flag)
					{
						this.Signature.SetInterfaceIds(array);
					}
				}
			}

			// Token: 0x060032CB RID: 13003 RVA: 0x000C3D28 File Offset: 0x000C1F28
			private bool \u0001(int[] \u0002, ref int \u0003, IExpression \u0004, _ISignature \u0005)
			{
				if (\u0005 != null && \u0005.POUType == Operator.Interface)
				{
					string attributeValue = \u0005.GetAttributeValue(CompileAttributes.ATTRIBUTE_OBSOLETE);
					if (!string.IsNullOrEmpty(attributeValue))
					{
						this.Signature.AddMessage(Severity.Warning, MessageId.Wrn_Obsolete, new object[]
						{
							\u0005.OrgName,
							attributeValue
						});
					}
					int num = \u0003;
					\u0003 = num + 1;
					\u0002[num] = \u0005.Id;
					\u0005.AddDeclarer(this.Signature.Id);
					return true;
				}
				this.Signature.AddMessage(Severity.Error, MessageId.Err_InterfaceNotFound, new object[]
				{
					\u0004.ToString()
				});
				return false;
			}

			// Token: 0x060032CC RID: 13004 RVA: 0x000C3DC4 File Offset: 0x000C1FC4
			private void \u0001(IExpression \u0002, ISignature[] \u0003)
			{
				this.Signature.AddMessage(Severity.Error, MessageId.Err_Ambiguity, new object[]
				{
					\u0002.ToString()
				});
				foreach (ISignature signature in \u0003)
				{
					_ISourcePosition isourcePosition = global::\u0019.\u0003.\u0001();
					isourcePosition.SetObjectIdentification(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(signature.LibraryPath), signature.ObjectGuid);
					this.Signature.AddMessage(isourcePosition, Severity.Information, MessageId.Inf_RelatedPosition, Array.Empty<object>());
				}
			}

			// Token: 0x04000991 RID: 2449
			[CompilerGenerated]
			private readonly _ISignature \u0001;
		}
	}
}

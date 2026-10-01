using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0006;
using \u000E;
using \u0011;
using \u0017;
using \u0019;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0081;

namespace \u007F
{
	// Token: 0x02000326 RID: 806
	internal sealed class \u0011
	{
		// Token: 0x06003006 RID: 12294 RVA: 0x000B5A38 File Offset: 0x000B3C38
		public \u0011(\u001B \u008F\u0004)
		{
			this.CompileInformation = \u008F\u0004;
		}

		// Token: 0x170007FB RID: 2043
		// (get) Token: 0x06003007 RID: 12295 RVA: 0x000B5A48 File Offset: 0x000B3C48
		private \u001B CompileInformation { get; }

		// Token: 0x170007FC RID: 2044
		// (get) Token: 0x06003008 RID: 12296 RVA: 0x000B5A50 File Offset: 0x000B3C50
		private _ICompileContext ComconNew
		{
			get
			{
				return this.CompileInformation.ComconNew;
			}
		}

		// Token: 0x06003009 RID: 12297 RVA: 0x000B5A60 File Offset: 0x000B3C60
		internal void \u0001(_ISignature \u0002, IScope5 \u0003)
		{
			if (\u0002.POUType == Operator.Interface)
			{
				this.\u0002(\u0002, \u0003);
				return;
			}
			this.\u0003(\u0002, \u0003);
		}

		// Token: 0x0600300A RID: 12298 RVA: 0x000B5A80 File Offset: 0x000B3C80
		internal void \u0001(_ISignature \u0002)
		{
			if (\u0002.POUType != Operator.FunctionBlock)
			{
				if (\u0002.POUType != Operator.Interface && \u0002.BaseSignatureId != Helper.InvalidId && !\u0002.GetFlag(SignatureFlag.Structure))
				{
					\u0002.AddMessage(Severity.Error, MessageId.Err_InheritanceError, Array.Empty<object>());
				}
				if (\u0002.InterfaceExpressions != null && \u0002.InterfaceExpressions.Length != 0 && \u0002.POUType != Operator.Interface)
				{
					\u0002.AddMessage(Severity.Error, MessageId.Err_InterfaceImplementationError, Array.Empty<object>());
				}
			}
		}

		// Token: 0x0600300B RID: 12299 RVA: 0x000B5AF8 File Offset: 0x000B3CF8
		private void \u0002(_ISignature \u0002, IScope5 \u0003)
		{
			LList<_ISignature> llist = new LList<_ISignature>();
			global::\u0017.\u0001.\u0002(\u0002, \u0003, llist);
			foreach (_ISignature u in llist)
			{
				this.\u0001(\u0002, u, \u0003);
			}
		}

		// Token: 0x0600300C RID: 12300 RVA: 0x000B5B50 File Offset: 0x000B3D50
		private void \u0003(_ISignature \u0002, IScope5 \u0003)
		{
			int baseSignatureId = \u0002.BaseSignatureId;
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.Append(\u0002.OrgName);
			while (baseSignatureId != Helper.InvalidId)
			{
				_ISignature isignature = \u0003[baseSignatureId] as _ISignature;
				if (isignature == null)
				{
					break;
				}
				lstringBuilder.Append(" -> " + isignature.OrgName);
				if (baseSignatureId == \u0002.Id)
				{
					\u0002.ResetBaseSignatureId();
					\u0002.AddMessage(Severity.Error, MessageId.Err_SelfInheritanceBase, new object[]
					{
						lstringBuilder.ToString()
					});
					break;
				}
				if (isignature.GetFlag(SignatureFlag.Final))
				{
					\u0002.AddMessage(Severity.Error, MessageId.Err_NoInheritanceOnFinalType, new object[]
					{
						isignature.OrgName
					});
					_ISourcePosition sourcepos = global::\u0019.\u0003.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(isignature.LibraryPath), isignature.ObjectGuid, 0L, 0, 0);
					\u0002.AddMessage(sourcepos, Severity.Information, MessageId.Inf_RelatedPosition, Array.Empty<object>());
					break;
				}
				if (isignature.GetFlag(SignatureFlag.Internal) && !string.Equals(\u0002.LibraryPath, isignature.LibraryPath, StringComparison.OrdinalIgnoreCase))
				{
					\u0002.AddMessage(Severity.Error, MessageId.Err_AccessToInternalObject, new object[]
					{
						isignature.OrgName,
						isignature.LibraryPath
					});
					break;
				}
				this.\u0001(\u0002, isignature);
				this.\u0001(\u0002, isignature, \u0003);
				baseSignatureId = isignature.BaseSignatureId;
			}
			this.\u0001(\u0002, \u0002.InterfaceIds, \u0003, \u0002.Name, new LList<int>());
		}

		// Token: 0x0600300D RID: 12301 RVA: 0x000B5CC4 File Offset: 0x000B3EC4
		private void \u0001(_ISignature \u0002, _ISignature \u0003)
		{
			foreach (_IVariable ivariable in \u0003.AllVariables)
			{
				_IVariable ivariable2 = \u0002[ivariable.VersionedName] as _IVariable;
				if (ivariable2 != null && (!ivariable2.IsProperty || !ivariable.IsProperty) && !ivariable2.HasAttribute(CompileAttributes.ATTRIBUTE_IGNORE_DUPLICATE))
				{
					\u0002.AddMessage(Severity.Error, MessageId.Err_VariableOverride, new object[]
					{
						ivariable.VersionedName,
						\u0002.OrgName,
						\u0003.OrgName
					});
				}
				if (ivariable2 != null && ivariable2.IsProperty && ivariable.IsProperty && this.ComconNew.DataManager.PackMode != 8 && TypeTable.IsLType(ivariable2.Type.Class))
				{
					string attributeValue = ivariable2.GetAttributeValue(CompileAttributes.ATTRIBUTE_MONITORING);
					string attributeValue2 = ivariable.GetAttributeValue(CompileAttributes.ATTRIBUTE_MONITORING);
					if (attributeValue != attributeValue2)
					{
						\u0002.AddMessage(Severity.Error, MessageId.Err_InterfaceChangedBase, new object[]
						{
							ivariable2.OrgName,
							\u0003.OrgName
						});
					}
				}
			}
		}

		// Token: 0x0600300E RID: 12302 RVA: 0x000B5DE4 File Offset: 0x000B3FE4
		private bool \u0001(_ISignature \u0002, _ISignature \u0003, _ISignature \u0004, _ISignature \u0005)
		{
			if (\u0004.POUType != Operator.Method)
			{
				\u0002.AddMessage(Severity.Error, MessageId.Err_OnlyMethodsOverride, new object[]
				{
					Scanner.GetTextOfOperator(\u0004.POUType),
					\u0004.OrgName,
					\u0003.OrgName
				});
				return true;
			}
			if (\u0005.POUType != Operator.Method)
			{
				\u0002.AddMessage(Severity.Error, MessageId.Err_SomethingOverridingMethodBase, new object[]
				{
					Scanner.GetTextOfOperator(\u0005.POUType),
					\u0005.OrgName,
					\u0003.OrgName
				});
				return true;
			}
			return false;
		}

		// Token: 0x0600300F RID: 12303 RVA: 0x000B5E6C File Offset: 0x000B406C
		private bool \u0002(_ISignature \u0002, _ISignature \u0003, _ISignature \u0004, _ISignature \u0005)
		{
			if (\u0004.GetFlag(SignatureFlag.Final) || \u0004.GetFlag(SignatureFlag.Private))
			{
				if (\u0004.GetFlag(SignatureFlag.Final))
				{
					\u0005.AddMessage(Severity.Error, MessageId.Err_NoOverrideOnFinalMethod, new object[]
					{
						\u0002.OrgName,
						\u0003.OrgName,
						\u0004.OrgName
					});
				}
				else
				{
					\u0005.AddMessage(Severity.Error, MessageId.Err_NoOverrideOnPrivateMethod, new object[]
					{
						\u0002.OrgName,
						\u0003.OrgName,
						\u0004.OrgName
					});
				}
				_ISourcePosition sourcepos = global::\u0019.\u0003.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0004.LibraryPath), \u0004.ObjectGuid, 0L, 0, 0);
				\u0005.AddMessage(sourcepos, Severity.Information, MessageId.Inf_RelatedPosition, Array.Empty<object>());
				return true;
			}
			return false;
		}

		// Token: 0x06003010 RID: 12304 RVA: 0x000B5F50 File Offset: 0x000B4150
		private void \u0001(_ISignature \u0002, _ISignature \u0003, _ISignature \u0004, _ISignature \u0005)
		{
			if (\u0004.GetFlag(SignatureFlag.Internal) && !string.Equals(\u0004.LibraryPath, \u0005.LibraryPath, StringComparison.OrdinalIgnoreCase))
			{
				\u0005.AddMessage(Severity.Error, MessageId.Err_NoOverrideOnInternalMethod, new object[]
				{
					\u0002.OrgName,
					\u0003.OrgName,
					\u0004.OrgName,
					\u0004.LibraryPath
				});
				_ISourcePosition sourcepos = global::\u0019.\u0003.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0004.LibraryPath), \u0004.ObjectGuid, 0L, 0, 0);
				\u0005.AddMessage(sourcepos, Severity.Information, MessageId.Inf_RelatedPosition, Array.Empty<object>());
			}
		}

		// Token: 0x06003011 RID: 12305 RVA: 0x000B5FFC File Offset: 0x000B41FC
		private bool \u0003(_ISignature \u0002, _ISignature \u0003, _ISignature \u0004, _ISignature \u0005)
		{
			if (\u0004.GetFlag(SignatureFlag.Protected) == \u0005.GetFlag(SignatureFlag.Protected) && \u0004.GetFlag(SignatureFlag.Internal) == \u0005.GetFlag(SignatureFlag.Internal) && \u0004.GetFlag(SignatureFlag.Private) == \u0005.GetFlag(SignatureFlag.Private))
			{
				return false;
			}
			string textOfOperator = Scanner.GetTextOfOperator(Operator.Public);
			if (\u0004.GetFlag(SignatureFlag.Protected))
			{
				textOfOperator = Scanner.GetTextOfOperator(Operator.Protected);
			}
			else if (\u0004.GetFlag(SignatureFlag.Internal))
			{
				textOfOperator = Scanner.GetTextOfOperator(Operator.Internal);
			}
			if (this.\u0001(\u0004) && \u0005.GetFlag(SignatureFlag.Private))
			{
				\u0002.AddMessage(Severity.Warning, MessageId.Wrn_ChangeOfAccessModifier, new object[]
				{
					\u0002.OrgName,
					\u0005.OrgName,
					textOfOperator,
					\u0003.OrgName
				});
				return false;
			}
			if (!this.\u0002(\u0004))
			{
				\u0002.AddMessage(Severity.Error, MessageId.Err_NoChangeOnAccessModifier, new object[]
				{
					\u0002.OrgName,
					\u0005.OrgName,
					textOfOperator,
					\u0003.OrgName
				});
			}
			return true;
		}

		// Token: 0x06003012 RID: 12306 RVA: 0x000B6144 File Offset: 0x000B4344
		private bool \u0001(_ISignature \u0002)
		{
			return !\u0002.GetFlag(SignatureFlag.Private) && !\u0002.GetFlag(SignatureFlag.Protected) && !\u0002.GetFlag(SignatureFlag.Internal);
		}

		// Token: 0x06003013 RID: 12307 RVA: 0x000B617C File Offset: 0x000B437C
		private void \u0001(_ISignature \u0002, _ISignature \u0003, _ISignature \u0004, IScope5 \u0005)
		{
			IVariable[] outputs = \u0004.Outputs;
			IVariable[] outputs2 = \u0003.Outputs;
			if (outputs.Length != outputs2.Length)
			{
				\u0004.AddMessage(Severity.Error, MessageId.Err_InterfaceChangedBase, new object[]
				{
					\u0004.Name,
					\u0002.Name
				});
				return;
			}
			for (int i = 0; i < outputs.Length; i++)
			{
				if (!(outputs[i] as _IVariable).IsEqual(outputs2[i], false, false))
				{
					\u0004.AddMessage(Severity.Error, MessageId.Err_InterfaceChangedBase, new object[]
					{
						\u0004.Name,
						\u0002.Name
					});
					return;
				}
			}
			IVariable[] allInputs = \u0004.AllInputs;
			IVariable[] allInputs2 = \u0003.AllInputs;
			bool flag = \u0004.Name == IdentifierConstants.InitMethodName && allInputs.Length >= allInputs2.Length;
			if (allInputs.Length != allInputs2.Length && !flag)
			{
				\u0004.AddMessage(Severity.Error, MessageId.Err_InterfaceChangedBase, new object[]
				{
					\u0004.Name,
					\u0002.Name
				});
				return;
			}
			\u007F.\u0011.\u0001(\u0002, \u0005, \u0004, allInputs, allInputs2, flag);
		}

		// Token: 0x06003014 RID: 12308 RVA: 0x000B6278 File Offset: 0x000B4478
		private void \u0001(_ISignature \u0002, _ISignature \u0003, IScope5 \u0004)
		{
			foreach (_ISignature isignature in \u0003.SubSignatures.OfType<_ISignature>())
			{
				_ISignature isignature2 = \u0002.GetSubSignature(isignature.Name) as _ISignature;
				if (isignature2 != null && !this.\u0001(\u0002, \u0003, isignature, isignature2) && !this.\u0002(\u0002, \u0003, isignature, isignature2))
				{
					this.\u0001(\u0002, \u0003, isignature, isignature2);
					if (!this.\u0003(\u0002, \u0003, isignature, isignature2))
					{
						this.\u0001(\u0003, isignature, isignature2, \u0004);
					}
				}
			}
		}

		// Token: 0x06003015 RID: 12309 RVA: 0x000B6310 File Offset: 0x000B4510
		private static void \u0001(_ISignature \u0002, IScope5 \u0003, _ISignature \u0004, IVariable[] \u0005, IVariable[] \u0006, bool \u0007)
		{
			for (int i = 0; i < \u0005.Length; i++)
			{
				if (!(\u0005[i].OrgName == IdentifierConstants.InstancePointer) && (!\u0007 || i < \u0006.Length - 1))
				{
					_IVariable ivariable = (_IVariable)\u0005[i];
					if (!ivariable.IsEqual(\u0006[i], false, false, true, \u0003))
					{
						\u0004.AddMessage(Severity.Error, MessageId.Err_InterfaceChangedBase, new object[]
						{
							\u0004.Name,
							\u0002.Name
						});
						_IVariable ivariable2 = \u0006[i] as _IVariable;
						if (!global::\u0006.\u0011.\u0001(ivariable._Type.EffectiveType, ivariable2._Type.EffectiveType, \u0003 as ICommonScope))
						{
							\u007F.\u0011.\u0001(\u0003, \u0004, ivariable, ivariable2);
							return;
						}
						break;
					}
				}
			}
		}

		// Token: 0x06003016 RID: 12310 RVA: 0x000B63C4 File Offset: 0x000B45C4
		private static void \u0001(IScope5 \u0002, _ISignature \u0003, _IVariable \u0004, _IVariable \u0005)
		{
			string text = global::\u000E.\u000F.\u0001(\u0002 as ICommonScope, \u0004._Type.EffectiveType);
			string text2 = global::\u000E.\u000F.\u0001(\u0002 as ICommonScope, \u0005._Type.EffectiveType);
			\u0003.AddMessage(Severity.Error, MessageId.Err_TypeMismatch, new object[]
			{
				text,
				text2
			});
			if (\u0004._Type.EffectiveType.DeRefType.Class == TypeClass.Userdef && \u0005._Type.EffectiveType.DeRefType.Class == TypeClass.Userdef)
			{
				ISignature signature = \u0002[(\u0004._Type.EffectiveType.DeRefType as _IUserdefType).SignatureId];
				ISignature signature2 = \u0002[(\u0005._Type.EffectiveType.DeRefType as _IUserdefType).SignatureId];
				if (signature != null)
				{
					_ISourcePosition sourcepos = global::\u0019.\u0003.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(signature.LibraryPath), signature.ObjectGuid, 0L, 0, (short)signature.OrgName.Length);
					\u0003.AddMessage(sourcepos, Severity.Information, MessageId.Inf_RelatedPosition, Array.Empty<object>());
				}
				if (signature2 != null)
				{
					_ISourcePosition sourcepos2 = global::\u0019.\u0003.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(signature2.LibraryPath), signature2.ObjectGuid, 0L, 0, (short)signature2.OrgName.Length);
					\u0003.AddMessage(sourcepos2, Severity.Information, MessageId.Inf_RelatedPosition, Array.Empty<object>());
				}
			}
		}

		// Token: 0x06003017 RID: 12311 RVA: 0x000B6528 File Offset: 0x000B4728
		private bool \u0001(_ISignature \u0002, int \u0003, _ISignature \u0004, string \u0005)
		{
			string text = \u0005 + " -> " + \u0004.Name;
			if (\u0003 == \u0002.Id)
			{
				\u0002.AddMessage(Severity.Error, MessageId.Err_SelfInheritance, new object[]
				{
					text
				});
				return true;
			}
			return false;
		}

		// Token: 0x06003018 RID: 12312 RVA: 0x000B6568 File Offset: 0x000B4768
		private bool \u0001(_ISignature \u0002, _ISignature \u0003)
		{
			if (\u0003.GetFlag(SignatureFlag.Internal) && !string.Equals(\u0002.LibraryPath, \u0003.LibraryPath, StringComparison.OrdinalIgnoreCase))
			{
				\u0002.AddMessage(Severity.Error, MessageId.Err_AccessToInternalObject, new object[]
				{
					\u0003.OrgName,
					\u0003.LibraryPath
				});
				return true;
			}
			return false;
		}

		// Token: 0x06003019 RID: 12313 RVA: 0x000B65C4 File Offset: 0x000B47C4
		private void \u0001(_ISignature \u0002, int[] \u0003, IScope \u0004, string \u0005, LList<int> \u0006)
		{
			foreach (int num in \u0003)
			{
				_ISignature isignature = \u0004[num] as _ISignature;
				if (isignature != null)
				{
					if (this.\u0001(\u0002, num, isignature, \u0005) || this.\u0001(\u0002, isignature))
					{
						break;
					}
					if (!\u007F.\u0011.\u0001(\u0006, num))
					{
						\u0006.Add(num);
						this.\u0001(\u0002, isignature, \u0004);
						ISignature[] subSignatures = isignature.SubSignatures;
						this.\u0001(\u0002, \u0004, isignature, subSignatures);
						this.\u0001(\u0002, \u0004, \u0005, \u0006, isignature);
					}
				}
			}
		}

		// Token: 0x0600301A RID: 12314 RVA: 0x000B6648 File Offset: 0x000B4848
		private static bool \u0001(LList<int> \u0002, int \u0003)
		{
			using (IEnumerator<int> enumerator = \u0002.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current == \u0003)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x0600301B RID: 12315 RVA: 0x000B6694 File Offset: 0x000B4894
		private void \u0001(_ISignature \u0002, IScope \u0003, string \u0004, LList<int> \u0005, _ISignature \u0006)
		{
			if (\u0006.BaseSignatureId != Helper.InvalidId)
			{
				int[] array = new int[1 + \u0006.InterfaceIds.Count<int>()];
				array[0] = \u0006.BaseSignatureId;
				\u0006.InterfaceIds.CopyTo(array, 1);
				string u = \u0004 + " -> " + \u0006.Name;
				this.\u0001(\u0002, array, \u0003, u, \u0005);
			}
		}

		// Token: 0x0600301C RID: 12316 RVA: 0x000B66FC File Offset: 0x000B48FC
		private void \u0001(_ISignature \u0002, IScope \u0003, _ISignature \u0004, ISignature[] \u0005)
		{
			foreach (ISignature signature in \u0005)
			{
				if (signature.POUType == Operator.Method)
				{
					ISignature subSignature = \u0002.GetSubSignature(signature.Name);
					_ISignature isignature = null;
					if (subSignature == null)
					{
						\u007F.\u0011.\u0001(\u0002, \u0003, signature, ref subSignature, ref isignature);
					}
					else
					{
						\u007F.\u0011.\u0001(\u0002, \u0004, signature, subSignature);
					}
					if (subSignature == null)
					{
						\u0002.AddMessage(Severity.Error, MessageId.Err_NoMethodImplementation, new object[]
						{
							signature.Name,
							\u0004.Name
						});
					}
					else
					{
						if (isignature == null)
						{
							isignature = (subSignature as _ISignature);
						}
						if (subSignature.POUType != Operator.Method)
						{
							\u0002.AddMessage(Severity.Error, MessageId.Err_SomethingOverridingMethod, new object[]
							{
								Scanner.GetTextOfOperator(subSignature.POUType),
								subSignature.Name,
								\u0004.Name
							});
						}
						else
						{
							this.\u0001(signature, subSignature, \u0004, isignature, \u0003);
						}
					}
				}
			}
		}

		// Token: 0x0600301D RID: 12317 RVA: 0x000B67D4 File Offset: 0x000B49D4
		private static void \u0001(_ISignature \u0002, _ISignature \u0003, ISignature \u0004, ISignature \u0005)
		{
			bool flag = \u0005.GetFlag(SignatureFlag.Internal) && \u0003.GetFlag(SignatureFlag.Internal) && \u0004.GetFlag(SignatureFlag.Internal);
			if ((\u0005.GetFlag(SignatureFlag.Private) || \u0005.GetFlag(SignatureFlag.Protected) || \u0005.GetFlag(SignatureFlag.Internal)) && !flag)
			{
				\u0002.AddMessage(Severity.Error, MessageId.Err_InterfaceMethodImplementationNotPublic, new object[]
				{
					\u0005.OrgName,
					\u0002.OrgName
				});
			}
		}

		// Token: 0x0600301E RID: 12318 RVA: 0x000B6870 File Offset: 0x000B4A70
		private static void \u0001(_ISignature \u0002, IScope \u0003, ISignature \u0004, ref ISignature \u0005, ref _ISignature \u0006)
		{
			ISignature signature;
			for (int baseSignatureId = \u0002.BaseSignatureId; baseSignatureId != Helper.InvalidId; baseSignatureId = signature.BaseSignatureId)
			{
				signature = \u0003[baseSignatureId];
				\u0005 = signature.GetSubSignature(\u0004.Name);
				if (\u0005 != null)
				{
					\u0006 = \u0002;
					return;
				}
			}
		}

		// Token: 0x0600301F RID: 12319 RVA: 0x000B68B4 File Offset: 0x000B4AB4
		private static _IVariable \u0001(_ISignature \u0002, IScope \u0003, _IVariable \u0004)
		{
			ISignature signature;
			for (int baseSignatureId = \u0002.BaseSignatureId; baseSignatureId != Helper.InvalidId; baseSignatureId = signature.BaseSignatureId)
			{
				signature = \u0003[baseSignatureId];
				IVariable variable = signature[\u0004.VersionedName];
				if (variable != null)
				{
					return variable as _IVariable;
				}
			}
			return null;
		}

		// Token: 0x06003020 RID: 12320 RVA: 0x000B68FC File Offset: 0x000B4AFC
		private void \u0001(_ISignature \u0002, _ISignature \u0003, IScope \u0004)
		{
			foreach (_IVariable ivariable in \u0003.AllVariables)
			{
				_IVariable ivariable2 = \u0002[ivariable.VersionedName] as _IVariable;
				if (ivariable2 == null)
				{
					ivariable2 = \u007F.\u0011.\u0001(\u0002, \u0004, ivariable);
				}
				if (ivariable2 != null && (!ivariable2.IsProperty || !ivariable.IsProperty))
				{
					\u0002.AddMessage(Severity.Error, MessageId.Err_VariableOverride, new object[]
					{
						ivariable.VersionedName,
						\u0002.OrgName,
						\u0003.OrgName
					});
				}
				this.\u0001(\u0002, \u0003, ivariable, ivariable2);
			}
		}

		// Token: 0x06003021 RID: 12321 RVA: 0x000B69AC File Offset: 0x000B4BAC
		private void \u0001(_ISignature \u0002, _ISignature \u0003, _IVariable \u0004, _IVariable \u0005)
		{
			if (\u0005 != null && \u0005.IsProperty && \u0004.IsProperty)
			{
				bool flag = TypeTable.IsLType(\u0005.Type.Class);
				string attributeValue = \u0005.GetAttributeValue(CompileAttributes.ATTRIBUTE_MONITORING);
				string attributeValue2 = \u0004.GetAttributeValue(CompileAttributes.ATTRIBUTE_MONITORING);
				if (attributeValue != attributeValue2 && (attributeValue == CompileAttributes.ATTRIBUTEVALUE_CALL || attributeValue2 == CompileAttributes.ATTRIBUTEVALUE_CALL))
				{
					if (this.ComconNew.DataManager.PackMode != 8 && flag)
					{
						\u0002.AddMessage(Severity.Error, MessageId.Err_InterfaceChangedBase, new object[]
						{
							\u0005.OrgName,
							\u0003.OrgName
						});
						return;
					}
					\u0002.AddMessage(Severity.Warning, MessageId.Wrn_InterfaceChangedBase, new object[]
					{
						\u0005.OrgName,
						\u0003.OrgName
					});
				}
			}
		}

		// Token: 0x06003022 RID: 12322 RVA: 0x000B6A88 File Offset: 0x000B4C88
		private void \u0001(ISignature \u0002, ISignature \u0003, _ISignature \u0004, _ISignature \u0005, IScope \u0006)
		{
			IVariable[] allOutputs = \u0003.AllOutputs;
			IVariable[] allOutputs2 = \u0002.AllOutputs;
			if (allOutputs.Length != allOutputs2.Length)
			{
				\u0005.AddMessage(Severity.Error, MessageId.Err_InterfaceChanged, new object[]
				{
					\u0003.Name,
					\u0004.Name
				});
				\u0005.AddMessage(Severity.Error, MessageId.Err_InterfaceChanged_NumberOfInputsOutputsDifferent, new object[]
				{
					\u0003.Name,
					\u0004.Name
				});
				_ISourcePosition sourcepos = global::\u0019.\u0003.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0002.LibraryPath), \u0002.ObjectGuid, 0L, 0, 0);
				\u0005.AddMessageString(sourcepos, Severity.Information, \u0081.\u0002.Err_RelatedPositionInterface, new object[]
				{
					\u0002.Name,
					\u0004.Name
				});
				return;
			}
			for (int i = 0; i < allOutputs.Length; i++)
			{
				if (!(allOutputs[i] as _IVariable).IsEqual(allOutputs2[i], false, false, true, \u0006))
				{
					\u0005.AddMessage(Severity.Error, MessageId.Err_InterfaceChanged, new object[]
					{
						\u0003.Name,
						\u0004.Name
					});
					\u0005.\u0001(allOutputs[i].SourcePosition, Severity.Error, MessageId.Err_InterfaceChanged_VariableDifferent, new object[]
					{
						allOutputs[i].OrgName,
						\u0003.Name,
						\u0004.Name
					});
					_ISourcePosition sourcepos2 = global::\u0019.\u0003.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0002.LibraryPath), \u0002.ObjectGuid, allOutputs2[i].SourcePosition.Position, allOutputs2[i].SourcePosition.PositionOffset, allOutputs2[i].SourcePosition.Length);
					\u0005.AddMessageString(sourcepos2, Severity.Information, \u0081.\u0002.Err_RelatedPositionInterface, new object[]
					{
						\u0002.Name,
						\u0004.Name
					});
					break;
				}
			}
			IVariable[] allInputs = \u0003.AllInputs;
			IVariable[] allInputs2 = \u0002.AllInputs;
			if (\u0003.AllInputs.Length != \u0002.AllInputs.Length)
			{
				\u0005.AddMessage(Severity.Error, MessageId.Err_InterfaceChanged, new object[]
				{
					\u0003.Name,
					\u0004.Name
				});
				\u0005.AddMessage(Severity.Error, MessageId.Err_InterfaceChanged_NumberOfInputsOutputsDifferent, new object[]
				{
					\u0003.Name,
					\u0004.Name
				});
				_ISourcePosition sourcepos3 = global::\u0019.\u0003.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0002.LibraryPath), \u0002.ObjectGuid, 0L, 0, 0);
				\u0005.AddMessageString(sourcepos3, Severity.Information, \u0081.\u0002.Err_RelatedPositionInterface, new object[]
				{
					\u0002.Name,
					\u0004.Name
				});
				return;
			}
			for (int j = 0; j < allInputs.Length; j++)
			{
				if (!(allInputs[j].OrgName == IdentifierConstants.InstancePointer) && !(allInputs[j] as _IVariable).IsEqual(allInputs2[j], false, false, true, \u0006))
				{
					\u0005.AddMessage(Severity.Error, MessageId.Err_InterfaceChanged, new object[]
					{
						\u0003.Name,
						\u0004.Name
					});
					\u0005.\u0001(allInputs[j].SourcePosition, Severity.Error, MessageId.Err_InterfaceChanged_VariableDifferent, new object[]
					{
						allInputs[j].OrgName,
						\u0003.Name,
						\u0004.Name
					});
					_ISourcePosition sourcepos4 = global::\u0019.\u0003.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0002.LibraryPath), \u0002.ObjectGuid, allInputs2[j].SourcePosition.Position, allInputs2[j].SourcePosition.PositionOffset, allInputs2[j].SourcePosition.Length);
					\u0005.AddMessageString(sourcepos4, Severity.Information, \u0081.\u0002.Err_RelatedPositionInterface, new object[]
					{
						\u0002.Name,
						\u0004.Name
					});
					return;
				}
			}
		}

		// Token: 0x06003023 RID: 12323 RVA: 0x000B6E30 File Offset: 0x000B5030
		private bool \u0002(_ISignature \u0002)
		{
			if (string.IsNullOrEmpty(\u0002.LibraryPath))
			{
				return false;
			}
			_IPreCompileContext contextByLibraryPath = this.ComconNew.GetContextByLibraryPath(\u0002.LibraryPath);
			return contextByLibraryPath != null && this.ComconNew.IsDefined(contextByLibraryPath.UnitTestingDefine);
		}

		// Token: 0x04000930 RID: 2352
		[CompilerGenerated]
		private readonly \u001B \u0001;
	}
}

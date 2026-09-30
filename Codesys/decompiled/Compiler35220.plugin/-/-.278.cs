using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u0006;
using \u000E;
using \u0017;
using \u0019;
using \u001F;
using _3S.CoDeSys.Compiler35220.Compile.Phase1_Typification.Code;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.PreCompile.Typification;
using _3S.CoDeSys.Compiler35220.PreCompile.Typification.MessageSuppression;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace \u0014
{
	// Token: 0x020002EA RID: 746
	internal sealed class \u0012 : \u001F.\u0007
	{
		// Token: 0x170007B0 RID: 1968
		// (get) Token: 0x06002D5C RID: 11612 RVA: 0x000A6240 File Offset: 0x000A4440
		// (set) Token: 0x06002D5D RID: 11613 RVA: 0x000A6248 File Offset: 0x000A4448
		public IMessageSuppressionController MessageSuppressionController { get; set; } = NoSuppressions.Instance;

		// Token: 0x170007B1 RID: 1969
		// (get) Token: 0x06002D5E RID: 11614 RVA: 0x000A6254 File Offset: 0x000A4454
		// (set) Token: 0x06002D5F RID: 11615 RVA: 0x000A625C File Offset: 0x000A445C
		public Guid MessageGuid { get; set; } = Guid.Empty;

		// Token: 0x170007B2 RID: 1970
		// (get) Token: 0x06002D60 RID: 11616 RVA: 0x000A6268 File Offset: 0x000A4468
		// (set) Token: 0x06002D61 RID: 11617 RVA: 0x000A6270 File Offset: 0x000A4470
		public bool ConvertAllTypeMismatches { get; set; }

		// Token: 0x170007B3 RID: 1971
		// (get) Token: 0x06002D62 RID: 11618 RVA: 0x000A627C File Offset: 0x000A447C
		// (set) Token: 0x06002D63 RID: 11619 RVA: 0x000A6284 File Offset: 0x000A4484
		public bool InImplicitCode { get; set; }

		// Token: 0x170007B4 RID: 1972
		// (get) Token: 0x06002D64 RID: 11620 RVA: 0x000A6290 File Offset: 0x000A4490
		// (set) Token: 0x06002D65 RID: 11621 RVA: 0x000A6298 File Offset: 0x000A4498
		public bool ExplicitConversion { get; set; }

		// Token: 0x170007B5 RID: 1973
		// (get) Token: 0x06002D66 RID: 11622 RVA: 0x000A62A4 File Offset: 0x000A44A4
		// (set) Token: 0x06002D67 RID: 11623 RVA: 0x000A62AC File Offset: 0x000A44AC
		public bool AddCrossReferences { get; set; }

		// Token: 0x170007B6 RID: 1974
		// (get) Token: 0x06002D68 RID: 11624 RVA: 0x000A62B8 File Offset: 0x000A44B8
		// (set) Token: 0x06002D69 RID: 11625 RVA: 0x000A62C0 File Offset: 0x000A44C0
		public bool WriteConstants { get; private set; }

		// Token: 0x170007B7 RID: 1975
		// (get) Token: 0x06002D6A RID: 11626 RVA: 0x000A62CC File Offset: 0x000A44CC
		// (set) Token: 0x06002D6B RID: 11627 RVA: 0x000A62D4 File Offset: 0x000A44D4
		private _ICompileContext Comcon { get; set; }

		// Token: 0x170007B8 RID: 1976
		// (get) Token: 0x06002D6C RID: 11628 RVA: 0x000A62E0 File Offset: 0x000A44E0
		// (set) Token: 0x06002D6D RID: 11629 RVA: 0x000A62E8 File Offset: 0x000A44E8
		private _IScope2 Scope { get; set; }

		// Token: 0x170007B9 RID: 1977
		// (get) Token: 0x06002D6E RID: 11630 RVA: 0x000A62F4 File Offset: 0x000A44F4
		// (set) Token: 0x06002D6F RID: 11631 RVA: 0x000A62FC File Offset: 0x000A44FC
		private _ICompiledPOU Compiledpou { get; set; }

		// Token: 0x170007BA RID: 1978
		// (get) Token: 0x06002D70 RID: 11632 RVA: 0x000A6308 File Offset: 0x000A4508
		// (set) Token: 0x06002D71 RID: 11633 RVA: 0x000A6310 File Offset: 0x000A4510
		private _ISignature SignToCheck { get; set; }

		// Token: 0x170007BB RID: 1979
		// (get) Token: 0x06002D72 RID: 11634 RVA: 0x000A631C File Offset: 0x000A451C
		private \u0081.\u0015 LiteralValueConverter { get; }

		// Token: 0x06002D73 RID: 11635 RVA: 0x000A6324 File Offset: 0x000A4524
		internal \u0012(_IScope2 \u009B\u0002, _ICompileContext \u0001\u0002, _ICompiledPOU \u0012\u0002)
		{
			this.Scope = \u009B\u0002;
			this.Comcon = \u0001\u0002;
			this.Compiledpou = \u0012\u0002;
			this.SignToCheck = (\u009B\u0002.LocalSignature as _ISignature);
			if (\u009B\u0002.MethodSignature != null)
			{
				this.SignToCheck = (this.Scope.MethodSignature as _ISignature);
			}
			this.LiteralValueConverter = new \u0081.\u0015(this);
		}

		// Token: 0x06002D74 RID: 11636 RVA: 0x000A63A0 File Offset: 0x000A45A0
		public void \u0001(_IExprement \u0002, MessageId \u0003, params object[] \u0004)
		{
			if (this.MessageSuppressionController.HandleMessage(\u0003, Severity.Error) != MessageHandling.Report)
			{
				return;
			}
			string format = global::\u000E.\u0018.\u0001(\u0003);
			if (this.MessageGuid != Guid.Empty)
			{
				\u0002.AddError(string.Format(format, \u0004), \u0003, this.MessageGuid);
				return;
			}
			\u0002.AddError(string.Format(format, \u0004), \u0003);
		}

		// Token: 0x06002D75 RID: 11637 RVA: 0x000A63FC File Offset: 0x000A45FC
		public bool \u0001(_IExprement \u0002, ICompiledType \u0003, TypeClass \u0004, IScope5 \u0005, ref _IExpression \u0006)
		{
			return this.\u0001(\u0002, \u0003, TypeTable.Get(\u0004), \u0005, ref \u0006);
		}

		// Token: 0x06002D76 RID: 11638 RVA: 0x000A6410 File Offset: 0x000A4610
		public bool \u0001(_IExprement \u0002, ICompiledType \u0003, ICompiledType \u0004, IScope5 \u0005, ref _IExpression \u0006)
		{
			bool flag;
			return this.\u0001(\u0002, \u0003, \u0004, \u0005, ref \u0006, out flag);
		}

		// Token: 0x06002D77 RID: 11639 RVA: 0x000A642C File Offset: 0x000A462C
		public static bool \u0001(\u001F.\u0007 \u0002, _IExpression \u0003, ICompiledType \u0004, ICompiledType \u0005, _ICompileContext \u0006, IScope5 \u0007)
		{
			bool flag;
			return \u0002.\u0001(\u0003, \u0004, \u0005, \u0007, ref \u0003, out flag);
		}

		// Token: 0x06002D78 RID: 11640 RVA: 0x000A6448 File Offset: 0x000A4648
		public bool \u0001(ICompiledType \u0002, ICompiledType \u0003, IScope5 \u0004, _IExpression \u0005)
		{
			ICompiledType compiledType = (\u0003.Class == TypeClass.Reference) ? \u0003.BaseType : \u0003;
			if (compiledType.Class == TypeClass.Enum)
			{
				_ISignature isignature = \u0004.FindSignature(compiledType as IEnumType) as _ISignature;
				if (!global::\u000E.\u000F.\u0001(\u0004 as ICommonScope, \u0002, compiledType) && isignature != null && !global::\u000E.\u000F.\u0001(\u0005, isignature, \u0004 as ICommonScope))
				{
					this.\u0001(\u0005, MessageId.Err_StrictEnumNotAMember, new object[]
					{
						\u0005.ToString(),
						isignature.OrgName
					});
					return false;
				}
				this.\u0001(\u0005, \u0004);
			}
			return true;
		}

		// Token: 0x06002D79 RID: 11641 RVA: 0x000A64DC File Offset: 0x000A46DC
		public bool \u0001(_IExprement \u0002, ICompiledType \u0003, ICompiledType \u0004, IScope5 \u0005, ref _IExpression \u0006, out bool \u0007)
		{
			\u0007 = false;
			if (\u0003 == null || \u0004 == null)
			{
				return false;
			}
			if (!this.\u0001(\u0003, \u0004, \u0005, \u0006))
			{
				return false;
			}
			ILiteralValue literalValue = \u0006.Literal(\u0005);
			int num = -1;
			if (literalValue != null && (global::\u0006.\u0011.\u0001(\u0004, \u0005) || global::\u0006.\u0011.\u0001(\u0004.DeRefType, \u0005)) && literalValue.GetInt(out num) && num == 0)
			{
				return true;
			}
			bool u = false;
			if (literalValue != null)
			{
				if (!this.\u0001(\u0002, \u0003, \u0004, \u0005, ref \u0006, ref \u0007, literalValue))
				{
					return false;
				}
			}
			else
			{
				if (!global::\u0006.\u0011.\u0002(\u0003, \u0004, \u0005))
				{
					this.\u0001(\u0002, \u0003, \u0004, \u0005, ref \u0006, ref \u0007);
					return false;
				}
				if (this.Comcon.ApplicationGuid == Guid.Empty && global::\u0006.\u0011.\u0003(\u0003, \u0004, \u0005) && !this.ExplicitConversion)
				{
					u = this.\u0001(\u0002, \u0003, \u0004);
				}
			}
			this.\u0002(\u0002, \u0003, \u0004, \u0005);
			TypeClass typeClass = \u0003.DeRefType.Class;
			if (\u0003.DeRefType.Class == TypeClass.Pointer)
			{
				if (TypeTable.GetSize(TypeClass.Pointer, \u0005) == 8)
				{
					typeClass = TypeClass.LWord;
				}
				else
				{
					typeClass = TypeClass.DWord;
				}
			}
			if (global::\u0014.\u0012.\u0001(\u0003, \u0004, \u0005, typeClass))
			{
				if (this.\u0001(\u0002, \u0004, \u0005, ref \u0006))
				{
					return true;
				}
				bool u2 = TypeTable.IsAnyType(\u0004.Class);
				this.\u0001(\u0002, \u0003, \u0004, \u0005, u, u2);
				this.\u0001(\u0002, \u0003, \u0004, \u0005);
				this.\u0001(\u0004, \u0005, ref \u0006, ref \u0007, typeClass, u2);
			}
			return true;
		}

		// Token: 0x06002D7A RID: 11642 RVA: 0x000A6634 File Offset: 0x000A4834
		private void \u0001(ICompiledType \u0002, IScope5 \u0003, ref _IExpression \u0004, ref bool \u0005, TypeClass \u0006, bool \u0007)
		{
			if (!global::\u0006.\u0011.\u0001(\u0006, \u0002.DeRefType.Class, this.Comcon.TreatLRealAsReal, this.Comcon.TreatInt64AsInt32, TypeTable.GetSize(TypeClass.Pointer, \u0003) == 8) && !\u0007)
			{
				TypeClass u = \u0002.DeRefType.Class;
				if (\u0002.DeRefType.Class == TypeClass.Pointer)
				{
					if (TypeTable.GetSize(TypeClass.Pointer, \u0003) == 8)
					{
						u = TypeClass.LWord;
					}
					else
					{
						u = TypeClass.DWord;
					}
				}
				_IImplicitConversionExpression iimplicitConversionExpression = global::\u0019.\u0003.\u0001(\u0006, u, Token.Empty);
				iimplicitConversionExpression._Exp = \u0004;
				\u0004 = iimplicitConversionExpression;
				\u0004.Type = \u0002.DeRefType;
				if (this.Comcon != null && this.Comcon.Codegenerator != null)
				{
					string empty = string.Empty;
					TypeClass typeClass = TypeClass.None;
					if (ImplicitFunctionCallsHandler.\u0001(this.Comcon, iimplicitConversionExpression, this.Scope, ref empty, ref typeClass))
					{
						\u0005 = true;
						IList<ISignature> list = this.Scope[empty];
						if (this.Comcon.ApplicationGuid != Guid.Empty)
						{
							Debug.\u0001(list != null && list.Count == 1);
							ISignature signature = list[0];
							if (this.Compiledpou != null && this.Compiledpou.SignatureId != signature.Id)
							{
								this.\u0002((_ISignature)signature);
							}
							if (\u0003.MethodSignature != null)
							{
								this.\u0001((_ISignature)\u0003.MethodSignature, signature.Id);
								return;
							}
							if (\u0003.LocalSignature != null && \u0003.LocalSignature.Id != signature.Id)
							{
								this.\u0001((_ISignature)\u0003.LocalSignature, signature.Id);
							}
						}
					}
				}
			}
		}

		// Token: 0x06002D7B RID: 11643 RVA: 0x000A67DC File Offset: 0x000A49DC
		private void \u0001(_IExprement \u0002, ICompiledType \u0003, ICompiledType \u0004, IScope5 \u0005)
		{
			if (TypeTable.IsInteger(\u0003.Class) && TypeTable.IsReal(\u0004.Class) && TypeTable.GetSize(\u0003.Class, \u0005) >= TypeTable.GetSize(\u0004.Class, \u0005) && !this.Comcon.IsDefined("NO_3_0_CONVERSION_CHECKS") && !this.InImplicitCode)
			{
				this.\u0002(\u0002, MessageId.Wrn_ImplicitIntToReal, new object[]
				{
					\u0003,
					\u0004
				});
			}
			if (\u0003.Class == TypeClass.LReal && \u0004.Class == TypeClass.Real && !this.Comcon.TreatLRealAsReal && !this.Comcon.IsDefined("NO_3_0_CONVERSION_CHECKS") && !this.InImplicitCode)
			{
				this.\u0002(\u0002, MessageId.Wrn_ImplicitIntToReal, new object[]
				{
					\u0003,
					\u0004
				});
			}
		}

		// Token: 0x06002D7C RID: 11644 RVA: 0x000A68A8 File Offset: 0x000A4AA8
		private void \u0001(_IExprement \u0002, ICompiledType \u0003, ICompiledType \u0004, IScope5 \u0005, bool \u0006, bool \u0007)
		{
			TypeClass tc = \u0003.DeRefType.Class;
			TypeClass @class = \u0004.DeRefType.Class;
			if (\u0003.Class == TypeClass.Enum && TypeTable.IsSigned(tc) != TypeTable.IsSigned(@class) && TypeTable.GetSize(tc, \u0005) <= TypeTable.GetSize(@class, \u0005))
			{
				if ((\u0003 as _IEnumType).GetSignature(\u0005).HasAttribute("nounsignedcheck"))
				{
					tc = @class;
				}
			}
			else if (\u0004.Class == TypeClass.Enum && TypeTable.IsSigned(tc) != TypeTable.IsSigned(@class) && TypeTable.GetSize(tc, \u0005) <= TypeTable.GetSize(@class, \u0005) && (\u0004 as _IEnumType).GetSignature(\u0005).HasAttribute("nounsignedcheck"))
			{
				tc = @class;
			}
			if (TypeTable.IsSigned(tc) && !TypeTable.IsSigned(@class) && !\u0007)
			{
				if (!\u0006 && !this.Comcon.IsDefined("NO_3_0_CONVERSION_CHECKS") && !this.InImplicitCode)
				{
					this.\u0002(\u0002, MessageId.Wrn_ImplicitSignedToUnsigned, new object[]
					{
						\u0003,
						\u0004
					});
					return;
				}
			}
			else if (!TypeTable.IsSigned(tc) && TypeTable.IsSigned(@class) && !TypeTable.IsReal(@class) && TypeTable.GetSize(tc, \u0005) == TypeTable.GetSize(@class, \u0005))
			{
				bool flag = \u0002 is IVariableExpression && (\u0002 as IVariableExpression).Name == "LOG_STD_LOGGER" && \u0003.Class == TypeClass.Pointer;
				if (!\u0006 && !this.Comcon.IsDefined("NO_3_0_CONVERSION_CHECKS") && !this.InImplicitCode && !flag)
				{
					this.\u0002(\u0002, MessageId.Wrn_ImplicitUnsignedToSigned, new object[]
					{
						\u0003,
						\u0004
					});
				}
			}
		}

		// Token: 0x06002D7D RID: 11645 RVA: 0x000A6A48 File Offset: 0x000A4C48
		private bool \u0001(_IExprement \u0002, ICompiledType \u0003, IScope5 \u0004, ref _IExpression \u0005)
		{
			_ILiteralExpression iliteralExpression = \u0005 as _ILiteralExpression;
			if (iliteralExpression != null && this.LiteralValueConverter.\u0001(\u0002, \u0003, \u0004, ref iliteralExpression))
			{
				\u0005 = iliteralExpression;
				return true;
			}
			return false;
		}

		// Token: 0x06002D7E RID: 11646 RVA: 0x000A6A7C File Offset: 0x000A4C7C
		private static bool \u0001(ICompiledType \u0002, ICompiledType \u0003, IScope5 \u0004, TypeClass \u0005)
		{
			return \u0005 != TypeClass.Array && \u0005 != TypeClass.String && \u0005 != TypeClass.Pointer && \u0005 != TypeClass.Userdef && !global::\u0006.\u0011.\u0001(\u0002.DeRefType, \u0003.DeRefType, \u0004) && \u0002.DeRefType.Class != \u0003.DeRefType.Class;
		}

		// Token: 0x06002D7F RID: 11647 RVA: 0x000A6AD0 File Offset: 0x000A4CD0
		private void \u0002(_IExprement \u0002, ICompiledType \u0003, ICompiledType \u0004, IScope5 \u0005)
		{
			if (\u0003.Class == \u0004.Class && \u0004.Class == TypeClass.Enum)
			{
				_IEnumType ienumType = (\u0003.Class == TypeClass.Enum) ? (\u0003 as _IEnumType) : (\u0003.DeRefType as _IEnumType);
				_IEnumType ienumType2 = (\u0004.Class == TypeClass.Enum) ? (\u0004 as _IEnumType) : (\u0004.DeRefType as _IEnumType);
				if (ienumType != null && ienumType2 != null && ienumType.SignatureId != ienumType2.SignatureId)
				{
					_ISignature isignature = \u0005[ienumType.SignatureId] as _ISignature;
					_ISignature isignature2 = \u0005[ienumType2.SignatureId] as _ISignature;
					string text = ienumType.ToString();
					string text2 = ienumType2.ToString();
					if (isignature != null && isignature.IsLibraryObject)
					{
						text = text + " (" + isignature.LibraryPath + ")";
					}
					if (isignature2 != null && isignature2.IsLibraryObject)
					{
						text2 = text2 + " (" + isignature2.LibraryPath + ")";
					}
					this.\u0002(\u0002, MessageId.Wrn_ImplicitEnumConversion, new object[]
					{
						text,
						text2
					});
				}
			}
		}

		// Token: 0x06002D80 RID: 11648 RVA: 0x000A6BF4 File Offset: 0x000A4DF4
		private bool \u0001(_IExprement \u0002, ICompiledType \u0003, ICompiledType \u0004)
		{
			bool flag = true;
			if (TypeTable.IsResolvedXType(\u0004))
			{
				flag = false;
			}
			if (TypeTable.IsResolvedXType(\u0003))
			{
				flag = false;
			}
			if (flag)
			{
				this.\u0001(\u0002, \u0003, \u0004);
			}
			return flag;
		}

		// Token: 0x06002D81 RID: 11649 RVA: 0x000A6C24 File Offset: 0x000A4E24
		private void \u0001(_IExprement \u0002, ICompiledType \u0003, ICompiledType \u0004, IScope5 \u0005, ref _IExpression \u0006, ref bool \u0007)
		{
			if (this.ConvertAllTypeMismatches)
			{
				_IImplicitConversionExpression iimplicitConversionExpression = global::\u0019.\u0003.\u0001(\u0003.DeRefType.Class, \u0004.DeRefType.Class, Token.Empty);
				iimplicitConversionExpression._Exp = \u0006;
				\u0006 = iimplicitConversionExpression;
				\u0006.Type = \u0004.DeRefType;
				if (this.Comcon != null && this.Comcon.Codegenerator != null)
				{
					string empty = string.Empty;
					TypeClass typeClass = TypeClass.None;
					if (ImplicitFunctionCallsHandler.\u0001(this.Comcon, iimplicitConversionExpression, this.Scope, ref empty, ref typeClass))
					{
						\u0007 = true;
						return;
					}
				}
			}
			else
			{
				this.\u0001(\u0005, \u0002, \u0003, \u0004);
			}
		}

		// Token: 0x06002D82 RID: 11650 RVA: 0x000A6CBC File Offset: 0x000A4EBC
		private bool \u0001(_IExprement \u0002, ICompiledType \u0003, ICompiledType \u0004, IScope5 \u0005, ref _IExpression \u0006, ref bool \u0007, ILiteralValue \u0008)
		{
			int u = \u0004.DeRefType.Size(\u0005);
			if (!this.\u0001(\u0006, \u0004, u) && !global::\u0006.\u0011.\u0001(\u0008, \u0003, \u0004, \u0005, \u0005))
			{
				if (this.ConvertAllTypeMismatches)
				{
					_IImplicitConversionExpression iimplicitConversionExpression = global::\u0019.\u0003.\u0001(\u0003.DeRefType.Class, \u0004.DeRefType.Class, Token.Empty);
					iimplicitConversionExpression._Exp = \u0006;
					\u0006 = iimplicitConversionExpression;
					\u0006.Type = \u0004.DeRefType;
					if (this.Comcon != null && this.Comcon.Codegenerator != null)
					{
						string empty = string.Empty;
						TypeClass typeClass = TypeClass.None;
						if (ImplicitFunctionCallsHandler.\u0001(this.Comcon, iimplicitConversionExpression, this.Scope, ref empty, ref typeClass))
						{
							\u0007 = true;
							IList<ISignature> list = this.Scope[empty];
							Debug.\u0001(list != null && list.Count == 1);
						}
					}
				}
				else if (\u0004.Class == TypeClass.Subrange)
				{
					this.\u0001(\u0006, MessageId.Err_TypeMismatch, new object[]
					{
						\u0006,
						\u0004
					});
				}
				else if (global::\u0006.\u0011.\u0002(\u0003, \u0004, \u0005))
				{
					if (TypeTable.IsSigned(\u0003.Class) && !TypeTable.IsSigned(\u0004.Class) && !TypeTable.IsAnyType(\u0004.Class))
					{
						if (!this.Comcon.IsDefined("NO_3_0_CONVERSION_CHECKS") && !this.InImplicitCode)
						{
							this.\u0002(\u0002, MessageId.Wrn_ImplicitSignedToUnsigned, new object[]
							{
								\u0003,
								\u0004
							});
						}
					}
					else if (!TypeTable.IsSigned(\u0003.Class) && TypeTable.IsSigned(\u0004.Class) && !TypeTable.IsReal(\u0004.Class) && TypeTable.GetSize(\u0003.Class, \u0005) == TypeTable.GetSize(\u0004.Class, \u0005) && !this.Comcon.IsDefined("NO_3_0_CONVERSION_CHECKS") && !this.InImplicitCode)
					{
						this.\u0002(\u0002, MessageId.Wrn_ImplicitUnsignedToSigned, new object[]
						{
							\u0003,
							\u0004
						});
					}
					_IImplicitConversionExpression iimplicitConversionExpression2;
					if (\u0004.DeRefType.Class == TypeClass.Pointer)
					{
						TypeClass u2 = TypeClass.DWord;
						if (TypeTable.GetSize(TypeClass.Pointer, \u0005) == 8)
						{
							u2 = TypeClass.LWord;
						}
						iimplicitConversionExpression2 = global::\u0019.\u0003.\u0001(\u0003.DeRefType.Class, u2, Token.Empty);
					}
					else
					{
						iimplicitConversionExpression2 = global::\u0019.\u0003.\u0001(\u0003.DeRefType.Class, \u0004.DeRefType.Class, Token.Empty);
					}
					if (this.\u0001(iimplicitConversionExpression2.From, iimplicitConversionExpression2.To))
					{
						iimplicitConversionExpression2._Exp = \u0006;
						\u0006 = iimplicitConversionExpression2;
						\u0006.Type = \u0004.DeRefType;
					}
				}
				else
				{
					this.\u0001(\u0005, \u0002, \u0003, \u0004);
				}
				return false;
			}
			return true;
		}

		// Token: 0x06002D83 RID: 11651 RVA: 0x000A6F60 File Offset: 0x000A5160
		private bool \u0001(TypeClass \u0002, TypeClass \u0003)
		{
			return TypeTable.GetSize(\u0002, this.Scope) <= TypeTable.GetSize(\u0003, this.Scope) && \u0002 != \u0003;
		}

		// Token: 0x06002D84 RID: 11652 RVA: 0x000A6F88 File Offset: 0x000A5188
		public void \u0001(_IExprement \u0002, ISignature \u0003, _IExpression \u0004, _IVariable \u0005)
		{
			if (\u0005.Type == null || \u0004._CompiledType == null)
			{
				return;
			}
			if (\u0005.Type.Class == TypeClass.Reference || \u0005.HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY))
			{
				bool bVarInoutConstant = \u0005.IsVarInoutConstant;
				if ((\u0005._Type.DeRefType.Class == TypeClass.String || \u0005._Type.DeRefType.Class == TypeClass.WString) && !\u0005.GetFlag(VarFlag.Inout))
				{
					if (\u0004.IsLiteral || \u0004.IsConstant(this.Scope, true))
					{
						bool flag = this.SignToCheck != null && this.SignToCheck.IsCompiledLibraryObject;
						if (!\u0003.GetFlag(SignatureFlag.External) && !flag)
						{
							this.\u0002(\u0004, MessageId.Wrn_LValueForVarinoutStrings, new object[]
							{
								\u0005.OrgName,
								\u0003.OrgName
							});
						}
					}
					bVarInoutConstant = true;
				}
				if (!\u0004.IsVarInOutInput(this.Scope, this.WriteConstants, bVarInoutConstant))
				{
					bool flag2 = false;
					int num;
					if (!\u0005.GetFlag(VarFlag.Inout) && \u0004 is ILiteralExpression && (\u0004 as ILiteralExpression).LiteralValue.GetInt(out num) && num == 0)
					{
						flag2 = true;
					}
					if (!flag2)
					{
						_IVariable ivariable = \u0004.GetVariable(this.Scope) as _IVariable;
						if (ivariable != null && ivariable.IsProperty)
						{
							this.\u0001(\u0004, MessageId.Err_NoPropertyForVarInout, Array.Empty<object>());
							return;
						}
						if (\u0005.IsVarInoutConstant && (\u0004.IsLiteral || \u0004.IsConstant(this.Scope, true)))
						{
							this.\u0001(\u0002, MessageId.Err_VariableForVarinoutConstant, new object[]
							{
								\u0005.OrgName,
								\u0003.OrgName
							});
							return;
						}
						if (\u0005._Type.DeRefType.Class == TypeClass.String || \u0005._Type.DeRefType.Class == TypeClass.WString)
						{
							this.\u0001(\u0004, MessageId.Err_LValueForVarinoutStrings, new object[]
							{
								\u0005.OrgName,
								\u0003.OrgName
							});
							return;
						}
						if (\u0005.HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY))
						{
							this.\u0002(\u0004, MessageId.Wrn_LValueForVarinoutStrings, new object[]
							{
								\u0005.OrgName,
								\u0003.OrgName
							});
							return;
						}
						this.\u0001(\u0004, MessageId.Err_LValueForVarinout, new object[]
						{
							\u0005.OrgName,
							\u0003.OrgName
						});
						return;
					}
				}
				else
				{
					bool flag3 = this.SignToCheck != null && this.SignToCheck.IsCompiledLibraryObject;
					if (!\u0005.IsVarInoutConstant && !\u0003.GetFlag(SignatureFlag.External) && !flag3)
					{
						bool flag4 = false;
						ICompiledType deRefType = \u0005._Type.DeRefType;
						ICompiledType deRefType2 = \u0004._CompiledType.DeRefType;
						TypeClass @class = deRefType.Class;
						TypeClass class2 = deRefType2.Class;
						if (@class == TypeClass.String && class2 == TypeClass.String)
						{
							ICompiledType compiledType = deRefType as _IStringType;
							_IStringType istringType = deRefType2 as _IStringType;
							if (compiledType.Size(this.Scope) > istringType.Size(this.Scope))
							{
								flag4 = true;
							}
						}
						else if (@class == TypeClass.WString && class2 == TypeClass.WString)
						{
							ICompiledType compiledType2 = deRefType as _IWStringType;
							_IWStringType iwstringType = deRefType2 as _IWStringType;
							if (compiledType2.Size(this.Scope) > iwstringType.Size(this.Scope))
							{
								flag4 = true;
							}
						}
						if (flag4)
						{
							this.\u0001(\u0004, MessageId.Err_StringTooShortForVarInOut, new object[]
							{
								\u0004,
								\u0005.OrgName,
								\u0003.OrgName
							});
						}
					}
				}
			}
		}

		// Token: 0x06002D85 RID: 11653 RVA: 0x000A72E0 File Offset: 0x000A54E0
		internal bool \u0001(_IExpression \u0002, IScope5 \u0003)
		{
			if (!typeof(_IVariableExpression).IsAssignableFrom(\u0002.GetType()) && !typeof(_ICompoAccessExpression).IsAssignableFrom(\u0002.GetType()) && !typeof(_IGlobalScopeExpression).IsAssignableFrom(\u0002.GetType()))
			{
				return true;
			}
			if (typeof(_IUserdefType).IsAssignableFrom(\u0002._CompiledType.GetType()) || typeof(_IEnumType).IsAssignableFrom(\u0002._CompiledType.GetType()))
			{
				ICompiledType compiledType = global::\u0006.\u0011.\u0001(\u0002._CompiledType, \u0003);
				if (compiledType != null && typeof(_IEnumType).IsAssignableFrom(compiledType.GetType()) && \u0002.GetVariable(\u0003) == null)
				{
					this.\u0001(\u0002, MessageId.Err_UnexpectedTypeName, new object[]
					{
						\u0002.ToString()
					});
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002D86 RID: 11654 RVA: 0x000A73BC File Offset: 0x000A55BC
		private bool \u0001(_IExpression \u0002, ICompiledType \u0003, int \u0004)
		{
			bool result = false;
			_IStringLiteralExpression2 istringLiteralExpression = \u0002 as _IStringLiteralExpression2;
			if (istringLiteralExpression != null && istringLiteralExpression.Type != null && TypeTable.IsString(istringLiteralExpression.Type.Class))
			{
				long num = global::\u0017.\u0003.Singleton.\u0001(istringLiteralExpression.StringValue, ByteOrder.Intel, \u0002.Type.Class, istringLiteralExpression.StringEncoding);
				if ((\u0002.Type.Class == TypeClass.String && \u0003.Class == TypeClass.String && num > (long)(\u0004 - 1)) || (\u0002.Type.Class == TypeClass.WString && \u0003.Class == TypeClass.WString && num > (long)(\u0004 - 2)))
				{
					this.\u0001(istringLiteralExpression, \u0003, \u0004);
					result = true;
				}
			}
			return result;
		}

		// Token: 0x06002D87 RID: 11655 RVA: 0x000A7464 File Offset: 0x000A5664
		private void \u0001(_IStringLiteralExpression2 \u0002, ICompiledType \u0003, int \u0004)
		{
			int length = \u0002.StringValue.Length;
			int num = (TypeClass.String == \u0003.Class) ? (\u0004 - 1) : ((\u0004 - 2) / 2);
			if (\u0003.Class == TypeClass.String && num > length)
			{
				num = length;
			}
			string text = \u0002.ToString();
			if (length < 1000)
			{
				if (num >= 3)
				{
					num -= 3;
				}
				text = text.Substring(0, num) + "...";
			}
			else
			{
				int num2 = length - num;
				string str = string.Format(\u0081.\u0001.TooLongStringLiteralHint, num2);
				int num3 = num / 2;
				text = text.Substring(0, num3) + str + text.Substring(num3 + num2);
			}
			this.\u0002(\u0002, MessageId.Wrn_StringConstantTooLong, new object[]
			{
				text,
				\u0003.ToString()
			});
		}

		// Token: 0x06002D88 RID: 11656 RVA: 0x000A7524 File Offset: 0x000A5724
		private void \u0002(_IExprement \u0002, MessageId \u0003, params object[] \u0004)
		{
			Severity severity = Severity.Warning;
			MessageHandling messageHandling = this.MessageSuppressionController.HandleMessage(\u0003, Severity.Warning);
			if (messageHandling != MessageHandling.Suppress)
			{
				if (messageHandling == MessageHandling.Ignore)
				{
					return;
				}
			}
			else
			{
				severity = Severity.SuppressedWarning;
			}
			string format = global::\u000E.\u0018.\u0001(\u0003);
			\u0002.AddMessage(string.Format(format, \u0004), \u0002._Position, severity, \u0002.LengthIntern, \u0003);
		}

		// Token: 0x06002D89 RID: 11657 RVA: 0x000A7570 File Offset: 0x000A5770
		public void \u0001(_IExprement \u0002, _ICompilerMessage \u0003)
		{
			\u0002.AddMessage(\u0003, false, true);
		}

		// Token: 0x06002D8A RID: 11658 RVA: 0x000A757C File Offset: 0x000A577C
		internal void \u0001(IScope5 \u0002, _IExprement \u0003, ICompiledType \u0004, ICompiledType \u0005)
		{
			this.\u0001(\u0002 as ICommonScope, \u0003, \u0004, \u0005);
		}

		// Token: 0x06002D8B RID: 11659 RVA: 0x000A7590 File Offset: 0x000A5790
		internal void \u0001(ICommonScope \u0002, _IExprement \u0003, ICompiledType \u0004, ICompiledType \u0005)
		{
			string text = global::\u000E.\u000F.\u0001(\u0002, \u0004);
			string text2 = global::\u000E.\u000F.\u0001(\u0002, \u0005);
			this.\u0001(\u0003, MessageId.Err_TypeMismatch, new object[]
			{
				text,
				text2
			});
		}

		// Token: 0x06002D8C RID: 11660 RVA: 0x000A75C8 File Offset: 0x000A57C8
		private void \u0001(_IExprement \u0002, IType \u0003, IType \u0004)
		{
			this.\u0002(\u0002, MessageId.Wrn_PointerMisatch, new object[]
			{
				\u0003,
				\u0004
			});
		}

		// Token: 0x06002D8D RID: 11661 RVA: 0x000A75E4 File Offset: 0x000A57E4
		private void \u0002(_ISignature \u0002)
		{
			if (this.AddCrossReferences && this.Compiledpou != null)
			{
				\u0002.AddCaller(this.Compiledpou.SignatureId);
			}
		}

		// Token: 0x06002D8E RID: 11662 RVA: 0x000A7608 File Offset: 0x000A5808
		private void \u0001(_ISignature \u0002, int \u0003)
		{
			if (this.AddCrossReferences)
			{
				\u0002.AddCallee(\u0003, false);
			}
		}

		// Token: 0x0400089D RID: 2205
		[CompilerGenerated]
		private IMessageSuppressionController \u0001;

		// Token: 0x0400089E RID: 2206
		[CompilerGenerated]
		private Guid \u0001;

		// Token: 0x0400089F RID: 2207
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x040008A0 RID: 2208
		[CompilerGenerated]
		private bool \u0002;

		// Token: 0x040008A1 RID: 2209
		[CompilerGenerated]
		private bool \u0003;

		// Token: 0x040008A2 RID: 2210
		[CompilerGenerated]
		private bool \u0004;

		// Token: 0x040008A3 RID: 2211
		[CompilerGenerated]
		private bool \u0005;

		// Token: 0x040008A4 RID: 2212
		[CompilerGenerated]
		private _ICompileContext \u0001;

		// Token: 0x040008A5 RID: 2213
		[CompilerGenerated]
		private _IScope2 \u0001;

		// Token: 0x040008A6 RID: 2214
		[CompilerGenerated]
		private _ICompiledPOU \u0001;

		// Token: 0x040008A7 RID: 2215
		[CompilerGenerated]
		private _ISignature \u0001;

		// Token: 0x040008A8 RID: 2216
		[CompilerGenerated]
		private readonly \u0081.\u0015 \u0001;
	}
}

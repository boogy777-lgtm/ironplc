using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.Services
{
	// Token: 0x0200024E RID: 590
	public class PreCompileSmartCodingService : ILMPreCompileSmartCodingService5, ILMPreCompileSmartCodingService4, ILMPreCompileSmartCodingService3, ILMPreCompileSmartCodingService2, ILMPreCompileSmartCodingService
	{
		// Token: 0x06002788 RID: 10120 RVA: 0x000633C2 File Offset: 0x000623C2
		public bool CheckPOUCode(ILMPreCompileSet preCompileSet, Guid guidObject, IList<IMessage4> compilermessages)
		{
			return ((_IPreCompileContext)preCompileSet).CheckPOUCode(guidObject, compilermessages);
		}

		// Token: 0x06002789 RID: 10121 RVA: 0x000633D4 File Offset: 0x000623D4
		public bool CheckSignature(ILMPreCompileSet preCompileSet, Guid guidObject, IList<IMessage4> compilermessages)
		{
			bool result = false;
			_ISignature isignature = preCompileSet.GetSignature(guidObject) as _ISignature;
			PreCompileContext preCompileContext = preCompileSet as PreCompileContext;
			if (isignature != null && preCompileContext != null)
			{
				preCompileContext.CheckSignature(isignature);
				if (compilermessages != null)
				{
					IList<_ICompilerMessage> precompileMessages = isignature.PrecompileMessages;
					if (precompileMessages != null)
					{
						foreach (_ICompilerMessage item in precompileMessages)
						{
							compilermessages.Add(item);
						}
					}
					result = (compilermessages.Count == 0);
				}
			}
			return result;
		}

		// Token: 0x0600278A RID: 10122 RVA: 0x00063460 File Offset: 0x00062460
		public void DeriveAccessPathInformation(ILMPreCompileSet preCompileSet, Guid guidSignature, int nProjectHandle, string stAccessPathOrType, out bool bError, out IPrecompileScope derivedScope, out ISignature derivedSignature, out IVariable derivedVariable, out IType derivedType, out IPrecompileScope searchScope)
		{
			((_IPreCompileContext)preCompileSet).DeriveAccessPathInformation(guidSignature, nProjectHandle, stAccessPathOrType, out bError, out derivedScope, out derivedSignature, out derivedVariable, out derivedType, out searchScope);
		}

		// Token: 0x0600278B RID: 10123 RVA: 0x00063488 File Offset: 0x00062488
		public IExprement FindExpressionAtSourcePosition(ISourcePosition sourcepos, WhatToFind whattofind, out ILMPreCompileSet precom)
		{
			IPreCompileContext preCompileContext;
			IExprement exprement = CompilerProxy.FindExpressionAtSourcePosition(sourcepos, whattofind, out preCompileContext);
			precom = (ILMPreCompileSet)preCompileContext;
			_IExprement iexprement = exprement as _IExprement;
			if (iexprement == null)
			{
				return null;
			}
			return iexprement.Duplicate();
		}

		// Token: 0x0600278C RID: 10124 RVA: 0x000634B6 File Offset: 0x000624B6
		public IEnumerable<IIdentifierInfo> FindSubelements(ILMPreCompileSet preCompileSet, Guid guidSignature, string stAccessPathOrType, FindSubelementsFlags flags, out bool bError)
		{
			return CompilerProxy.FindSubelements((_IPreCompileContext)preCompileSet, guidSignature, stAccessPathOrType, flags, out bError);
		}

		// Token: 0x0600278D RID: 10125 RVA: 0x000634C9 File Offset: 0x000624C9
		public IntellisenseScopeInfoFlags GetIntellisenseScopeInfoForItem(IIdentifierInfo iiItem)
		{
			return this.GetIntellisenseScopeInfoForItem(iiItem.Flags, iiItem.Variable, iiItem.Signature, ItemTypeFlags.None);
		}

		// Token: 0x0600278E RID: 10126 RVA: 0x000634E4 File Offset: 0x000624E4
		public IntellisenseScopeInfoFlags GetIntellisenseScopeInfoForItem(IVariable var, ISignature sig, ItemTypeFlags itemType)
		{
			return this.GetIntellisenseScopeInfoForItem(IdentifierInfoFlag.None, var, sig, itemType);
		}

		// Token: 0x0600278F RID: 10127 RVA: 0x000634F0 File Offset: 0x000624F0
		private IntellisenseScopeInfoFlags GetIntellisenseScopeInfoForItem(IdentifierInfoFlag infoFlags, IVariable var, ISignature sig, ItemTypeFlags itemTypeFlags)
		{
			IntellisenseScopeInfoFlags intellisenseScopeInfoFlags = IntellisenseScopeInfoFlags.None;
			if ((infoFlags & IdentifierInfoFlag.Global) != IdentifierInfoFlag.None || (infoFlags & IdentifierInfoFlag.Scope) != IdentifierInfoFlag.None)
			{
				intellisenseScopeInfoFlags |= IntellisenseScopeInfoFlags.GlobalScope;
			}
			else if ((infoFlags & IdentifierInfoFlag.Local) != IdentifierInfoFlag.None)
			{
				intellisenseScopeInfoFlags |= IntellisenseScopeInfoFlags.LocalScope;
			}
			if (var != null)
			{
				if (var.GetFlag(VarFlag.Global) || PreCompileSmartCodingService.IsEnumConstant(var))
				{
					intellisenseScopeInfoFlags |= IntellisenseScopeInfoFlags.GlobalScope;
				}
				else if (var.GetFlag(VarFlag.Local) || var.GetFlag(VarFlag.Input) || var.GetFlag(VarFlag.Output) || var.GetFlag(VarFlag.Temp))
				{
					intellisenseScopeInfoFlags |= IntellisenseScopeInfoFlags.LocalScope;
				}
			}
			if (sig != null)
			{
				if (sig.POUType == Operator.Program || sig.POUType == Operator.VarGlobal || sig.POUType == Operator.Function || sig.GetFlag(SignatureFlag.Global))
				{
					intellisenseScopeInfoFlags |= IntellisenseScopeInfoFlags.GlobalScope;
				}
				if (sig.POUType == Operator.Method)
				{
					intellisenseScopeInfoFlags |= IntellisenseScopeInfoFlags.LocalScope;
				}
			}
			if ((itemTypeFlags & ItemTypeFlags.NameSpaceItem) != ItemTypeFlags.None)
			{
				intellisenseScopeInfoFlags |= IntellisenseScopeInfoFlags.GlobalScope;
			}
			if ((itemTypeFlags & ItemTypeFlags.ConversionItem) != ItemTypeFlags.None || (itemTypeFlags & ItemTypeFlags.KeywordItem) != ItemTypeFlags.None || (itemTypeFlags & ItemTypeFlags.OperatorItem) != ItemTypeFlags.None || (itemTypeFlags & ItemTypeFlags.StandardDatatypeItem) != ItemTypeFlags.None)
			{
				intellisenseScopeInfoFlags |= IntellisenseScopeInfoFlags.KeywordScope;
			}
			return intellisenseScopeInfoFlags;
		}

		// Token: 0x06002790 RID: 10128 RVA: 0x000635D4 File Offset: 0x000625D4
		private static bool IsEnumConstant(IVariable var)
		{
			return var != null && var.GetFlag(VarFlag.Enum) && (var.GetFlag(VarFlag.Constant) || var.GetFlag(VarFlag.ReplacedConstant));
		}

		// Token: 0x06002791 RID: 10129 RVA: 0x00063600 File Offset: 0x00062600
		public IExpressionInfo GetExpressionInfo(ILMPreCompileSet preCompileSet, Guid guidSignature, string stExpression)
		{
			return CompilerProxy.GetExpressionInfo((_IPreCompileContext)preCompileSet, guidSignature, stExpression);
		}

		// Token: 0x06002792 RID: 10130 RVA: 0x0006360F File Offset: 0x0006260F
		public IEnumerable<IIdentifierInfo> GetIdentifierInfo(ILMPreCompileSet preCompileSet, Guid guidSignature, string stAccessPath)
		{
			return CompilerProxy.GetIdentifierInfo((_IPreCompileContext)preCompileSet, guidSignature, stAccessPath);
		}

		// Token: 0x06002793 RID: 10131 RVA: 0x0006361E File Offset: 0x0006261E
		public IEnumerable<IIdentifierInfo> GetIdentifierInfoAtSourcePosition(string stName, ISourcePosition sourcepos, WhatToFind whattofind)
		{
			return CompilerProxy.GetIdentifierInfoAtSourcePosition(stName, sourcepos, whattofind);
		}

		// Token: 0x06002794 RID: 10132 RVA: 0x00063628 File Offset: 0x00062628
		public IEnumerable<IDeclarationInfo> ParseForUnknownIdentifiers(ILMPreCompileSet preCompileSet, string stCode, string stPOUName, string stSubObjectName)
		{
			return CompilerProxy.ParseForUnknownIdentifiers((_IPreCompileContext)preCompileSet, stCode, stPOUName, stSubObjectName);
		}

		// Token: 0x06002795 RID: 10133 RVA: 0x0006363C File Offset: 0x0006263C
		public IEnumerable<string> GetConversionOperators()
		{
			_IScanner iscanner = CompilerProxy.CreateScanner();
			Operator[] dataTypes = iscanner.GetDataTypes();
			LHashSet<string> lhashSet = new LHashSet<string>();
			foreach (Operator @operator in dataTypes)
			{
				if (@operator != Operator.Any)
				{
					string text = iscanner._GetTextOfOperator(@operator, true);
					lhashSet.Add("TO_" + text);
					foreach (Operator operator2 in dataTypes)
					{
						if (@operator != operator2 && operator2 != Operator.Any)
						{
							string str = iscanner._GetTextOfOperator(operator2, true);
							lhashSet.Add(text + "_TO_" + str);
						}
					}
				}
			}
			return lhashSet;
		}

		// Token: 0x06002796 RID: 10134 RVA: 0x000636E4 File Offset: 0x000626E4
		public IEnumerable<string> GetOverloadedConversionOperators()
		{
			_IScanner iscanner = CompilerProxy.CreateScanner();
			Operator[] dataTypes = iscanner.GetDataTypes();
			LHashSet<string> lhashSet = new LHashSet<string>();
			foreach (Operator @operator in dataTypes)
			{
				if (@operator != Operator.Any)
				{
					string str = iscanner._GetTextOfOperator(@operator, true);
					lhashSet.Add("TO_" + str);
				}
			}
			return lhashSet;
		}

		// Token: 0x06002797 RID: 10135 RVA: 0x0006373B File Offset: 0x0006273B
		public string WriteExprement(IExprement exprement, WriteExprementFlags flags)
		{
			return CompilerProxy._ExprementWriter.WriteExprement((_IExprement)exprement, flags);
		}

		// Token: 0x06002798 RID: 10136 RVA: 0x0006374E File Offset: 0x0006274E
		public IEnumerable<ISourcePosition> GetUnusedStatementPositions(Guid guidApplication, ISignature signature)
		{
			return CompilerProxy.GetUnusedStatementPositions(guidApplication, signature);
		}

		// Token: 0x06002799 RID: 10137 RVA: 0x00063757 File Offset: 0x00062757
		public IEnumerable<ISourcePosition> GetUnusedStatementPositions(Guid guidApplication, ISignature signature, EPouScopeFlags eForWhichScope)
		{
			return CompilerProxy.GetUnusedStatementPositions(guidApplication, signature, eForWhichScope);
		}

		// Token: 0x0600279A RID: 10138 RVA: 0x00063761 File Offset: 0x00062761
		public IExpressionInfo GetExpressionInfo(ILMPreCompileSet preCompileSet, Guid guidSignature, string stExpression, bool bImplicit)
		{
			return CompilerProxy.GetExpressionInfo((_IPreCompileContext)preCompileSet, guidSignature, stExpression, bImplicit);
		}
	}
}

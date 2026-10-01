using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000097 RID: 151
	[TypeGuid("{516f9e24-1aba-4ad8-8ca7-e8eab121bdee}")]
	[StorageVersion("3.5.22.0")]
	public class MethodDeclarationStatement : POUDeclarationStatement, _IMethodDeclarationStatement, _IPOUDeclarationStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IPOUDeclarationStatement
	{
		// Token: 0x06000932 RID: 2354 RVA: 0x000157DF File Offset: 0x000147DF
		public MethodDeclarationStatement()
		{
		}

		// Token: 0x06000933 RID: 2355 RVA: 0x000157E7 File Offset: 0x000147E7
		public MethodDeclarationStatement(IToken token) : base(token)
		{
		}

		// Token: 0x17000218 RID: 536
		// (get) Token: 0x06000934 RID: 2356 RVA: 0x000157F0 File Offset: 0x000147F0
		// (set) Token: 0x06000935 RID: 2357 RVA: 0x000157F8 File Offset: 0x000147F8
		[DefaultSerialization("Override")]
		[StorageVersion("3.5.22.0")]
		[StorageDefaultValue(false)]
		public bool Override { get; set; }

		// Token: 0x17000219 RID: 537
		// (get) Token: 0x06000936 RID: 2358 RVA: 0x00015801 File Offset: 0x00014801
		// (set) Token: 0x06000937 RID: 2359 RVA: 0x00015809 File Offset: 0x00014809
		[DefaultSerialization("Overload")]
		[StorageVersion("3.5.22.0")]
		[StorageDefaultValue(false)]
		public bool Overload { get; set; }

		// Token: 0x06000938 RID: 2360 RVA: 0x00015814 File Offset: 0x00014814
		public override _IExprement Duplicate()
		{
			MethodDeclarationStatement methodDeclarationStatement = new MethodDeclarationStatement();
			this.DuplicateCommon(methodDeclarationStatement);
			methodDeclarationStatement.Name = base.Name;
			methodDeclarationStatement.NameExpression = base.NameExpression;
			methodDeclarationStatement.Type = base.Type;
			methodDeclarationStatement.Class = base.Class;
			methodDeclarationStatement.Access = base.Access;
			methodDeclarationStatement.Declarations = (base.Declarations.Duplicate() as Statement);
			methodDeclarationStatement.Override = this.Override;
			methodDeclarationStatement.Overload = this.Overload;
			if (base.Implements != null)
			{
				foreach (_IExpression iexpression in base.Implements)
				{
					methodDeclarationStatement.AddInterfaceImplementation(iexpression.Duplicate() as _IExpression);
				}
			}
			if (base.Extends != null)
			{
				foreach (_IExpression iexpression2 in base.Extends)
				{
					methodDeclarationStatement.Extends.Add(iexpression2.Duplicate() as _IExpression);
				}
			}
			return methodDeclarationStatement;
		}
	}
}

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000039 RID: 57
	internal static class LegacyDelegateFactory
	{
		// Token: 0x060000E3 RID: 227 RVA: 0x00002C4C File Offset: 0x00000E4C
		public static LegacyGetterDelegate CreateGetter(PropertyInfo property)
		{
			DynamicMethod dynamicMethod = new DynamicMethod("GetterP", typeof(object), LegacyDelegateFactory.ONE_OBJECT, property.DeclaringType, true);
			ILGenerator ilgenerator = dynamicMethod.GetILGenerator();
			MethodInfo getMethod = property.GetGetMethod(true);
			ilgenerator.Emit(OpCodes.Ldarg_0);
			if (getMethod.IsFinal)
			{
				ilgenerator.Emit(OpCodes.Call, getMethod);
			}
			else
			{
				ilgenerator.Emit(OpCodes.Callvirt, getMethod);
			}
			if (getMethod.ReturnType.IsValueType)
			{
				ilgenerator.Emit(OpCodes.Box, property.PropertyType);
			}
			ilgenerator.Emit(OpCodes.Ret);
			return (LegacyGetterDelegate)dynamicMethod.CreateDelegate(typeof(LegacyGetterDelegate));
		}

		// Token: 0x060000E4 RID: 228 RVA: 0x00002CF4 File Offset: 0x00000EF4
		public static LegacyGetterDelegate CreateGetter(FieldInfo field)
		{
			DynamicMethod dynamicMethod = new DynamicMethod("GetterF", typeof(object), LegacyDelegateFactory.ONE_OBJECT, field.DeclaringType, true);
			ILGenerator ilgenerator = dynamicMethod.GetILGenerator();
			ilgenerator.Emit(OpCodes.Ldarg_0);
			ilgenerator.Emit(OpCodes.Ldfld, field);
			if (field.FieldType.IsValueType)
			{
				ilgenerator.Emit(OpCodes.Box, field.FieldType);
			}
			ilgenerator.Emit(OpCodes.Ret);
			return (LegacyGetterDelegate)dynamicMethod.CreateDelegate(typeof(LegacyGetterDelegate));
		}

		// Token: 0x060000E5 RID: 229 RVA: 0x00002D7C File Offset: 0x00000F7C
		public static LegacySetterDelegate CreateSetter(PropertyInfo property)
		{
			DynamicMethod dynamicMethod = new DynamicMethod("SetterP", null, LegacyDelegateFactory.TWO_OBJECTS, property.DeclaringType, true);
			ILGenerator ilgenerator = dynamicMethod.GetILGenerator();
			MethodInfo setMethod = property.GetSetMethod(true);
			ilgenerator.Emit(OpCodes.Ldarg_0);
			ilgenerator.Emit(OpCodes.Ldarg_1);
			if (property.PropertyType.IsValueType)
			{
				ilgenerator.Emit(OpCodes.Unbox_Any, property.PropertyType);
			}
			if (setMethod.IsFinal)
			{
				ilgenerator.Emit(OpCodes.Call, setMethod);
			}
			else
			{
				ilgenerator.Emit(OpCodes.Callvirt, setMethod);
			}
			ilgenerator.Emit(OpCodes.Ret);
			return (LegacySetterDelegate)dynamicMethod.CreateDelegate(typeof(LegacySetterDelegate));
		}

		// Token: 0x060000E6 RID: 230 RVA: 0x00002E24 File Offset: 0x00001024
		public static LegacySetterDelegate CreateSetter(FieldInfo field)
		{
			DynamicMethod dynamicMethod = new DynamicMethod("SetterF", null, LegacyDelegateFactory.TWO_OBJECTS, field.DeclaringType, true);
			ILGenerator ilgenerator = dynamicMethod.GetILGenerator();
			ilgenerator.Emit(OpCodes.Ldarg_0);
			ilgenerator.Emit(OpCodes.Ldarg_1);
			if (field.FieldType.IsValueType)
			{
				ilgenerator.Emit(OpCodes.Unbox_Any, field.FieldType);
			}
			ilgenerator.Emit(OpCodes.Stfld, field);
			ilgenerator.Emit(OpCodes.Ret);
			return (LegacySetterDelegate)dynamicMethod.CreateDelegate(typeof(LegacySetterDelegate));
		}

		// Token: 0x04000031 RID: 49
		private static readonly Type[] ONE_OBJECT = new Type[]
		{
			typeof(object)
		};

		// Token: 0x04000032 RID: 50
		private static readonly Type[] TWO_OBJECTS = new Type[]
		{
			typeof(object),
			typeof(object)
		};
	}
}

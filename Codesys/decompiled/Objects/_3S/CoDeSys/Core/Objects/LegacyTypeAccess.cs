using System;
using System.Collections.Generic;
using System.Reflection;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200003F RID: 63
	internal class LegacyTypeAccess
	{
		// Token: 0x0600010D RID: 269 RVA: 0x000039EC File Offset: 0x00001BEC
		private LegacyTypeAccess(StorageVersionAttribute storageVersionAttribute, bool bSetSerializableValue2IsOverridden, bool bSetSerializableValue3IsOverridden)
		{
			this.StorageVersionAttribute = storageVersionAttribute;
			this.SetSerializableValue2IsOverridden = bSetSerializableValue2IsOverridden;
			this.SetSerializableValue3IsOverridden = bSetSerializableValue3IsOverridden;
		}

		// Token: 0x0600010E RID: 270 RVA: 0x00003A20 File Offset: 0x00001C20
		public static LegacyTypeAccess Create(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			MethodInfo method = type.GetMethod("SetSerializableValue", LegacyTypeAccess.SET_SERIALIZABLE_VALUE_2_TYPES);
			bool bSetSerializableValue2IsOverridden = method != null && method.DeclaringType != typeof(GenericObject);
			MethodInfo method2 = type.GetMethod("SetSerializableValue", LegacyTypeAccess.SET_SERIALIZABLE_VALUE_3_TYPES);
			bool bSetSerializableValue3IsOverridden = method2 != null && method2.DeclaringType != typeof(GenericObject2);
			StorageVersionAttribute storageVersionAttribute = null;
			object[] customAttributes = type.GetCustomAttributes(typeof(StorageVersionAttribute), false);
			if (customAttributes != null && customAttributes.Length != 0 && customAttributes[0] is StorageVersionAttribute)
			{
				storageVersionAttribute = (StorageVersionAttribute)customAttributes[0];
			}
			LegacyTypeAccess legacyTypeAccess = new LegacyTypeAccess(storageVersionAttribute, bSetSerializableValue2IsOverridden, bSetSerializableValue3IsOverridden);
			MemberInfo[] members = type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			int i = 0;
			while (i < members.Length)
			{
				MemberInfo memberInfo = members[i];
				customAttributes = memberInfo.GetCustomAttributes(typeof(DefaultSerializationAttribute), true);
				if (customAttributes != null && customAttributes.Length != 0 && customAttributes[0] is DefaultSerializationAttribute)
				{
					string valueName = ((DefaultSerializationAttribute)customAttributes[0]).ValueName;
					Type type2;
					LegacyGetterDelegate getter;
					LegacySetterDelegate setter;
					if (memberInfo is PropertyInfo)
					{
						type2 = ((PropertyInfo)memberInfo).PropertyType;
						getter = LegacyDelegateFactory.CreateGetter((PropertyInfo)memberInfo);
						setter = LegacyDelegateFactory.CreateSetter((PropertyInfo)memberInfo);
					}
					else
					{
						if (!(memberInfo is FieldInfo))
						{
							goto IL_362;
						}
						type2 = ((FieldInfo)memberInfo).FieldType;
						getter = LegacyDelegateFactory.CreateGetter((FieldInfo)memberInfo);
						setter = LegacyDelegateFactory.CreateSetter((FieldInfo)memberInfo);
					}
					StorageVersionAttribute storageVersion = null;
					customAttributes = memberInfo.GetCustomAttributes(typeof(StorageVersionAttribute), true);
					if (customAttributes != null && customAttributes.Length != 0 && customAttributes[0] is StorageVersionAttribute)
					{
						storageVersion = (StorageVersionAttribute)customAttributes[0];
					}
					bool bIgnorable = false;
					customAttributes = memberInfo.GetCustomAttributes(typeof(StorageIgnorableAttribute), true);
					if (customAttributes != null && customAttributes.Length != 0 && customAttributes[0] is StorageIgnorableAttribute)
					{
						bIgnorable = true;
					}
					StorageDefaultValueAttribute defaultValue = null;
					customAttributes = memberInfo.GetCustomAttributes(typeof(StorageDefaultValueAttribute), true);
					if (customAttributes != null && customAttributes.Length != 0 && customAttributes[0] is StorageDefaultValueAttribute)
					{
						defaultValue = (StorageDefaultValueAttribute)customAttributes[0];
					}
					StorageSaveAsArrayAttribute saveAsArray = null;
					customAttributes = memberInfo.GetCustomAttributes(typeof(StorageSaveAsArrayAttribute), true);
					if (customAttributes != null && customAttributes.Length != 0 && customAttributes[0] is StorageSaveAsArrayAttribute)
					{
						saveAsArray = (StorageSaveAsArrayAttribute)customAttributes[0];
					}
					StorageSaveAsNonGenericCollectionAttribute saveAsNonGenericCollection = null;
					customAttributes = memberInfo.GetCustomAttributes(typeof(StorageSaveAsNonGenericCollectionAttribute), true);
					if (customAttributes != null && customAttributes.Length != 0 && customAttributes[0] is StorageSaveAsNonGenericCollectionAttribute)
					{
						saveAsNonGenericCollection = (StorageSaveAsNonGenericCollectionAttribute)customAttributes[0];
					}
					LegacySerializableMemberAccess value = new LegacySerializableMemberAccess(valueName, storageVersion, bIgnorable, defaultValue, saveAsArray, saveAsNonGenericCollection, type2, getter, setter);
					legacyTypeAccess.SerializableMemberAccesses[valueName] = value;
					goto IL_29C;
				}
				goto IL_29C;
				IL_362:
				i++;
				continue;
				IL_29C:
				customAttributes = memberInfo.GetCustomAttributes(typeof(DefaultDuplicationAttribute), true);
				if (customAttributes != null && customAttributes.Length != 0 && customAttributes[0] is DefaultDuplicationAttribute)
				{
					DuplicationMethod method3 = ((DefaultDuplicationAttribute)customAttributes[0]).Method;
					Type type3;
					LegacyGetterDelegate getter2;
					LegacySetterDelegate setter2;
					if (memberInfo is PropertyInfo)
					{
						type3 = ((PropertyInfo)memberInfo).PropertyType;
						getter2 = LegacyDelegateFactory.CreateGetter((PropertyInfo)memberInfo);
						setter2 = LegacyDelegateFactory.CreateSetter((PropertyInfo)memberInfo);
					}
					else
					{
						if (!(memberInfo is FieldInfo))
						{
							goto IL_362;
						}
						type3 = ((FieldInfo)memberInfo).FieldType;
						getter2 = LegacyDelegateFactory.CreateGetter((FieldInfo)memberInfo);
						setter2 = LegacyDelegateFactory.CreateSetter((FieldInfo)memberInfo);
					}
					LegacyCloneableMemberAccess item = new LegacyCloneableMemberAccess(method3, type3, getter2, setter2);
					legacyTypeAccess.CloneableMemberAccesses.Add(item);
					goto IL_362;
				}
				goto IL_362;
			}
			return legacyTypeAccess;
		}

		// Token: 0x0400003D RID: 61
		public readonly Dictionary<string, LegacySerializableMemberAccess> SerializableMemberAccesses = new Dictionary<string, LegacySerializableMemberAccess>();

		// Token: 0x0400003E RID: 62
		public readonly List<LegacyCloneableMemberAccess> CloneableMemberAccesses = new List<LegacyCloneableMemberAccess>();

		// Token: 0x0400003F RID: 63
		public readonly StorageVersionAttribute StorageVersionAttribute;

		// Token: 0x04000040 RID: 64
		public readonly bool SetSerializableValue2IsOverridden;

		// Token: 0x04000041 RID: 65
		public readonly bool SetSerializableValue3IsOverridden;

		// Token: 0x04000042 RID: 66
		private static readonly Type[] SET_SERIALIZABLE_VALUE_2_TYPES = new Type[]
		{
			typeof(string),
			typeof(object)
		};

		// Token: 0x04000043 RID: 67
		private static readonly Type[] SET_SERIALIZABLE_VALUE_3_TYPES = new Type[]
		{
			typeof(string),
			typeof(object),
			typeof(IArchiveReporter)
		};
	}
}

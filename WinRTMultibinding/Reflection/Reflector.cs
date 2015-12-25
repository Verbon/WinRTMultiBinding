using System;
using System.Reflection;

namespace WinRTMultibinding.Reflection
{
    internal static class Reflector
    {
        public static TMember ScanHierarchyForMember<TMember>(Type type, Func<TypeInfo, TMember> memberExtractor) where TMember : MemberInfo
        {
            while (type != typeof(object))
            {
                var typeInfo = type.GetTypeInfo();
                TMember memberInfo;

                if ((memberInfo = memberExtractor(typeInfo)) != null)
                {
                    return memberInfo;
                }

                type = typeInfo.BaseType;
            }

            return null;
        }
    }
}
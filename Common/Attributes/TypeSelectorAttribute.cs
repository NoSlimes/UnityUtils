using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace NoSlimes.UnityUtils.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class TypeSelectorAttribute : PropertyAttribute
    {
        public Type BaseType;
        public bool IncludeAbstract;

        public TypeSelectorAttribute(Type baseType, bool includeAbstract = false)
        {
            BaseType = baseType;
            IncludeAbstract = includeAbstract;
        }
    }
}

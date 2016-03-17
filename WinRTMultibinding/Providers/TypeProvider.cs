using System;
using WinRTMultibinding.Interfaces;

namespace WinRTMultibinding.Providers
{
    public abstract class TypeProvider<T> : ITypeProvider
    {
        private Type _targetType;


        Type ITypeProvider.GetType()
            => _targetType ?? (_targetType = typeof (T));
    }
}
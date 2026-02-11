using System;

namespace TwitchNotifier.Configurations
{
    internal abstract class BaseConfiguration
    {
        public virtual string GetSectionName() => null;

        public abstract void Validate();

        public void ThrowIfStringIsEmpty(string value, string propertyName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new Exception($"[{GetSectionName()}] {propertyName} must not be empty");
            }
        }

        public void ThrowIfIntIsInferiorAt(int value, int threshold, string propertyName)
        {
            if (value < threshold)
            {
                throw new Exception($"[{GetSectionName()}] {propertyName} must be >= {threshold}");
            }
        }
    }
}

namespace OpenInput.Mechanics.Input
{
    using System;

    public struct InputAxis
    {
        /// <summary>
        /// Initialize a new <see cref="InputAxis"/> structure.
        /// </summary>
        public InputAxis(string name, InputKey key, float value)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (float.IsInfinity(value) || float.IsNaN(value)) throw new ArgumentOutOfRangeException(nameof(value));

            this.Name = name;
            this.Key = key;
            this.Value = value;
        }

        /// <summary>
        /// Gets the action name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the key.
        /// </summary>
        public InputKey Key { get; }

        /// <summary>
        /// Gets the value of the axis.
        /// </summary>
        public float Value { get; }
    }
}

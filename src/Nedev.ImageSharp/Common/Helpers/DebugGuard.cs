// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Diagnostics;

// TODO: These should just call the guard equivalents
namespace Nedev.ImageSharp
{
    /// <summary>
    /// Provides methods to protect against invalid parameters for a DEBUG build.
    /// </summary>
    internal static partial class DebugGuard
    {
        /// <summary>
        /// Verifies whether a specific condition is met, throwing an exception if it's false.
        /// </summary>
        /// <param name="target">The condition</param>
        /// <param name="message">The error message</param>
        [Conditional("DEBUG")]
        public static void IsTrue(bool target, string message)
        {
            if (!target)
            {
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Verifies whether a condition (indicating disposed state) is met, throwing an ObjectDisposedException if it's true.
        /// </summary>
        /// <param name="isDisposed">Whether the object is disposed.</param>
        /// <param name="objectName">The name of the object.</param>
        [Conditional("DEBUG")]
        public static void NotDisposed(bool isDisposed, string objectName)
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(objectName);
            }
        }

        /// <summary>
        /// Verifies, that the target span is of same size than the 'other' span.
        /// </summary>
        /// <typeparam name="T">The element type of the spans</typeparam>
        /// <param name="target">The target span.</param>
        /// <param name="other">The 'other' span to compare 'target' to.</param>
        /// <param name="parameterName">The name of the parameter that is to be checked.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="target"/> has a different size than <paramref name="other"/>
        /// </exception>
        [Conditional("DEBUG")]
        public static void MustBeSameSized<T>(ReadOnlySpan<T> target, ReadOnlySpan<T> other, string parameterName)
            where T : struct
        {
            if (target.Length != other.Length)
            {
                throw new ArgumentException("Span-s must be the same size!", parameterName);
            }
        }

        /// <summary>
        /// Verifies, that the `target` span has the length of 'minSpan', or longer.
        /// </summary>
        /// <typeparam name="T">The element type of the spans</typeparam>
        /// <param name="target">The target span.</param>
        /// <param name="minSpan">The 'minSpan' span to compare 'target' to.</param>
        /// <param name="parameterName">The name of the parameter that is to be checked.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="target"/> has less items than <paramref name="minSpan"/>
        /// </exception>
        [Conditional("DEBUG")]
        public static void MustBeSizedAtLeast<T>(ReadOnlySpan<T> target, ReadOnlySpan<T> minSpan, string parameterName)
            where T : struct
        {
            if (target.Length < minSpan.Length)
            {
                throw new ArgumentException($"Span-s must be at least of length {minSpan.Length}!", parameterName);
            }
        }

        [Conditional("DEBUG")]
        public static void MustBeGreaterThan<T>(T value, T threshold, string parameterName)
            where T : IComparable<T>
        {
            if (value.CompareTo(threshold) <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be greater than {threshold}.");
            }
        }

        [Conditional("DEBUG")]
        public static void MustBeGreaterThan(double value, double threshold, string parameterName)
        {
            if (value <= threshold)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be greater than {threshold}.");
            }
        }

        [Conditional("DEBUG")]
        public static void MustBeGreaterThan(long value, long threshold, string parameterName)
        {
            if (value <= threshold)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be greater than {threshold}.");
            }
        }

        [Conditional("DEBUG")]
        public static void MustBeLessThan<T>(T value, T max, string parameterName)
            where T : IComparable<T>
        {
            if (value.CompareTo(max) >= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be less than {max}.");
            }
        }

        [Conditional("DEBUG")]
        public static void MustBeLessThan(double value, double max, string parameterName)
        {
            if (value >= max)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be less than {max}.");
            }
        }

        [Conditional("DEBUG")]
        public static void MustBeLessThan(long value, long max, string parameterName)
        {
            if (value >= max)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be less than {max}.");
            }
        }

        [Conditional("DEBUG")]
        public static void MustBeLessThanOrEqualTo<T>(T value, T max, string parameterName)
            where T : IComparable<T>
        {
            if (value.CompareTo(max) > 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be less than or equal to {max}.");
            }
        }

        [Conditional("DEBUG")]
        public static void MustBeBetweenOrEqualTo<T>(T value, T min, T max, string parameterName)
            where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be between {min} and {max} (inclusive).");
            }
        }

        [Conditional("DEBUG")]
        public static void MustBeGreaterThanOrEqualTo<T>(T value, T min, string parameterName)
            where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be greater than or equal to {min}.");
            }
        }

        [Conditional("DEBUG")]
        public static void NotNull(object value, string parameterName)
        {
            if (value is null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        [Conditional("DEBUG")]
        public static void NotNullOrWhiteSpace(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", parameterName);
            }
        }

        [Conditional("DEBUG")]
        public static void IsTrue(bool target, string parameterName, string message)
        {
            if (!target)
            {
                throw new ArgumentException(message, parameterName);
            }
        }

        [Conditional("DEBUG")]
        public static void IsFalse(bool condition, string parameterName, string message)
        {
            if (condition)
            {
                throw new ArgumentException(message, parameterName);
            }
        }
    }
}


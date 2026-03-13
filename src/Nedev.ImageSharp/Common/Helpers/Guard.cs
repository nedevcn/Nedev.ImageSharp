// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Runtime.CompilerServices;
using Nedev.ImageSharp;

namespace Nedev.ImageSharp
{
    internal static partial class Guard
    {
        /// <summary>
        /// Ensures that the value is a value type.
        /// </summary>
        /// <param name="value">The target object, which cannot be null.</param>
        /// <param name="parameterName">The name of the parameter that is to be checked.</param>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <exception cref="ArgumentException"><paramref name="value"/> is not a value type.</exception>
        [MethodImpl(InliningOptions.ShortMethod)]
        public static void MustBeValueType<TValue>(TValue value, string parameterName)
        {
            if (value.GetType().IsValueType)
            {
                return;
            }

            throw new ArgumentException("Type must be a struct.", parameterName);
        }

        public static void NotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value is null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        public static void DestinationShouldNotBeTooShort<TSource, TDestination>(Span<TSource> source, Span<TDestination> destination, string parameterName)
        {
            if (destination.Length < source.Length)
            {
                throw new ArgumentException("Destination span is too short.", parameterName);
            }
        }

        public static void DestinationShouldNotBeTooShort<TSource, TDestination>(ReadOnlySpan<TSource> source, Span<TDestination> destination, string parameterName)
        {
            if (destination.Length < source.Length)
            {
                throw new ArgumentException("Destination span is too short.", parameterName);
            }
        }

        public static void MustBeGreaterThan<T>(T value, T threshold, string parameterName)
            where T : IComparable<T>
        {
            if (value.CompareTo(threshold) <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be greater than {threshold}.");
            }
        }

        public static void MustBeGreaterThanOrEqualTo<T>(T value, T threshold, string parameterName)
            where T : IComparable<T>
        {
            if (value.CompareTo(threshold) < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be greater than or equal to {threshold}.");
            }
        }

        public static void IsTrue(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        public static void IsTrue(bool condition, string parameterName, string message)
        {
            if (!condition)
            {
                throw new ArgumentException(message, parameterName);
            }
        }

        public static void IsFalse(bool condition, string parameterName, string message)
        {
            if (condition)
            {
                throw new ArgumentException(message, parameterName);
            }
        }

        public static void MustBeLessThan<T>(T value, T max, string parameterName)
            where T : IComparable<T>
        {
            if (value.CompareTo(max) >= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be less than {max}.");
            }
        }

        public static void MustBeLessThanOrEqualTo<T>(T value, T max, string parameterName)
            where T : IComparable<T>
        {
            if (value.CompareTo(max) > 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be less than or equal to {max}.");
            }
        }

        public static void MustBeBetweenOrEqualTo<T>(T value, T min, T max, string parameterName)
            where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be between {min} and {max} (inclusive).");
            }
        }

        public static void MustBeGreaterThan(double value, double min, string parameterName)
        {
            if (value <= min)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be greater than {min}.");
            }
        }

        public static void MustBeGreaterThanOrEqualTo(double value, double min, string parameterName)
        {
            if (value < min)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be greater than or equal to {min}.");
            }
        }

        public static void MustBeLessThan(double value, double max, string parameterName)
        {
            if (value >= max)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be less than {max}.");
            }
        }

        public static void MustBeLessThanOrEqualTo(double value, double max, string parameterName)
        {
            if (value > max)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be less than or equal to {max}.");
            }
        }

        public static void MustBeGreaterThan(long value, long min, string parameterName)
        {
            if (value <= min)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be greater than {min}.");
            }
        }

        public static void MustBeGreaterThanOrEqualTo(long value, long min, string parameterName)
        {
            if (value < min)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be greater than or equal to {min}.");
            }
        }

        public static void MustBeLessThan(long value, long max, string parameterName)
        {
            if (value >= max)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be less than {max}.");
            }
        }

        public static void MustBeLessThanOrEqualTo(long value, long max, string parameterName)
        {
            if (value > max)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be less than or equal to {max}.");
            }
        }

        public static void NotNullOrWhiteSpace(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", parameterName);
            }
        }
    }
}


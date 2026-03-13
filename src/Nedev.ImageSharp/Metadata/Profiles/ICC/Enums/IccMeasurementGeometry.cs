// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace Nedev.ImageSharp.Metadata.Profiles.Icc
{
    /// <summary>
    /// Measurement Geometry
    /// </summary>
    internal enum IccMeasurementGeometry : uint
    {
        /// <summary>
        /// Unknown geometry
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Geometry of 0бу:45бу or 45бу:0бу
        /// </summary>
        Degree0To45Or45To0 = 1,

        /// <summary>
        /// Geometry of 0бу:d or d:0бу
        /// </summary>
        Degree0ToDOrDTo0 = 2,
    }
}


using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace TFnsc.BinaryConverter.Tests.Models
{
    public class Coordinate : IEquatable<Coordinate>
    {
        public int Id { get; set; }

        public float Lat { get; set; }

        public float Long { get; set; }

        public DateTime Searched { get; set; }

        public byte AccuracyPercentage { get; set; }

        public bool Equals([AllowNull] Coordinate other)
        {
            if (other == null)
                return false;
            return Id == other.Id
                && Lat == other.Lat
                && Long == other.Long
                && Searched == other.Searched
                && AccuracyPercentage == other.AccuracyPercentage;
        }
    }
}

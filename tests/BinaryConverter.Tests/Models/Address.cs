using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace TFnsc.BinaryConverter.Tests.Models
{
    public class Address : IEquatable<Address>
    {
        public string Street { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Country { get; set; }

        public int Number { get; set; }

        public PlaceType Type { get; set; }

        public Coordinate Coordinate { get; set; }

        public bool Equals([AllowNull] Address other) =>
            other != null &&
            Street == other.Street &&
            City == other.City &&
            State == other.State &&
            Country == other.Country &&
            Number == other.Number &&
            Type == other.Type &&
            Coordinate.Equals(other.Coordinate);
    }
}

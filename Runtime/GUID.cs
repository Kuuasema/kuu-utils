using System;
using System.Runtime.InteropServices;
using UnityEngine;

// Based on: 
// https://ivanderevianko.com/2016/04/unity3d-serialize-and-deserialize-system-guid-using-jsonutility
// and
// https://forum.unity.com/threads/cannot-serialize-a-guid-field-in-class.156862/



namespace Kuuasema.Utils {

    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public class GUID : IComparable, IComparable<GUID>, IEquatable<GUID> {

        public static readonly GUID Zero = new GUID(0, 0, 0, 0);

        [FieldOffset(0)]
        public Guid Guid;
        [FieldOffset(0), SerializeField]
        private uint Part1;
        [FieldOffset(4), SerializeField]
        private uint Part2;
        [FieldOffset(8), SerializeField]
        private uint Part3;
        [FieldOffset(12), SerializeField]
        private uint Part4;

        public GUID(Guid Guid) {
            // parts must be assigned to satisfy struct constructor
            // the assignment to Guid will reassign those so they just need to be initialized
            this.Part1 = 0;
            this.Part2 = 0;
            this.Part3 = 0;
            this.Part4 = 0;
            this.Guid = Guid;
        }

        public GUID(uint part1, uint part2, uint part3, uint part4) {
            this.Guid = Guid.NewGuid();
            this.Part1 = part1;
            this.Part2 = part2;
            this.Part3 = part3;
            this.Part4 = part4;
        }

        public static GUID Create() {
            return new GUID(Guid.NewGuid());
        }

        public void Renew() {
            this.Guid = Guid.NewGuid();
        }

        public static implicit operator GUID(Guid guid) {
            return new GUID(guid);
        }

        public static implicit operator Guid(GUID serializableGuid) {
            return serializableGuid.Guid;
        }

        public int CompareTo(object value) {
            if (value == null) {
                return 1;
            }
            if (!(value is GUID)) {
                throw new ArgumentException("Must be GUID");
            }
            GUID guid = (GUID) value;
            return guid.Guid.CompareTo(this.Guid);
        }

        public int CompareTo(GUID other) {
            return other.Guid.CompareTo(this.Guid);
        }

        public bool Equals(GUID other) {
            return other.Guid.Equals(this.Guid);
        }

        public override bool Equals(object obj) {
            return base.Equals(obj);
        }

        public override int GetHashCode() {
            return this.Guid.GetHashCode();
        }

        public override string ToString() {
            return this.Guid.ToString();
        }

        public static bool operator ==(GUID lhs, GUID rhs) {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(GUID lhs, GUID rhs) {
            return !(lhs == rhs);
        }
    }
}
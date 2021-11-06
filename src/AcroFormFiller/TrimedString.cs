namespace AcroFormFiller
{
    public readonly struct TrimedString
    {
        private readonly string _value;
        public TrimedString(string value)
        {
            _value = value.TrimToNull();
        }

        public static bool operator == (TrimedString a, TrimedString b) => a._value == b._value;

        public static bool operator !=(TrimedString a, TrimedString b) => a._value != b._value;

        public static implicit operator string(TrimedString str) => str._value;
        //public static explicit operator TrimedString(string str) => new TrimedString(str);
        public static implicit operator TrimedString(string str) => new TrimedString(str);
    }
}
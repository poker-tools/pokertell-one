// Date: 5/7/2009

namespace Tools.GenericRanges
{
    using System;

    public class GenericRange<T> : IGenericRange<T>
        where T : IComparable
    {
        #region Constructors and Destructors

        public GenericRange(T minValue, T maxValue)
        {
            if (minValue.CompareTo(maxValue) > 0)
            {
                throw new ArgumentException("MinValue cannot be greater than MaxValue");
            }

            MinValue = minValue;
            MaxValue = maxValue;
        }

        #endregion

        #region Properties

        public T MaxValue { get; protected set; }

        public T MinValue { get; protected set; }

        #endregion

        #region Implemented Interfaces

        #region IComparable

        int IComparable.CompareTo(object obj)
        {
            return MinValue.CompareTo(((GenericRange<T>)obj).MinValue);
        }

        #endregion

        #region IGenericRange<T>

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
           
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            
            return obj.GetType() == GetType() && Equals((GenericRange<T>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (MaxValue.GetHashCode() * 397) ^ MinValue.GetHashCode();
            }
        }

        public bool IncludesValue(IComparable value)
        {
            bool isGreaterOrEqualToMin = value.CompareTo(MinValue) >= 0;
            bool isSmallerOrEqualToMax = value.CompareTo(MaxValue) <= 0;

            return isGreaterOrEqualToMin && isSmallerOrEqualToMax;
        }

        public bool Equals(GenericRange<T> other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }
            
            return Equals(other.MaxValue, MaxValue) && Equals(other.MinValue, MinValue);
        }

        public override string ToString()
        {
            return string.Format("MinValue={0}, MaxValue={1}", MinValue, MaxValue);
        }

        #endregion

        #endregion
    }
}
﻿namespace Tools.Extensions
{
    using System;
    using System.Diagnostics;

    public static class ArrayComparer
    {
        #region Public Methods

        public static bool EqualsArray(this Array array1, Array array2)
        {
            return ArraysAreEqual(array1, array2, false);
        }

        public static bool EqualsArray(this Array array1, Array array2, bool showMessages)
        {
            return ArraysAreEqual(array1, array2, showMessages);
        }

        #endregion

        #region Methods

        static bool ArraysAreEqual(Array array1, Array array2, bool showMessages)
        {
            
            if (array1 == null && array2 == null)
            {
                return true;
            }

            if ((array1 == null) || (array2 == null))
            {
                if (showMessages)
                {
                    Debug.WriteLine(
                        string.Format(
                            "Array1 is null is: <{0}>, Array2 is null is: <{1}>", array1 == null, array2 == null));
                }

                return false;
            }

            if (ReferenceEquals(array1, array2))
            {
                return true;
            }

            if (array1.Length != array2.Length)
            {
                if (showMessages)
                {
                    Debug.WriteLine(
                        string.Format("Length1: <{0}> not equal Length2: <{1}>", array1.Length, array2.Length));
                }

                return false;
            }

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1.GetValue(i).ToStringNullSafe() != array2.GetValue(i).ToStringNullSafe())
                {
                    if (showMessages)
                    {
                        Debug.WriteLine(
                            string.Format(
                                "<{0}> not equal\n<{1}>", 
                                array1.GetValue(i).ToStringNullSafe(), 
                                array2.GetValue(i).ToStringNullSafe()));
                    }

                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
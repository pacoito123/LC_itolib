using System;
using UnityEngine.InputSystem.Utilities;

namespace itolib.Extensions
{
    /// <summary>
    ///     Extensions for some <c>InputUtils</c> stuff.
    /// </summary>
    internal static class InputActionExtensions
    {
        public static void InsertAtWithCapacity<T>(ref this CallbackArray<T> array, T listener, int index, int capacityIncrement = 10) where T : Delegate
        {
            array.m_Callbacks.InsertAtWithCapacity(listener, index, capacityIncrement);
        }

        public static void InsertAtWithCapacity<T>(ref this InlinedArray<T> array, T listener, int index, int capacityIncrement = 10) where T : Delegate
        {
            index = Math.Clamp(index, 0, array.length);

            if (index == array.length)
            {
                _ = array.AppendWithCapacity(listener, capacityIncrement);

                return;
            }

            int count = array.length - 1;

            if (index == 0)
            {
                if (array.additionalValues == null)
                {
                    array.additionalValues = new T[capacityIncrement];
                    array.additionalValues[0] = array.firstValue;
                }
                else
                {
                    ArrayHelpers.InsertAtWithCapacity(ref array.additionalValues, ref count, 0, array.firstValue, capacityIncrement);
                }

                array.firstValue = listener;
            }
            else
            {
                ArrayHelpers.InsertAtWithCapacity(ref array.additionalValues, ref count, index - 1, listener, capacityIncrement);
            }

            array.length++;
        }
    }
}
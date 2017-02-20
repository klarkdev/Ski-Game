using UnityEngine;
using System.Collections;

//public enum EMyEnum
//{
//    Cow = 0x01,
//    Chicken = 0x02,
//    Cat = 0x04,
//    Dog = 0x08,
//}

//public class Example : MonoBehaviour
//{
//    [BitMask(typeof(EMyEnum))]
//    public EMyEnum someMask;
//}

//Setting flags:
//MyEnumFlags currentFlags = Value1 | Value3 | Value5;

// Comparing flags:
// We are using the & operator to compare if the flag is on. It will
// return FALSE if the value is == to 00000000.
// currentFlags (00010111) & Value3 (00000100) = TRUE (00000100)
// currentFlags (00010111) & Value4 (00001000) = FALSE (00000000)
// So the following statement evaluates to TRUE because we do indeed
// have Value3 set in our currentFlags list.
// bool Value3_IsOn = System.Convert.ToBoolean(currentFlags & MyEnumFlags.Value3);

//Removing flags:
//1) (00010111) &= ~(00000100)
//2) (00010111) &= (11111011)
//3) (00010011)
// currentFlags &= ~MyEnumFlags.Value3;

public class BitMaskAttribute : PropertyAttribute
{
    public System.Type propType;
    public BitMaskAttribute(System.Type aType)
    {
        propType = aType;
    }
}
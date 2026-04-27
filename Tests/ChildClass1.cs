using System;
using UnityEngine;

namespace ESF.Core.PField.Tests
{
    [Serializable]
    public class ChildClass1 : BaseClass
    {
        [Header("Child Class 1")]
        [SerializeField] private string ChildStringVar;
        [SerializeField] private float ChildFloatVar;
    }
}
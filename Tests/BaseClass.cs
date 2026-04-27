using System;
using UnityEngine;

namespace ESF.Core.PField.Tests
{
    [Serializable]
    public class BaseClass
    {
        [Header("Base Class")]
        public int IntVar;
        public string StringVar;
        public Sprite SpriteVar;
        public bool BoolVar;
    }
}
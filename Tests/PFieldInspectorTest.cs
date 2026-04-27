using System;
using System.Collections.Generic;
using UnityEngine;

namespace ESF.Core.PField.Tests
{
    // [CreateAssetMenu(menuName = "PField Inspector Test", fileName = "PField Inspector Test")]
    public class PFieldInspectorTest : ScriptableObject
    {
        public List<BaseClass> list1;
        public PList<BaseClass> plist1;

        public List<ListBaseClass> list2;
        public PList<BaseClass> plist2;
        public List<PListBaseClass> list3;
        public PList<ListBaseClass> plist3;

        [SerializeReference, PField] public BaseClass var1;
        public BaseClass var2;
        public PListBaseClass var3;
        [SerializeReference, PField] public PListBaseClass var4;
        public ListPListBaseClass var5;
        public PListPListBaseClass var6;
        [SerializeReference, PField] public PListPListBaseClass var7;
    }

    [Serializable]
    public class PListBaseClass
    {
        public PList<BaseClass> plist = new PList<BaseClass>();
    }

    [Serializable]
    public class ListBaseClass
    {
        public List<BaseClass> list = new List<BaseClass>();
    }

    [Serializable]
    public class ListPListBaseClass
    {
        public List<PListBaseClass> list = new List<PListBaseClass>();
    }

    [Serializable]
    public class ListListBaseClass
    {
        public List<ListBaseClass> list = new List<ListBaseClass>();
    }

    [Serializable]
    public class PListPListBaseClass
    {
        public PList<PListBaseClass> plist = new PList<PListBaseClass>();
    }

    [Serializable]
    public class PFieldContainer
    {
        [SerializeReference, PField] public BaseClass var;
        [SerializeReference] public BaseClass var2;
    }
}
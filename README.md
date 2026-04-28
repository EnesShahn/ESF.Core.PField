# PField - Unity Polymorphic Field Editor

In Unity 2019.3 the [SerializeReference](https://docs.unity3d.com/ScriptReference/SerializeReference.html) attribute was added, it enabled developers to make fields serialized by reference.

However, Unity's default property drawer doesn't give us the means to specify which type to serialize the field to.

There are many third-party tools that already had this functionallity. However, all had limitations:
- Editor performance issues.
- Duplicated or new added items referenced previous list items, which meant that there exists 2 or more references to the same object in the list, this was a serious issue for me.
- No list nesting support.

To overcome these limitation I've taken a different approach while writing this tool.

Minimum supported Unity version is 2019.3 (Not tested) but I recommended Unity 2021 (Tested) LTS due to [this](https://blog.unity.com/engine-platform/serializereference-improvements-in-unity-2021-lts)

## Features
 - PList nesting support
 - Proper item duplication support on PList (values are duplicated)
 - Better performance compared to other third-party tools
 - Copy-Paste support on single value fields (Collection item copy-paste not supported yet)

#### Fields:
![PField](https://user-images.githubusercontent.com/22725319/222987732-070d7236-71f6-4882-a9b5-847081cde2ee.gif)
#### Lists:
![PList](https://user-images.githubusercontent.com/22725319/222987740-3651b8f5-e877-43a2-bad9-c46f5bbc16be.gif)
#### Nested Lists:
![PListNested](https://user-images.githubusercontent.com/22725319/222987742-93e12a0b-f209-4a7f-be20-e1cdda0e19ba.gif)

## Installation
Download .unitypackage from Releases section.

## Usage
 - Include `EnesShahn.PField` namespace
 - For collection field:
 ```
 public class Example
 {
     public PList<BaseClass> list;
 }
 ```
 **Basically instead of using List\<BaseClass\>, use PList\<BaseClass\>**

 - For Single value field:
 ```
 public class Example
 {
     [SerializeReference, PField] public BaseClass var;
 }
 ```
 
 ## Notes
- Classes should be Serializable
- Classes must not derive from UnityEngine.Object
- Abstract classes are not included in the context menu
- When renaming or moving a class to a different namespace, make sure to use MovedFrom attribute. 
Please refer to [this thread](https://forum.unity.com/threads/serializereference-data-loss-when-class-name-is-changed.736874/)


## TODOs
- [ ] Add to UPM registery
- [ ] Copy-Paste context menu for list items

Making a wrapper class around List<> instead of using an attribute has some neglible performance cost, but it was necessary since:
 1. When adding an attribute to an enumerable (e.g. Lists, Arrays), Unity applies the attribute on each list item individually.
 2. We are not able to override the default PropertyDrawer for List<> in a clean way. 

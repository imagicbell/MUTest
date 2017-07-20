# MUTest

A simple test framework for Unity.

### Features

*  Support implementing a Test Unit, see [TestExample](UnityProject/Assets/MUTest/Example/TestExample.cs);
*  Support multiple Test Cases in a Test Unit;
*  Support a self-defined GUI window for a Test Unit;
*  Output Test Log to xml files;
*  Offer an automatic random button click test unit;



### How to Use

* Call `TestRunner.Create()` where you want initialization, or attach the component `TestRunner.cs`to a gameObject(*suggest a new gameObject in the first scene*). 
* Press `F5` in Unity Editor, or touch with 5 fingers on iOS/Android devices to show the test window.
* To try the button click test offered, mark buttons in UI prefabs you made by attaching component `TestUIButtonClick.cs`to UI Buttons. There is a fast way to do this: select all UI prefabs, right click, "MUTestTools/Batch Mark Button".



### Dev Plan

* Offer more test units.
* Offer a editable UI test with GUI.

### Version

v1.0.0

### References

[Unity UI Test Automation Framework](https://github.com/taphos/unity-uitest)
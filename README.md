# Convert Revit detail elements to Detail Component with one click
You can draw detail elements in model view and change it to Component. Add-in takes selected elements, creates detail family and place it back in model view.

![LazyDetailComponentUI](https://user-images.githubusercontent.com/72662709/173247776-9e880693-62f6-4b01-9acc-6d6dbf81a3b7.png)

**Lazy/Fast Workflow**
- select detail elements in view
- run Lazy Detail Component Command
- hit Create

**Complex Workflow**

- select detail elements or just start Add-in
- type detail name (default name consists [Prefix]_[UserName]_[Date]_[Time])
- use "+" button to add more elements to the selection and after finished hit "Finish" Button
- use "+" button to choose detail base point (default Center)
- you can decide if you want copy or not line styles and if selected elements will be deleted after detail item been created
- hit Create

![LazyAddinTab](https://user-images.githubusercontent.com/72662709/173247809-c9d550d8-333a-47e3-bb15-a08d2fdb5a7e.png)

**First run**

When running first time select detail family template in the Add-in interface. Add-in remember your selection for future use. For detail component you need plane-based template eg Metric Detail Item.rft

**Features**

- View Types - works in all model views where it is possible to draw detail lines and filled regions (plans, sections, elevations, drafting views, legends, detail views) 
- Name - by default name is filled automatically with template name: [Prefix]_[UserName]_[Date]_[Time]. You can choose from drop down list one of name existing in model to speed up typing or to follow your naming convention. When typing, name "Create" button is disabled if name already been used in model. To change default Prefix - type text and use option at the end of drop down list (hidden pro feature).
- Selection - elements can be selected before add-in was run. In Add-in interface use "+" button to add or remove elements from selection. After selection process finished hit "Finish" Button to return to interface.
- Base Point - select specific Detail Component base point. Use "+" button to choose any specific in view or stay with default (Center). When selecting point hit Esc on keyboard to return to default Center.
- Copy line styles - you can choose if line styles will be copied to detail family. Copied line styles can be managed in Manage > Object Styles > Model Object > Detail Items. If copy option is not selected all lines in detail family will be set as default (Detail Items)
- Delate objects - set Add-in behaviour whether elements in model view will be deleted after Detail Component creation
- Template File - select detail family template used as an base template for Detail Components creation. For this purpose choose plain-based detail item eg Metric Detail Item.rft This can be found in Revit Library as common in folder C:\ProgramData\Autodesk\RVT... The user selection will be saved to config file.
- Configuration - all setups made in the add-in interface are stored in config file, so next time you run add-in all properties looks same. Config file is stored in add-in location. Make sure add-in folder have read/write access allowed.

## Tested Revit Versions

Works on Revit 2018, 2020, 2022

## Installation

Manually copy "Lazy.addin" & "LazyAddins.dll" from [>Install Folder<](https://github.com/PitPaf/LazyDetailComponent/tree/master/Install) to add-in folder on your PC eg "C:\Users\ [ your user name ] \AppData\Roaming\Autodesk\Revit\Addins\ [ Revit version ]".

**Issues**

Make sure folder you placed add-in is read/write allowed. If it is write protected this blocks add-in to save config file and may cause problem. By default config file is saved as LazyAddins.dll.config
If you meet any issue contact your admin to change file access setup. Config file can be deleted to reset default starting configuration.

## My Links

- http://www.zurawarchitekt.pl
- https://www.linkedin.com/in/zurawarchitekt/



# Overview #
The **Filter** component provides a set of classes to cater to filtering folders and files. They can be used to filter default .syncless configuration files as well as archive files.

# Classes #

![![](http://big5sync.googlecode.com/files/Filter%20-%20v2%20-%20thumb.png)](http://big5sync.googlecode.com/files/Filter%20-%20v2.png)

# Description of Design #
Filter component is designed based on the factory pattern. `FilterFactory` is used to create all the `Filter` objects. This allows the developers to create different types of `Filter` objects easily without know what is the underlying implementations.

# Extending Filter #
Extending Filter can be easily done. It can be done by creating a custom `Filter` class that extends `Filter` class. The developer only needs to implement the `Match` method. If the developer only wants to apply filter to files, he can extend the `FileFilter` object instead. It will ensure that folders will pass through. The developer can then edit the `FilterFactory` to create more types of filter. However, if the developer does not want to change the implementation of the **System Core** component, he can choose to extend the `FilterFactory` instead.

# Miscellaneous #
Ideally, filtering should only be applied to the physical address (i.e. C:\Lecture\) and not the logical address (address stored in the **Tagging** component).
However, some filters can be applied to logical address.
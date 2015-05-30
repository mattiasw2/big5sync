

# Overview #

The User Interface (UI) component is in charge of handling all interactions between the user and the control layer. It is implemented using the Windows Presentation Foundation (WPF) framework which allows for the creation of a clean and professional look-and-feel. It also allows for the true separation of the UI from the `SystemLogicLayer`, allowing other developers to create their own UI to interact with our Logic Layer.

# Classes #

The UI component consists of several important sub-components, including the single instance components - `SingleInstanceManager` , `SynclessApplication`, windows - `MainWindow`, `TagWindow`, `UntagWindow`, `TagDetailsWindow`, `ShortcutsWindow`, `LogWindow`, `WelcomeScreenWindow`, `PreviewSyncWindow`, `DialogWindow`, `CreateTagWindow`, `OptionsWindow`, helper components - `CommandLineHelper`, `RegistryHelper`, `FileFolderHelper`, `DialogType`, `DialogHelper`, resource dictionary - `AppResourceDictionary`, notification components - `NotificationWatcher`, `PriorityNotificationWatcher`, `SyncProgressWatcher`, preview sync components - `PreviewVisitor`, `SyncUIHelper` , misc components - `PortableSettingsProvider`.

# Third-Party Components #
In order to enhance the usability of Syncless, third-party components have been used.

`PortableSettingsProvider.cs` - Save application settings to the application folder, user settings become portable.

`Hardcodet.Wpf.TaskbarNotification.dll` - WPFTrayNotification Icon

`Ookii.Dialogs.Wpf.dll` - Folder Browser Dialog for Windows Vista and Above

`System.Windows.Controls.Input.Toolkit.dll` - AutoCompleteBox in `TagWindow`

`WPFToolkit.dll` - Datagrid in `LogWindow` and `PreviewSyncWindow`

# Description of Design #

## Single Instance Components ##
In order to support many of the other fundamental features which Syncless has such as shell integration and possible future CLI support, the UI had to convert to a single instance application which support arguments. App.xaml had to be removed and replaced with the combination of `SingleInstanceManager` and `SynclessApplication`.

## Separation of View from Control & Logic ##

The whole user interface architecture is based on the Service Locator pattern. Using this pattern, we can hide the implementation from the UI. The UI will retrieve from `Syncless.Core.ServiceLocator` the `Syncless.Core.IUIControllerInterface` Interface, which abstracts a subset of methods which the `SystemLogicLayer` will provide to the user interface.

The UI can then make use of these methods to interact with the logic layer. This decouples the UI from the `SystemLogicLayer`, preventing changes in the `SystemLogicLayer` from affecting the UI.

## Application Resource Dictionary ##
The UI makes use of the `AppResourceDictionary` to hold templates of controls and other resources such as `BrushStyles` or `ColorResources`, so as to allow for their reusability in other controls.

## Application Settings ##
The user interface makes use of session settings to keep track of settings in a session of Syncless running and and properties in C# for persistance of resources such as application settings. However, since Syncless is intended to be portable, application settings need to be stored along with Syncless instead of being stored to the User Settings locale. Thus we have adapted the `PortableSettingsProvider` for use as a `CustomSettingsProvider`.

## Shell Integration ##
The UI makes use of the `RegistryHelper` to write registry entries to enable shell integration. It also uses the `CommandLineHelper` to handle command line arguments, which any action from the shell uses.

## Notification ##
The UI has made use of the `NotificationQueue` and `PriorityNotificationQueue` provider by the `Syncless.Core.ServiceLocator` and the observer pattern for `SyncProgressWatcher` to interact with the [Syncless.Notification](http://code.google.com/p/big5sync/wiki/DeveloperComponentNotification) component.

The main notification components that are of concern are the `NotificationWatcher`, `PriorityNotificationWatcher`, `SyncProgressWatcher`.

  * `NotificationWatcher` - to dequeue any notification in the `UINotificationQueue`
  * `PriorityNotificationWatcher` - to dequeue any notification in the `UIPriorityQueue`
  * `SyncProgressWatcher` - to watch a particular !`SyncProgress` as an observer for any changes to the !`SyncProgress`

### NotificationWatcher & PriorityNotificationWatcher ###
`NotificationWatcher` and `PriorityNotificationWatcher` both run threads to dequeue any notification. After handling the notification, they will notify the `MainWindow`to refresh itself. Refer to the sample code in `NotificationWatcher`

```
// Handles Sync Start Notification
if (notification.NotificationCode.Equals(NotificationCode.SyncStartNotification))
{
    SyncStartNotification ssNotification = notification as SyncStartNotification;
    if (ssNotification != null)
    {
        _main.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
        {
            _main.CurrentProgress = ssNotification.Progress;
            new SyncProgressWatcher(_main, ssNotification.TagName, ssNotification.Progress);
        }));
    }
}
```

### SyncProgressWatcher ###

SyncProgressWatcher is initialized when a `NotificationCode.SyncStartNotification` is received. It then adds itself as a watcher to the SyncProgress and notifies the `MainWindow` to refresh itself whenever the progress is changed. Refer to the sample code below in `SyncProgressWatcher`

```
private void SyncStart()
{
    _main.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
    {
        _main.ProgressNotifySyncStart(Progress);
        StateChanged();
    }));
}
```

## Preview Synchronization ##
The Preview Synchronization makes use of the visitor pattern which has been used in the [Syncless.CompareAndSync](http://code.google.com/p/big5sync/wiki/DeveloperComponentCompareAndSync) component.

The algorithm in `PreviewVisitor`, which is similar to that of the `SyncerVisitor` in `Syncless.CompareAndSync` is used to populate the datagrid in `PreviewSyncWindow`.

# WPF Features In Play #
The user interface has made use of features such as data binding, customization of controls, animation and commands in WPF. The following explains animation and commands.

## Animation ##
All windows makes use of animation through storyboarding and triggers. All animations are stored in the `.xaml` class of each window. To edit the animation or trigger itself, please use Microsoft Express Blend 3.0 - http://www.microsoft.com/expression/products/Blend_Overview.aspx

In order to start storyboards manually, please refer to the sample code below. It will locate for the Storyboard `MainWindowOnLoaded` in the window's resources and then begin loading.

```
private void DisplayLoadingAnimation()
{
    ..
    Storyboard loading = (Storyboard)Resources["MainWindowOnLoaded"];
    loading.Begin();
    ..
}
```

## Commands ##
Almost all keyboard shortcuts and toolbar functionality in `MainWindow` have been initialized through commands.

Refer to the following sample code below:

```
private void InitializeTimeSyncCommand()
{
    RoutedCommand TimeSyncCommand = new RoutedCommand();

    CommandBinding cb = new CommandBinding(TimeSyncCommand, TimeSyncCommandExecute, TimeSyncCommandCanExecute);
    CommandBindings.Add(cb);

    BtnTimeSync.Command = TimeSyncCommand;

    KeyGesture kg = new KeyGesture(Key.Y, ModifierKeys.Control);
    InputBinding ib = new InputBinding(TimeSyncCommand, kg);
    InputBindings.Add(ib);
}

private void TimeSyncCommandExecute(object sender, ExecutedRoutedEventArgs e)
{
    e.Handled = true;
    InitiateTimeSync();
}

private void TimeSyncCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
{
    e.CanExecute = true;
    e.Handled = true;
}
```

`TimeSyncCommandExecute` contains the actual code to execute the actual TimeSync functionality.
`TimeSyncCommandCanExecute` contains the code to ensure that the command can be executed.
`TimeSyncCommand` is the actual command which will be assign to components like `BtnTimeSync`
`CommandBinding` binds all the above 3 together.

# UI Design Principles & Philosophy #

Our overall goal is to make the whole tagging and synchronization process as simple and seamless as ever. This can be remembered with the following acronym, SIMBA.

  1. Seamless - Users should forget that Syncless is synchronizing their files in the background in seamless mode.
  1. Intuitive - Users should find Syncless' functionality intuitive.
  1. Minimalist - Aims to prevent cognitive overload on users.
  1. Best of Class - Aims to adopt the best UI practices from application and web development.
  1. Accessible - Users can use Syncless with their workflow preference.

In addition, the user interface has been made as pleasing as possible, with the appropriate use of colours, controls, meaningful icons and with the consideration of user interface guidelines always in mind.

# Developing an Alternative User Interface for Syncless #

To create a UI for Syncless, the developer just need to follow a few steps.
  1. Implement `Syncless.Core.IUInterface`. This is to provide the System Core with a way to update the interface as and when necessary.
  1. Use `Syncless.Core.ServiceLocator` to get the `IUIControllerInterface`. This provide the list of methods which the UI developer can use to call from the UI.
  1. Call the Initiate method in the `IUIControllerInterface` to initiate the `SystemLogicLayer`.
  1. Refer to the Developer API: `Syncless.Core.IUIControllerInterface` for all the methods which will be used.

# Credits #
We would like to thank the following resources/developers for their contributions/controls to challenges that we have encountered in the course of developing the UI.

  * **Singleton Application** - http://www.switchonthecode.com/tutorials/wpf-writing-a-single-instance-application
  * **Combined File & Folder Browser Dialog** - http://dotnetzip.codeplex.com/SourceControl/changeset/view/29832#432677
  * **WPF ToolKit** - http://www.codeplex.com/wpf (For its DataGrid and AutoCompleteBox, which the current WPF release is lacking)
  * **Portable Settings Provider** - http://www.codeproject.com/KB/vb/CustomSettingsProvider.aspx (C# version by gpgemini)
  * **Vista-Style Folder Browser Dialog** - http://www.ookii.org/software/dialogs/
  * **WPF NotifyIcon** - http://www.codeproject.com/KB/WPF/wpf_notifyicon.aspx
# VaultSync README

## How to install
The installer installs or upgrades VaultSync in a portable format.  It will install anywhere, but 
the best place to install it is on the same removable drive that the Vault is stored on. 
This ensures that file extraction is always possible if the Vault is required for recovery. If VaultSync has been previously installed then it runs an upgrade to the current version.

Run the installer file **Setup.exe** and select a removable drive or network drive to install on.
The installer selects the first removable drive as the default installation location. 
If there isn't a removable drive then *C:\VaultSync* is selected by default. 
It is highly recommnded to change this to some other location, preferably somewhere that is not 
on the same hardware as the files that will be put into the Vault.

When the install location is satifactory then press the *Install* button to complete the installation. 

Run **VaultSync** to create a new vault in the same location as the installation. 
Add files and folders to the synchronize list and then run the sync.

## Release Notes
### Version 1.13

* Added image preview
### Version 1.12

* Added a 'Move Computer' context menu to move items in the vault to a new computer
### Version 1.11

* Added an 'Auto Synchronise' button to toggle synchronisation when the vault opens
### Version 1.10

* Changed the technique for checking for an update
* Automatically check for an update after unlocking the vault

### Version 1.9

* Fixed a bug in the update handler
* Always create the vault if it doesn't exist, instead of asking

### Version 1.8

* Clear the password field if an incorrect password is entered
* Prevent duplicate processes from running that will cause data corruption

### Version 1.7

* Restore an extracted file's create and modified date/time correctly
* Handle internal database upgrade more robustly

### Version 1.6

* Improve performance of sync and delete
* Show progress dialog during its setup

### Version 1.5

* Updated installation to move files to a sub-directory

### Version 1.4

* Fixed toolbar enables after multi-select with shift
* Clear the selection after a delete
* Added database vacuum to shrink the index at close

### Version 1.3

* Added a tool bar option to check for a software update
 
### Version 1.2

* Continually check available space on the destination drive and cancel the sync if space gets too 
   low to safely store the updated index.
* Various bug fixes
* Created new installer

### Version 1.1

* Fixed an exception that occured during sync when a file was removed from the source location.


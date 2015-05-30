

# Test Cases #

## Manual Sync (File) ##

| **No.** | **Summary** | **Steps** | **Expected Output** |
|:--------|:------------|:----------|:--------------------|
| 1.      | Create a file | 1) Create a file<br>2) Sync over <table><thead><th> New file created through all the folders </th></thead><tbody>
<tr><td> 2.      </td><td> Update a file </td><td> 1) Update a file<br>2) Sync over </td><td> File is updated through all the folders </td></tr>
<tr><td> 3.      </td><td> Rename a file </td><td> 1) Rename a file<br>2) Sync over </td><td> File is renamed through all the folders </td></tr>
<tr><td> 4.      </td><td> Delete a file </td><td> 1) Delete a file<br>2) Sync over </td><td> Deletion is propagated through all folders </td></tr>
<tr><td> 5.      </td><td> Renaming a file with 2 different names </td><td> 1) Rename a file at 1 folder<br>2) Rename a file at another folder<br>3) Sync it </td><td> At least 2 new files created </td></tr>
<tr><td> 6.      </td><td> Deleting a file , update from another tagged folder </td><td> 1) Delete a file<br>2) Update the same file from another folder<br>3) Sync it </td><td> All folders should reflect the updated file </td></tr>
<tr><td> 7.      </td><td> Rename a file and update the contents </td><td> 1) Rename a file<br>2) Update the contents<br>3) Sync it </td><td> Every tagged folder should have 2 files in this case </td></tr>
<tr><td> 8.      </td><td> Delete the file and create a new one with the same name </td><td> 1) Delete a file<br>2) Sync<br>3) Create a new file similar to (1)<br>4) Sync </td><td> All folders will show the same file again </td></tr>
<tr><td> 9.      </td><td> Rename a file from a folder , delete the old file </td><td> 1) Rename a file from a folder<br>2) Delete the old file from the folder<br>3) Sync </td><td> All folders should only show the renamed file </td></tr>
<tr><td> 10.     </td><td> Create a file in all folders </td><td> 1) Create a new file in all folder<br>2) Sync over </td><td> No change on each folder. But XML should be updated </td></tr>
<tr><td> 11.     </td><td> Rename a file from 2 folder </td><td> 1) Rename a file from 2 folders<br>2) Sync over </td><td> The file that is non-renamed will have syncless archive </td></tr>
<tr><td> 12.     </td><td> Delete the same file in every folder </td><td> 1) Delete the file in each folder<br>2) Sync them </td><td> XML will be updated </td></tr>
<tr><td> 13.     </td><td> Untag a folder and tag it again </td><td> 1) Untag a folder<br>2) Delete the files in it<br>3) Tag the folder removed in (1)<br>4) Sync again </td><td> The file will be repropagated again </td></tr>
<tr><td> 14.     </td><td> Rename a file from all folders </td><td> 1) Rename the files in all folder<br>2) Sync now </td><td> XML will be updated </td></tr>
<tr><td> 15.     </td><td> Update the file from all folders </td><td> 1) Update the file in all folders<br>2) Sync now </td><td> XML will be updated </td></tr>
<tr><td> 16.     </td><td> Rename each file to a different name </td><td> 1) Create a file and sync over<br>2) Rename the file in each directory<br>3) Sync over </td><td> XML should reflect all changes + all folder will reflect all number of file name </td></tr>
<tr><td> 17.     </td><td> Multiple Sync now </td><td> 1)  For the first tag , sync a large file<br>2) Go to the other multiple tag and sync now </td><td> Everything should be sync in order </td></tr>
<tr><td> 18.     </td><td> Sync with a removable storage and plug it out while syncing </td><td> 1) Create a tag with some folders from local directory and thumbdrive<br>2) While synchronizing , plug it out </td><td> The program should not hang , while the other folders should not be affected </td></tr></tbody></table>

<a href='#Test_Cases.md'>Back to top</a>

<h2>Manual Sync (Folder)</h2>

<table><thead><th> <b>No.</b> </th><th> <b>Summary</b> </th><th> <b>Steps</b> </th><th> <b>Expected Output</b> </th></thead><tbody>
<tr><td> 1.         </td><td> Create a folder </td><td> 1) Create a folder<br>2) Sync over </td><td> New folder is propagated to all folders </td></tr>
<tr><td> 2.         </td><td> Rename a folder </td><td> 1) Rename a folder<br>2) Sync over </td><td> Renamed folder is propagated to all folders </td></tr>
<tr><td> 3.         </td><td> Delete a folder </td><td> 1) Delete a folder<br>2) Sync over </td><td> Deletion of folder is propagated to all folders </td></tr>
<tr><td> 4.         </td><td> Rename a folder , and delete the folder from another folder </td><td> 1) Rename a folder , delete a folder from another folder<br>2) Sync over </td><td> Renamed folder will propagate over </td></tr>
<tr><td> 5.         </td><td> Delete a folder , create a subfolder </td><td> 1) Delete 1 of the folder<br>2) Create a sub folder in another folder<br>3) Sync over </td><td> Deletion will propagate through all folders </td></tr>
<tr><td> 6.         </td><td> Rename 2 levels of folder </td><td> 1) Rename a folder<br>2) Rename the subfolder </td><td> Rename will propagate through all levels </td></tr>
<tr><td> 7.         </td><td> Rename folder in 2 different folders </td><td> 1) Rename a folder in a folder<br>2) Rename it to another name in another folder<br>3) Sync now </td><td> All folders will reflect the different rename </td></tr>
<tr><td> 8.         </td><td> Delete a folder and create a new folder of the same name </td><td> 1) Delete the folder in all folders<br>2) Create a folder in either of the folder<br>3) Sync over </td><td> Will delete the folder </td></tr>
<tr><td> 9.         </td><td> Deletion of sub folder </td><td> 1) Delete the sub folder in either of the folder<br>2) Sync over </td><td> Will delete the sub folder </td></tr>
<tr><td> 10.        </td><td> Rename the folder and delete the sub-folder </td><td> 1) Rename the folder<br>2) Delete the sub-folder<br>3) Sync over </td><td> Will rename the folder and delete the sub-folder </td></tr>
<tr><td> 11.        </td><td> Rename a different folder at different level </td><td> 1) Rename a folder at different<br>2) Sync them </td><td> Will merge all the renamed folders in every level </td></tr>
<tr><td> 12.        </td><td> Create a new folder in each folder </td><td> 1) Create a new folder in each folder<br>2) Sync them </td><td> XML should reflect all the created folders </td></tr>
<tr><td> 13.        </td><td> Rename all the folders </td><td> 1) Rename all the folder<br>2) Sync them </td><td> XML should reflect all the created folders </td></tr>
<tr><td> 14.        </td><td> Untag the folder and tag it again </td><td> 1) Create a folder in all and sync<br>2) Untag the folder , delete the folder in the rest<br>3) Retag the folder and sync </td><td> Will repropagate the deleted folder </td></tr>
<tr><td> 15.        </td><td> Renaming 2 of the folder </td><td> 1) Create a folder and sync over<br>2) Rename 2 of the folder </td><td> All folders will be renamed </td></tr></tbody></table>

<a href='#Test_Cases.md'>Back to top</a>

<h2>Manual Sync (File + Folder)</h2>

<table><thead><th> <b>No.</b> </th><th> <b>Summary</b> </th><th> <b>Steps</b> </th><th> <b>Expected Output</b> </th></thead><tbody>
<tr><td> 1.         </td><td> Create a file in a folder </td><td> 1) Create a file inside a folder<br>2) Sync over </td><td> All folders will show the file inside the folder </td></tr>
<tr><td> 2.         </td><td> Rename the file , rename the folder </td><td> 1) Rename the file<br>2) Rename the folder<br>3) Sync over </td><td> Folder will propagate the renamed folder and renamed file </td></tr>
<tr><td> 3.         </td><td> Update the file </td><td> 1) Update the file<br>2) Sync over </td><td> File will be updated in all folders </td></tr>
<tr><td> 4.         </td><td> Rename the folder , update the file </td><td> 1) Rename the folder<br>2) Update the file<br>3) Sync over </td><td> Files will be updated , folders will be renamed </td></tr>
<tr><td> 5.         </td><td> Rename the folder , delete the file </td><td> 1) Delete the file<br>2) Rename the folder<br>3) Sync over </td><td> File will be deleted and folder will be renamed </td></tr>
<tr><td> 6.         </td><td> Rename the folder , create a file </td><td> 1) Create a file<br>2) Rename the folder<br>3) Sync over </td><td> Folder will be renamed with the file created </td></tr>
<tr><td> 7.         </td><td> Delete the folder, create a file in another folder </td><td> 1) Delete 1 of the sub - folder<br>2) Create a file<br>3) Sync over </td><td> No folder will be deleted , with a file inside created for all folders </td></tr>
<tr><td> 8.         </td><td> Deletion and renaming of folder with a file inside </td><td> 1) Create a folder with a file inside and sync over<br>2) Delete 1 of the folder , while u rename the other folder<br>3) Sync over </td><td> Folder will be renamed with the file inside </td></tr>
<tr><td> 9.         </td><td> Delete a folder , update the file </td><td> 1) Delete a folder containing a file on 1 side<br>2) Update the contents of the file on another side<br>3) Sync over </td><td> No folders will be deleted , the updated file will be propagated over </td></tr>
<tr><td> 10.        </td><td> Rename the folder with 2 different names , rename the file with 2 different names </td><td> 1) Rename the folder with 2 different names<br>2) Rename the files with 2 different names<br>3) Sync over </td><td> Folder will show at least 2 names. Files will show 2 different names </td></tr>
<tr><td> 11.        </td><td> Create a folder with a file inside in all folders </td><td> 1) Create a folder and file in all different folders<br>2) Sync over </td><td> XML will be updated in all folders </td></tr>
<tr><td> 12.        </td><td> Sync files/ folders with large file size / large number of files </td><td> 1) Sync it over </td><td> All changes will be propagated </td></tr>
<tr><td> 13.        </td><td> Rename 2 folders + creating a new file at the non-renamed </td><td> 1) Create a folder and sync over<br>2) Rename 2 of the folder and add a new file in the non-renamed folder<br>3) Sync over </td><td> All folders will have the renamed folder and the new file in it </td></tr>
<tr><td> 14.        </td><td> Folder and name with the same path </td><td> 1) Name a folder and a file to file.txt<br>2) Sync now </td><td> A conflict folder will appear </td></tr></tbody></table>

<a href='#Test_Cases.md'>Back to top</a>

<h2>TODO</h2>

<table><thead><th> <b>No.</b> </th><th> <b>Summary</b> </th><th> <b>Steps</b> </th><th> <b>Expected Output</b> </th></thead><tbody>
<tr><td> 1.         </td><td> Delete a file  </td><td> 1) Delete a file<br>2) Sync over </td><td> ToDo.xml will be created with the file name inside </td></tr>
<tr><td> 2.         </td><td> Rename a file  </td><td> 1) Rename a file<br>2) Sync over </td><td> ToDo.xml will be created </td></tr>
<tr><td> 3.         </td><td> Update to thumb drive </td><td> 1) Create a file in folders and sync over<br>2) Remove the thumb drive and delete the file and sync<br>3) Plug in the thumb drive and sync again </td><td> The file in the thumb drive will be deleted or renamed again </td></tr>
<tr><td> 4.         </td><td> Delete a file and update the file in thumbdrive </td><td> 1) Create a file in folders and sync over<br>2) Remove the thumb drive and delete the file and sync<br>3) Update the file content in the thumbdrive<br>4) Plug the thumbdrive back and sync </td><td> The newly modified file in the thumbdrive will propagate to the rest </td></tr></tbody></table>

<a href='#Test_Cases.md'>Back to top</a>

<h2>Seamless</h2>

<table><thead><th> <b>No.</b> </th><th> <b>Summary</b> </th><th> <b>Steps</b> </th><th> <b>Expected Output</b> </th></thead><tbody>
<tr><td> 1.         </td><td> Create a file  </td><td> 1) Create a file </td><td> File will be synced over </td></tr>
<tr><td> 2.         </td><td> Delete a file  </td><td> 1) Delete a file </td><td> File will be deleted in all folders </td></tr>
<tr><td> 3.         </td><td> Rename a file  </td><td> 1) Rename a file </td><td> File rename will be propagated </td></tr>
<tr><td> 4.         </td><td> Update a file  </td><td> 1) Update a file </td><td> File update will be propagated </td></tr>
<tr><td> 5.         </td><td> Copy a folder with a file into any of the tagged folder </td><td> 1) Copy a folder with a file </td><td> Folder will be propagated with the file inside </td></tr>
<tr><td> 6.         </td><td> Propagating a locked file </td><td> 1) Open MS word<br>2) Tag 2 folder that is seamless </td><td> All files (if any) will sync over except for the locked file | File will not be propagated if there is no changes </td></tr>
<tr><td> 7.         </td><td> Propagating a file that has path that is too long </td><td> 1) Create a file with maximum file path<br>2) Tag 2 folder<br>3) Delete the file </td><td> The file with maximum file path will not be sync over to the other folder. If the file is renamed to acceptable filepath , it should sync over automatically </td></tr>
<tr><td> 8.         </td><td> ToDo in Seamless </td><td> 1) Tag 3 folders , with 1 being a thumbdrive<br>2) Create a file in one of the folder<br>3) Safety remove the removable storage media<br>4) Delete the file in one of the remainding folder. The deletion will propagate to the other folder<br>5) Update the thumbdrive from another computer<br>6) Insert the same thumbdrive back </td><td> All 3 tagged folders will have the new updated file </td></tr>
<tr><td> 9.         </td><td> Seamless delete propagation </td><td> 1) Create a folder<br>2) After the folder is propagated , delete that folder </td><td> Ensure no folder is being recreated after deletion </td></tr>
<tr><td> 10.        </td><td> Seamless copy and paste </td><td> 1) Create a tag , switch it to seamless<br>2) Tag 3 folders which are empty<br>3) Throw a folder with some files into either of the folder (try 2nd and 3rd folder) </td><td> Folders and files should be propagated over </td></tr>
<tr><td> 11.        </td><td> Seamless tagging of folders </td><td> 1) Create a tag (Set it to seamless) with 2 folders from a removable storage<br>2) Tag the 3rd folder from local directory<br>3) Put a file in the local directory and wait to sync over </td><td> Folder and files should be propagated over </td></tr>
<tr><td> 12.        </td><td> Seamless tagging with thumbdrive </td><td> 1) Create a tag with 2 local folders and 1 folder from thumbdrive<br>2) Plug out the thumbdrive and switch to seamless<br>3) While analyzing , plug in the thumbdrive </td><td> The folder in the thumbdrive should have the updated data as well </td></tr>
<tr><td> 13.        </td><td> Rename a folder </td><td> 1) Create a folder<br>2) Rename the folder </td><td> All the xml should reflect the changes </td></tr>
<tr><td> 14.        </td><td> Seamless synchronization and plug out thumbdrive </td><td> 1) Create a tag with some folders from local directory and thumb drive<br>2) While seamless is syncing , plug out the thumbdrive </td><td> The program should not hang while the other folders should not be affected </td></tr></tbody></table>

<a href='#Test_Cases.md'>Back to top</a>

<h2>UI</h2>

<table><thead><th> <b>No.</b> </th><th> <b>Summary</b> </th><th> <b>Steps</b> </th><th> <b>Expected Output</b> </th></thead><tbody>
<tr><td> 1.         </td><td> Switching of mode </td><td> 1) Create a tag with 2 folders<br>2) Untag the 2 folders<br>3) Switch between seamless/manual </td><td> Should not hang or display anything erroneous </td></tr>
<tr><td> 2.         </td><td> Testing of all shortcuts </td><td> Try all shortcuts </td><td> All should display their own respective winforms </td></tr>
<tr><td> 3.         </td><td> Closing Syncless with other components running </td><td> 1) Create any component<br>2) Close syncless through the tray </td><td> All components should be closed after Syncless closes </td></tr>
<tr><td> 4.         </td><td> Click on every single button on the menu of syncless </td><td> Try all buttons </td><td> Should display their own respective winforms </td></tr>
<tr><td> 5.         </td><td> Only 1 instance of winform component should be displayed at any time </td><td> 1) Click on any winform component at the menu of syncless<br>2) Click on syncless on the taskbar to bring syncless to the front<br>3) Click on any shortcut of the same winform component </td><td> Should only show 1 instance of the winform at any time </td></tr>
<tr><td> 6.         </td><td> Tagging and Untagging folders from windows explorer </td><td> 1) Do the different steps respectively </td><td> Should produce the respective output </td></tr>
<tr><td> 7.         </td><td> Tagging a drive </td><td> 1) Go to windows explorer and tag a drive </td><td> Should prompt you for confirmation </td></tr>
<tr><td> 8.         </td><td> Syncing a locked file </td><td> 1) Sync a locked (MS word) file over<br>2) Locked file is not synced over </td><td> All other UI component should be working normally </td></tr>
<tr><td> 9.         </td><td> Closing Syncless while syncing </td><td> 1) Sync a large folder<br>2) Quit Syncless </td><td> Should show the termination bar </td></tr>
<tr><td> 10.        </td><td> Closing Syncless with many components open </td><td> 1) Open log<br>2) Click on tag at tray<br>3) Click on exit at tray </td><td> Should terminate syncless properly </td></tr>
<tr><td> 11.        </td><td> Untagging with no tags </td><td> 1) Create a tag<br>2) Tag some folders<br>3) Untag all folders<br>4) Try Untagging again </td><td> Nothing should happen  </td></tr>
<tr><td> 12.        </td><td> Spamming Cancel and Sync Now button </td><td> 1) Spam the Cancel and Sync Now button </td><td> Cancel button should disappear only at synchronization stage </td></tr>
<tr><td> 13.        </td><td> Sync now and Cancel </td><td> 1) Sync now with a big folder<br>2) Go to another tag and sync now too<br>3) While analyzing the large file , cancel the analyze. Preview button will show<br>4) Then switch mode </td><td> Other tag synchronization should continue </td></tr>
<tr><td> 14.        </td><td> Sync and untag from windows explorer </td><td> 1) Sync a few tags<br>2) Untag them from windows explorer after sync complete </td><td> Should not display any progress bar </td></tr></tbody></table>

<a href='#Test_Cases.md'>Back to top</a>

<h2>Filter</h2>

<table><thead><th> <b>No.</b> </th><th> <b>Summary</b> </th><th> <b>Steps</b> </th><th> <b>Expected Output</b> </th></thead><tbody>
<tr><td> 1.         </td><td> Empty filters  </td><td> 1) Create a filter<br>2) Set Ext. Mask to empty string<br>3) Click OK </td><td> Since it is an empty filter, it should allow the update of the file </td></tr>
<tr><td> 2.         </td><td> Same filters   </td><td> 1) Click on add filter<br>2) Click on add filter again </td><td> Should not allow you to set the filter </td></tr></tbody></table>

<a href='#Test_Cases.md'>Back to top</a>

<h2>TimeSync</h2>

<table><thead><th> <b>No.</b> </th><th> <b>Summary</b> </th><th> <b>Steps</b> </th><th> <b>Expected Output</b> </th></thead><tbody>
<tr><td> 1.         </td><td> Timesync       </td><td> 1) Change the time clock<br>2) Click on timesync </td><td> Time will be synchronised </td></tr></tbody></table>

<a href='#Test_Cases.md'>Back to top</a>

<h2>Removable Storage</h2>

<table><thead><th> <b>No.</b> </th><th> <b>Summary</b> </th><th> <b>Steps</b> </th><th> <b>Expected Output</b> </th></thead><tbody>
<tr><td> 1.         </td><td> Safely remove a removable storage </td><td> 1) Go to tray and safely remove thumbdrive </td><td> Removable drive should be removed successfully and updated on the tag window display </td></tr></tbody></table>

<a href='#Test_Cases.md'>Back to top</a>

<h2>Monitoring and Unmonitoring</h2>

<table><thead><th> <b>No.</b> </th><th> <b>Summary</b> </th><th> <b>Steps</b> </th><th> <b>Expected Output</b> </th></thead><tbody>
<tr><td> 1.         </td><td> Safely remove a removable storage </td><td> 1) Tag A , B  into Tag '1'<br>2) Tag B , C in Tag '2'<br>3) Switch both to seamless<br>4) Put a file in folder B<br>5) Ensure that the file in B goes to A and C<br>6) Put a file in C , ensure that it goes to A<br>7) Put a file in A , ensure that it goes to C<br>8) Switch Tag '2' to manual<br>9) Put a file in C , it should not go to A or B<br>10) Put a file in B , it should go to A<br>11) Put a file in A , it should go to B , not C<br>12) Switch Tag '1' to manual. Put a file in all A , B and C ... nothing will happen </td><td> NA                     </td></tr></tbody></table>

<a href='#Test_Cases.md'>Back to top</a>

<h2>Tag Merging</h2>

<table><thead><th> <b>No.</b> </th><th> <b>Steps</b> </th><th> <b>Expected Output</b> </th></thead><tbody>
<tr><td> 1.         </td><td> 1) Delete all the .syncless<br>2) Create tag ‘A’ in Computer 1. Tag folder A in computer 1, tag folder B in thumbdrive<br>3) Go to computer 2 </td><td> Tag A will appear, folder B will appear </td></tr>
<tr><td> 2.         </td><td> 1) Tag folder C in computer 2<br>2) Tag folder D in thumbdrive<br>3) Go to computer 1 </td><td> Folder C will be tagged but not shown. Folder D will be shown </td></tr>
<tr><td> 3.         </td><td> 1) Create tag B in computer 1<br>2) Tag folder E in computer 1 , tag folder F in thumb<br>3)Untag folder B from tag A<br>4) Go to computer 2 </td><td> Folder B will be untagged. Tag B will appear </td></tr>
<tr><td> 4.         </td><td> 1) Delete tag A<br>2) Go to computer A </td><td> Ensure Tag A is unmonitored and deleted </td></tr>
<tr><td> 5.         </td><td> 1) Create tag A in computer 1<br>2) Create tag A in computer 2 ( Thumbdrive is always in computer 1)<br>3) Tag something to A in thumbdrive<br>4) Plug the thumbdrive to computer 2 </td><td> When plugged into computer 2 , can see path </td></tr>
<tr><td> 6.         </td><td> 1) Create tag B in computer 1<br>2) Create tag B in computer 2<br>3) Delete tag B in computer 1<br>4) Plug into computer 2 </td><td> B will be deleted from computer 2 </td></tr>
<tr><td> 7.         </td><td> 1) Create tag C in computer 1<br>2) Delete tag C in computer 1<br>3) Create tag C in computer 2<br>4) Plug into computer 2 </td><td> C should not be deleted </td></tr></tbody></table>

<a href='#Test_Cases.md'>Back to top</a>
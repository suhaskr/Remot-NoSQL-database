Command line arguments:
-Writer_Count 2 -Reader_Count 3 -Reader_Partial_Display -Writer_Send_Message

Once TestExecutive starts running it launches DemoClient,Server and WPF GUI.
Once all operations(REad,rite,Persist) are done by DemoClient, it waits for User to enter a key.

Once you enter any key in TestExecutive window, it launches Writers and Readers based on the Writer count and Reader_Count.

All XML Files are saved in SavedXMLFiles folder.
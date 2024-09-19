<============================== CatImageAPI App ==============================>

Below are the directions in order to use the CatImageAPI App.
For further directions please contact the owner.

<============================= Database Section ==============================>
Run CAtImageAPI
Create an SQL Database.
Enable SQL server and Windows Authentication mode.
    Create a user using the Query (1)
    Create LOGIN userName
    WITH PASSWORD = 'userPassword'
    GO
Create Database using the Query (2)
    CREATE DATABASE databaseName
Create user for database with the Query (3)
    Create USER userName
    FOR LOGIN userName
    GO
Be advised that the userName is the userName used in the Query(1).
Navigate to your database > Security > Users
Right click on the user.
Select properties.
Navigate to Membership and grand them roles : db_datareader, db_datawriter, db_dll_admin.
Then open SQL 2022 server 2022 configuration.
Navigate to SQL Server Network Configuration > Protocols for myServerName.
Enable TCP/IP.
Right click on TCP/IP and select properties.
Navigate to IP Adresses > IPALL and set the TCP PORT (ex. 1433).

<=========================== Visual studio section ===========================>
Go to appsettings.json file.
Choose connection string depending if you want to run the project via terminal or docker.
For docker the your IP is the systems IP.
In order to retrieve it you should open a cmd and type ipconfig and retrieve the IP4 adress value.
Comment out and replace the values of the connection string you wish to use.
If you are going to use docker you need to initialize docker app.
Finally build and run the solution.

<=============================== Test Section ================================>
In order to use the created test you should download the project https://github.com/jimpapa7/CatImageAPI.test.
Create a folder.
Add to the folder the folder with the app and the folder with the test project.
Open the app solution.
Right click on the solution.
Navigate to Add > Existing project.
Rebuild solution.
Navigate to Test > Run All Tests

<============================== CatImageAPI App ==============================>


# TRec: A Regression Test Recommender

## Contents
This folder contains TRec's source code and console application organized as follows:

- The [**TRec**](/Tool/TRec/) folder contains the console application and all the necessory components needed to run the application.
  Please read the instructions below on using the console application. 


- The [**SourceCode**](/Tool/SourceCode/) folder contains TRec's source code. 
We provide the source code so that that tool can be extended. 
Please check ReadMe files in this folder and its subfolders for instructions on using TRec through the source code. 


## Using Console Application

### Prerequisites

#### .NET Framework
- Ensure that .NET Framework version 4.7.2 or higher is installed on your system.
- You can download and install the .NET Framework from the official Microsoft website: [Download .NET Framework](https://dotnet.microsoft.com/download/dotnet-framework)

#### Java
- Ensure that Java version 21.0.3 or higher is installed on your system.
- You can download and install the latest version of Java from the official Oracle website: [Download Java](https://www.oracle.com/java/technologies/javase-jdk17-downloads.html)

#### Checking Java Installation

To verify if Java is installed and to check its version, follow these steps:

1. Open Command Prompt (cmd).
2. Type the following command and press Enter:

   ```cmd
   java -version
   ```

3. If Java is installed correctly, you should see an output similar to this:

   ```
   java version "21.0.3" 2023-04-18 LTS
   Java(TM) SE Runtime Environment (build 21.0.3+8-LTS-211)
   Java HotSpot(TM) 64-Bit Server VM (build 21.0.3+8-LTS-211, mixed mode, sharing)
   ```

4. If you do not see the version information and instead get an error message, follow the steps below to install or configure Java.

#### Installing or Configuring Java

##### Step 1: Download Java

1. Go to the [Java SE Downloads](https://www.oracle.com/java/technologies/javase-jdk17-downloads.html) page.
2. Download the installer for your operating system.

##### Step 2: Install Java

1. Run the downloaded installer and follow the on-screen instructions to install Java.

##### Step 3: Set Up Java Environment Variables (Windows)

1. Open the Start menu, search for "Environment Variables," and select "Edit the system environment variables."
2. In the System Properties window, click on the "Environment Variables" button.
3. In the Environment Variables window, under System variables, click on "New" to add a new system variable.
  - Variable name: `JAVA_HOME`
  - Variable value: `C:\Program Files\Java\jdk-21.0.3` (Replace this path with the path where Java is installed on your system)
4. Find the `Path` variable in the System variables section, select it, and click on "Edit."
5. In the Edit Environment Variable window, click on "New" and add the following path:
  - `%JAVA_HOME%\bin`
6. Click OK to close all windows.

##### Step 4: Verify Java Installation

1. Open a new Command Prompt (cmd).
2. Type the following command and press Enter:

   ```cmd
   java -version
   ```

3. You should see the Java version information, indicating that Java is installed and configured correctly.

### Running Steps

#### Open the TRec Tool

1. Navigate to the `TRec` folder.
2. Double-click the file named `ConsoleApp.exe` to open TRec.
You can also open the file via a terminal or command prompt.

#### Provide Local Repository Path

1. The tool will prompt for the local repository path.
2. The project should be cloned using git bash and *not downloaded*, as downloading from Git does not include the commit history.
3. Example: `C:\Users\saiki\commons-lang`

#### Provide Commit ID

1. The tool will then ask for the Commit ID for recommendations.
2. Example: `3c13a07575642aa8ddbb6ab5c75b8da7b2f8e56b`

#### Initial Analysis (First-time Use)

1. If you are running the tool for the first time, it will perform an initial analysis.
This process may take a while to complete. 
For reference, TRec took 3 hours 22 minutes, 37 minutes, and 1 hour 48 minutes to analyze commons-lang, gson, and commons-io during the evaluation, respectively.
2. Note that TRec will create a TCT (test-to-code traceability) link database folder under the `TRec` folder.
The database size depends on the number of TCT links extracted by the tool.
Please ensure that the drive has sufficient storage.
We recommend at least 10GB free space since TRec uses some space for processing commits.
3. After the initial analysis, the tool will provide the recommendations.
4. You can close the console application by pressing "C".

#### Subsequent Runs

1. You have to start the `ConsoleApp.exe` each time you need a recommendation for a commit.
However, for subsequent uses, the tool will skip the initial analysis.
2. The same inputs will be required, and recommendations will be provided instantly.


#### Notes
- Ensure the provided repository path and commit ID are correct.
- The initial analysis may take some time, but it is only required during the first run.


### Skipping the Initial Analysis
If you prefer to skip the initial analysis for the projects on which TRec is evaluated on, you can use the already extracted TCT link databases as follows:
1. Extract the `DataBase.rar` file located in the `TRec` folder.
2. Copy/Replace the existing database folder in the `TRec` folder with the extracted database folder.
3. Ensure that you have the following directory structure: `TRec\DataBase\ParsedData` and `TRec\DataBase\ParsedData\RepoWareHouse`.
You should have a database file for each evaluated project in the `TRec\DataBase\ParsedData` folder.  



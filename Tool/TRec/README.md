
# TRec - Test Recommender Tool

## Prerequisites

### .NET Framework
- Ensure that .NET Framework version 4.7.2 or higher is installed on your system.
- You can download and install the .NET Framework from the official Microsoft website: [Download .NET Framework](https://dotnet.microsoft.com/download/dotnet-framework)

### Java
- Ensure that Java version 17.0.7 or higher is installed on your system.
- You can download and install the latest version of Java from the official Oracle website: [Download Java](https://www.oracle.com/java/technologies/javase-jdk17-downloads.html)

### Checking Java Installation

To verify if Java is installed and to check its version, follow these steps:

1. Open Command Prompt (cmd).
2. Type the following command and press Enter:

   ```cmd
   java -version
   ```

3. If Java is installed correctly, you should see an output similar to this:

   ```
   java version "17.0.7" 2023-04-18 LTS
   Java(TM) SE Runtime Environment (build 17.0.7+8-LTS-211)
   Java HotSpot(TM) 64-Bit Server VM (build 17.0.7+8-LTS-211, mixed mode, sharing)
   ```

4. If you do not see the version information and instead get an error message, follow the steps below to install or configure Java.

### Installing or Configuring Java

#### Step 1: Download Java

1. Go to the [Java SE Downloads](https://www.oracle.com/java/technologies/javase-jdk17-downloads.html) page.
2. Download the installer for your operating system.

#### Step 2: Install Java

1. Run the downloaded installer and follow the on-screen instructions to install Java.

#### Step 3: Set Up Java Environment Variables (Windows)

1. Open the Start menu, search for "Environment Variables," and select "Edit the system environment variables."
2. In the System Properties window, click on the "Environment Variables" button.
3. In the Environment Variables window, under System variables, click on "New" to add a new system variable.
   - Variable name: `JAVA_HOME`
   - Variable value: `C:\Program Files\Java\jdk-17.0.7` (Replace this path with the path where Java is installed on your system)
4. Find the `Path` variable in the System variables section, select it, and click on "Edit."
5. In the Edit Environment Variable window, click on "New" and add the following path:
   - `%JAVA_HOME%\bin`
6. Click OK to close all windows.

#### Step 4: Verify Java Installation

1. Open a new Command Prompt (cmd).
2. Type the following command and press Enter:

   ```cmd
   java -version
   ```

3. You should see the Java version information, indicating that Java is installed and configured correctly.

## Running Steps

### Open the TRec Tool

1. Navigate to the `TRec` folder.
2. Double-click the file named `ConsoleApp.exe` to open TRec.

### Provide Local Repository Path

1. The tool will prompt for the local repository path.
2. Example: `C:\Users\saiki\commons-lang`

### Provide Commit ID

1. The tool will then ask for the Commit ID for recommendations.
2. Example: `3c13a07575642aa8ddbb6ab5c75b8da7b2f8e56b`

### Initial Analysis (First-time Use)

1. If running the tool for the first time, it will perform an initial analysis.
2. This process may take a while to complete.
3. After the initial analysis, the tool will provide the recommendations.

### Subsequent Runs

1. For subsequent uses, the tool will skip the initial analysis.
2. The same inputs will be required, and recommendations will be provided instantly.

### Close the Application

1. To close the application, you will be prompted to press "C" to close.
2. If you wish to close it, just enter "C".

### Notes
- Ensure the provided repository path and commit ID are correct for accurate recommendations.
- The initial analysis may take some time, but it is only required during the first run.
- Thank you for using TRec!

## Using the Pre-processed Database

1. Extract the `DataBase.rar` file.
2. Replace the existing database folder in the `TRec` folder with the extracted database folder.

## Contact

For any questions or issues, please contact the project maintainers.

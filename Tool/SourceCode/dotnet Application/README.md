
# README for the TRec Folder

## Overview
The TRec folder contains the .NET application crucial for the operation of the TRec system, focusing on enhancing test recommendation processes.

## Prerequisites
- **Visual Studio 2017 or above**
- **.NET SDK 4.7 or higher**
- **Java 21.0.3 or above**


## How to Run
- **File to open:** `TestCase Management.sln`
- **Steps:**
  1. Double-click on the file `TestCase Management.sln`.
  2. This action will open the complete solution in Visual Studio.
  3. The solution contains two main projects:
     - **BusinessLogic Project:** Houses all independent files such as classes and models. This project encapsulates the core functionality and data handling needed for the application.
     - **Console Application:** Primarily contains the application logic used to display recommendations. This project is where the application's user interaction components are managed.

## NuGet Package Prerequisites
Make sure all the NuGet packages are installed correctly. To check, right-click on the project and click on 'Manage NuGet Packages'. The following NuGet packages are necessary:

- **Newtonsoft.Json**
- **LibGit2Sharp**
- **LibGit2Sharp.NativeBinaries**

## Troubleshooting
- If you face issues with the application not starting correctly, right-click on the Console Application project in Visual Studio, and select "Set as StartUp Project" to ensure it launches properly when running the solution.

## Usage
Open the solution in Visual Studio to modify, build, and run the applications. The BusinessLogic project should be used to manage and modify the underlying business rules and data models, while the Console Application is where the application logic for generating and showing recommendations is developed and tested.


## Overview
This folder contains the source code for applications that are integral to the functioning of the TRec system, designed to enhance traceability and recommendation capabilities in software development environments.

## Contents

### Java Application for Parsing
- **Location:** `Source code folder/Java Application for Parsing`
- **Description:** This application is used to parse Java files to extract method calls and method signatures. It utilizes a Java parser to accurately identify and process these elements, which are crucial for mapping relationships in code.

### .NET Application - TRec
- **Location:** `dot net application/TRec`
- **Description:** This .NET application integrates with the Java Application for Parsing to further process the extracted data. It extracts Test-to-Code Traceability (TCT) links and provides automated recommendations for testing based on changes in code. This application is a key component of the TRec system, linking changes in production code to corresponding tests.
- Open the dot net application application folder for detailed instructions.

## Usage
The Java Application for Parsing should be used to process Java source files, preparing the data for the .NET application. The .NET application (TRec) takes this parsed data to identify and recommend tests, enhancing the efficiency and accuracy of test recommendations in continuous integration environments.

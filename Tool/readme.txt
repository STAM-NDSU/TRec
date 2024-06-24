TRec - Test Recommender Tool
TRec is a console application developed in .NET. This tool helps in identifying and recommending tests for modified production code in continuous integration environments.

Running Steps
Open the TRec Tool:

Double-click the file named "console application" to open TRec.
Provide Local Repository Path:

The tool will prompt for the local repository path.
Example: C:\Users\saiki\commons-lang
Provide Commit ID:

The tool will then ask for the Commit ID for recommendations.
Example: 3c13a07575642aa8ddbb6ab5c75b8da7b2f8e56b
Initial Analysis (First-time Use):

If running the tool for the first time, it will perform an initial analysis.
This process may take a while to complete.
After the initial analysis, the tool will provide the recommendations.
Subsequent Runs:

For subsequent uses, the tool will skip the initial analysis.
The same inputs will be required, and recommendations will be provided instantly.
Close the Application:

To close the application, you will be prompted to press "C" to close.
If you wish to close it, just enter "C".
Notes:
Ensure the provided repository path and commit ID are correct for accurate recommendations.
The initial analysis may take some time, but it is only required during the first run.
Thank you for using TRec! 
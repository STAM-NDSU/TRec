
# README

## Overview

This folder contains datasets related to the evaluation of commits in various projects where methods were updated along with both production code and test code. Additionally, it includes datasets defining all the Test-to-Code Traceability (TCT) links extracted from the entire projects.

## Files and Descriptions

### AllRecommendations Files

These files contain data where evaluations have been performed on commits involving existing methods that were updated, including changes in both production and test code. They help in analyzing the effectiveness of the recommendations provided by our TRec tool.

- **CommonsLang_AllRecomendations_Cleaned.csv**: Contains the evaluation data for the Commons Lang project.
- **CommonsIO_AllRecomendations_Cleaned.csv**: Contains the evaluation data for the Commons IO project.
- **Gson_AllRecomendations_Cleaned.csv**: Contains the evaluation data for the Gson project.

#### Columns Description

- **Evaluation_Model**: Model name used during the evaluation.
- **Commit**: The commit ID at which modifications were made.
- **Datetime**: The date and time when the commit was made.
- **ProductionClass**: The class of the production method that was modified.
- **ChangedProductionMethods**: The actual methods in the production class that were changed.
- **TestClass**: The class name of the associated test methods.
- **ChangedTestMethods**: Test methods that were modified in the corresponding test class.
- **ChangedAndCalled**: Indicates methods that were changed and also call the production method.
- **CalledTestMethods**: All methods in the test class that call the production method.
- **RecommendedTests**: Tests recommended by TRec for the modified production method.
- **RecommendedIndex**: A relevance or priority index matching the recommended tests with the 'Changed and Called' values.

### Links Files

These files define all the Test-to-Code Traceability (TCT) links extracted from the entire projects. These links establish the relationship between production methods and the corresponding test methods.

- **CommonsLang_Links_Cleaned.csv**: Contains the TCT links for the Commons Lang project.
- **CommonsIO_Links_Cleaned.csv**: Contains the TCT links for the Commons IO project.
- **Gson_Links_Cleaned.csv**: Contains the TCT links for the Gson project.

#### Columns Description

- **linkage**: General identifier for the linkage type or ID.
- **linkage.sourceMethodFileName**: The filename containing the source method.
- **linkage.testMethodFileName**: The filename containing the test method.
- **linkage.SourceMethodName**: The name of the source method in the production code.
- **linkage.TestMethodName**: The name of the test method associated with the source method.
- **linkage.Sha_DateTime_All**: The commit SHA and the timestamp of the commit that introduced or affected the linkage.
- **linkage.Count**: The count of occurrences or frequency of this particular linkage across the dataset.

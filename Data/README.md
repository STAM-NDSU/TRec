## Overview

TRec is evaluated on three open-source Java projects, specifically commons-lang, gson, and commons-io.
This folder contains the evaluation results data. For each project, 
there are two files, one containing TCT (test-to-code traceability) links extracted by TRec and another containing TRec's test recommendation results.

## Files and Descriptions

### TCT Link Files

These files contain TCT links extracted from the projects. These links establish the association between production methods and the corresponding test methods.

- **CommonsLang_Links_Cleaned.csv**: Contains the TCT links for the commons-lang project.
- **CommonsIO_Links_Cleaned.csv**: Contains the TCT links for the commons-io project.
- **Gson_Links_Cleaned.csv**: Contains the TCT links for the Gson project.

Each of the above files have the following columns:

- **linkage**: General identifier for the linkage type or ID.
- **linkage.sourceMethodFileName**: The filename containing the production method.
- **linkage.testMethodFileName**: The filename containing the test method.
- **linkage.SourceMethodName**: The name of the production method.
- **linkage.TestMethodName**: The name of the test method associated with the production method.
- **linkage.Sha_DateTime_All**: The commit SHA and the timestamp of the commit containing the link.
- **linkage.Count**: The frequency of this particular link across commits.


### Recommendation Results

These files contain the evaluation dataset (commits, developers' modified methods and tests) used in RQ1 and TRec's recommendation results.

- **CommonsLang_AllRecomendations_Cleaned.csv**: Contains the recommendation data for the commons-lang project.
- **CommonsIO_AllRecomendations_Cleaned.csv**: Contains the recommendation data for the commons-io project.
- **Gson_AllRecomendations_Cleaned.csv**: Contains the recommendation data for the Gson project.

Each of the above files have the following columns:

- **Evaluation_Model**: Model name used during the evaluation.
- **Commit**: The commit ID where developers modified methods and tests.
- **Datetime**: The date and time when the commit was made.
- **ProductionClass**: The class of the modified production method.
- **ChangedProductionMethods**: The production method modified in the production class.
- **TestClass**: The class name of the associated modified test methods.
- **ChangedTestMethods**: Test methods modified in the test class.
- **ChangedAndCalled**: Modified test methods in which the production method was invoked.
- **CalledTestMethods**: All methods in the test class in which the production method was invoked.
- **RecommendedTests**: Tests recommended by TRec for the modified production method.
- **RecommendedIndex**: Ranking of the Tests extracted by comparing tests in ChangedAndCalled and RecommendedTests columns.
The column displays the ranking of all the tests recommended by TRec. 
However, we only considered the top 5 ranking for answering RQ1.  


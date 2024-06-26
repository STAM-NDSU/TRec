package org.example;

import java.util.List;
import java.util.ArrayList; // Import ArrayList

public class CandidateCommit {
    public  String CommitSha;
    public String RepoLocation; // Use 'String' instead of 'string'
    public List<String> ChangedFiles = new ArrayList<>(); // Correct initialization
}

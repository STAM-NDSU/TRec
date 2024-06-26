package org.example;

import com.github.javaparser.ast.CompilationUnit;
import com.github.javaparser.ast.body.MethodDeclaration;
import com.github.javaparser.ast.expr.MethodCallExpr;
import com.github.javaparser.ast.visitor.VoidVisitorAdapter;
import com.github.javaparser.symbolsolver.utils.SymbolSolverCollectionStrategy;
import com.github.javaparser.utils.ProjectRoot;
import com.github.javaparser.utils.SourceRoot;
import com.google.gson.Gson;


import java.io.File;
import java.io.IOException;
import java.nio.file.Path;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

public class Main {
    public static void main(String[] args) {
        // workingDir = System.getProperty("user.dir");
        //System.out.println("Current working directory : " + workingDir);
        //CandidateCommits theCandidateCommits = new CandidateCommits();

        //for (CandidateCommit theCommit  : theCandidateCommits.Commits) {
            //parseProject("C:\\Users\\saiki\\commons-lang" );
        String filePathRepo = args[0];
        String commitSha = args[1];
        List<String> changedFiles = Arrays.asList(args).subList(2, args.length);
        //parseProject(filePathRepo, commitSha, changedFiles);
        //String filePath = "C:\\Users\\saiki\\commons-lang";
       // String commitSha = "ABCD";
        //List<String> changedFiles = new ArrayList<>();
       // changedFiles.add( "ArrayUtils");  // Assume this is a single file
       // changedFiles.add( "ArrayUtilsTest");
        // Create a list from a single file name
       // List<String> changedFiles = Collections.singletonList(changedFileName);

        // Now call the method with a list
        parseProject(filePathRepo, commitSha, changedFiles);
           // parseProject(theCommit.FilePathRepo,theCommit.CommitSha,theCommit.ChangedFiles);
        //}

    }

    public static void parseProject(String filePath,String CommitSha,List<String> ChangedFiles) {

        File project = new File(filePath);
        SymbolSolverCollectionStrategy s = new SymbolSolverCollectionStrategy();
        s.getParserConfiguration().setStoreTokens(true);

        ProjectRoot projectRoot = s.collect(project.toPath());

        //System.out.println("Parsing " + projectName);
        for (SourceRoot sourceRoot : projectRoot.getSourceRoots()) {
            Path path = sourceRoot.getRoot();

            try {
                sourceRoot.tryToParse();
            } catch (IOException ioException) {
                ioException.printStackTrace();
                continue;
            }

            List<CompilationUnit> compilationUnits = sourceRoot.getCompilationUnits();

            for(String FileName :ChangedFiles) {
                for (CompilationUnit cu : compilationUnits) {
                    if (cu.getTypes().size() > 0 && cu.getType(0).getNameAsString().equals(FileName)) {
                        resolveCalls(cu,CommitSha,FileName);
                    }
                }
            }
        }

        List<Commit> abc= theCommits;
        System.out.println(new Gson().toJson(theCommits));
        //ObjectMapper mapper = new ObjectMapper();
    }
public static List<Commit> theCommits = new ArrayList<>();

    static void resolveCalls(CompilationUnit cu,String commitSha,String FileName){
        cu.accept(new VoidVisitorAdapter<Object>() {
            public void visit(MethodDeclaration dcl, Object arg) {
                super.visit(dcl, arg);
                //get test name
                //System.out.println("Main Method:" + dcl.getSignature() + " has the following resolved method calls: ");
                //String filename = FileName;
                Method theMethod = new Method();
                theMethod.MethodSignature_Raw=dcl.getDeclarationAsString();
                theMethod.MethodSignature = dcl.getSignature()+"";
theMethod.CodeSnippet=dcl.toString();
                //System.out.println("Full Method Snippet:");
                //System.out.println(dcl.toString());

                dcl.accept(new VoidVisitorAdapter<Object>() {
                    @Override
                    public void visit(MethodCallExpr expr, Object arg) {
                        super.visit(expr, arg);
                        try {
                            theMethod.MethodCalls.add(expr.resolve().getSignature().replace("java.lang.", ""));
                            //System.out.println(expr.resolve().getSignature().replace("java.lang.",""));
                        } catch (Exception ignore) {
                        }
                    }
                }, null);
                // Check if a commit with the given SHA does not exist and add it
                Commit commit = null;
                if (theCommits.stream().noneMatch(x -> x.CommitID.equals(commitSha))) {
                    commit = new Commit();
                    commit.CommitID = commitSha;
                    theCommits.add(commit);
                }

                    // Check if there is no file with an empty fileName in the first commit matching the SHA
                    Commit firstCommit = theCommits.stream()
                            .filter(x -> x.CommitID.equals(commitSha))
                            .findFirst()
                            .orElse(null);
                    if (firstCommit != null && firstCommit.theFiles.stream()
                            .noneMatch(x -> x.FileName.equals(FileName))) {
                        JavaFile file = new JavaFile();
                        file.FileName = FileName;
                        firstCommit.theFiles.add(file);
                    }


                // Add a Method to the first file with an empty fileName in the first commit matching the SHA
                Commit specificCommit = theCommits.stream()
                        .filter(x -> x.CommitID.equals(commitSha))
                        .findFirst()
                        .orElse(null);

                if (specificCommit != null) {
                    JavaFile specificFile = specificCommit.theFiles.stream()
                            .filter(x -> x.FileName.equals(FileName))
                            .findFirst()
                            .orElse(null);

                    if (specificFile != null) {
                        specificFile.MethodList.add(theMethod);
                    }
                }
                //theCommits.add(commit);
            }
        }, null);

    }

}
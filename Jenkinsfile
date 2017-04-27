pipeline {
    agent any

    stages {
        stage('Build and Package') {
            steps {
                withEnv(['VSToolsPath=./packages/MSBuild.Microsoft.VisualStudio.Web.targets.12.0.4/tools/VSToolsPath'])
				{
					bat 'powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "& ./Build.ps1 -Target Build"'
				}
            }
        }
        stage('Test') {
            steps {
                echo 'Testing..'
            }
        }
        stage('Deploy') {
            steps {
                echo 'Deploying....'
            }
        }
    }
}
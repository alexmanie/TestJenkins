pipeline {
    agent any

    stages {
        stage('Build and Package') {
         
		    
				bat 'powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "& ./Build.ps1 -Target Build -ScriptArgs \'-buildCounter='+env.BUILD_NUMBER+'\'"'
			
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
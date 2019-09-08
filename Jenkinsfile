pipeline {
    agent none
    stages {
        stage('react-web-tests-and-builds') {
            agent {
                docker { image 'node:8.10' }
            }
            steps {
                withEnv(['CI=true']) {
			sh 'npm install'
			sh 'npm run test'
			sh 'npm run build'
		}
            }
        }
        stage('unity-tests') {
            agent {
                docker { image 4f1' }
            }
            steps {
		sh 'unity -runTests -projectPath unity/ -testResults /dev/null -testPlatform StandaloneWindows -batchmode -nographics'
            }
        }
    }
}

plugins {
    id("java")
    id("antlr") // Add ANTLR plugin
}

group = "com.adyingdeath"
version = "1.0-SNAPSHOT"

repositories {
    mavenCentral()
}

dependencies {
    implementation("org.antlr:antlr4-runtime:4.13.1") // antlr runtime
    antlr("org.antlr:antlr4:4.13.1") // lexer and parser generation

    testImplementation(platform("org.junit:junit-bom:5.9.1"))
    testImplementation("org.junit.jupiter:junit-jupiter")
}

// Config Antlr Task
tasks.generateGrammarSource {
    arguments = arguments + listOf(
            "-visitor", // Generate Visitor interface
            "-no-listener", // Don't generate Listener interface (remove this line if needed)
            "-package", "com.adyingdeath.parser" // Set package name for generated code
    )
    outputDirectory = File("$buildDir/generated-src/antlr/main")
}

// Ensure ANTLR code generation is executed before compilation
tasks.compileJava {
    dependsOn(tasks.generateGrammarSource)
}

// Add generated code directory to source set
sourceSets.main {
    java.srcDir("$buildDir/generated-src/antlr/main")
}

tasks.test {
    useJUnitPlatform()
}

// Also delete generated code when cleaning
tasks.clean {
    delete.add("$buildDir/generated-src")
}
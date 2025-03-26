plugins {
    id("java")
    id("antlr") // Add ANTLR plugin
}

group = "com.adyingdeath.deco"
version = "1.0-SNAPSHOT"

repositories {
    mavenCentral()
}

dependencies {
    implementation("org.antlr:antlr4-runtime:4.13.1") // antlr runtime
    antlr("org.antlr:antlr4:4.13.1") // lexer and parser generation
    // Colorful terminal print
    // https://mvnrepository.com/artifact/org.fusesource.jansi/jansi
    implementation("org.fusesource.jansi:jansi:2.4.1")

    testImplementation(platform("org.junit:junit-bom:5.9.1"))
    testImplementation("org.junit.jupiter:junit-jupiter")
}

// Config Antlr Task
tasks.generateGrammarSource {
    arguments = arguments + listOf(
            "-visitor", // Generate Visitor interface
            "-package", "com.adyingdeath.deco.parser" // Set package name for generated code
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

// Configure jar task to create an executable jar with main class
tasks.jar {
    manifest {
        attributes(
            "Main-Class" to "com.adyingdeath.deco.Entry",
            "Implementation-Title" to project.name,
            "Implementation-Version" to project.version
        )
    }
    
    // Include dependencies in the JAR (fat jar)
    duplicatesStrategy = DuplicatesStrategy.EXCLUDE
    
    from(configurations.runtimeClasspath.get().map { if (it.isDirectory) it else zipTree(it) })
}

// Optional: create a task to make a standalone distributable jar
tasks.register<Jar>("fatJar") {
    archiveClassifier.set("fat")
    
    manifest {
        attributes(
            "Main-Class" to "com.adyingdeath.deco.Entry",
            "Implementation-Title" to project.name,
            "Implementation-Version" to project.version
        )
    }
    
    from(sourceSets.main.get().output)
    
    dependsOn(configurations.runtimeClasspath)
    from({
        configurations.runtimeClasspath.get().filter { it.name.endsWith("jar") }.map { zipTree(it) }
    })
    
    duplicatesStrategy = DuplicatesStrategy.EXCLUDE
}
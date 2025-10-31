## Description
This repository provides an implementation of the Atkin–Morain primality test for verifying the primality of odd integer inputs.
The algorithm leverages elliptic curves constructed via the Complex Multiplication (CM) method, offering improved practical efficiency over the 
Goldwasser–Kilian test, which depends on randomly generated curves <br/>and group order computations using Schoof’s algorithm.<br/>

Elliptic curve operations are performed using a mixed Jacobian–projective representation on recursively generated Weierstrass curves, ensuring efficient arithmetic and robust numerical stability.<br/>

## Requirements
- Build the [**Eduard Framework**](https://github.com/MateiIonutEduard/Eduard) from source using **.NET Framework 4.8**  
- **Windows:** Visual Studio or MSBuild supporting .NET Framework 4.8

## Usage (Windows OS, .NET Framework 4.8)

1. **Install prerequisites:**
   - [Visual Studio](https://visualstudio.microsoft.com/) with **.NET Framework 4.8 development tools**, or
   - **MSBuild** command-line tools supporting .NET Framework 4.8.

2. **Build the Eduard library:**
   - Clone the Eduard framework:<br/>
   
     ```bash
     git clone https://github.com/MateiIonutEduard/Eduard.git
     ```
     
   - Open the solution in Visual Studio or build from the command line:<br/>
   
     ```bash
     msbuild Eduard.sln /p:Configuration=Release
     ```

3. **Add reference to your project:**
   - Ensure the compiled Eduard DLLs are referenced by the EllipticCurvePrimalityProving project targeting **.NET Framework 4.8**.

4. **Build the EllipticCurvePrimalityProving project:**<br/>

   ```bash
   msbuild EllipticCurvePrimalityProving.sln /p:Configuration=Release
   ```

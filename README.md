
# Vehicle MOT Checker

## Overview
The Vehicle MOT Checker is a Blazor application that allows users to check the MOT (Ministry of Transport) status of vehicles by entering their registration numbers. The application fetches data from the government MOT API, providing essential details about the vehicle's MOT status, including expiry date and mileage.

## Table of Contents
- [Getting Started](#getting-started)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Usage](#usage)
- [Features](#features)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

## Getting Started
To get a copy of this project running on your local machine for development and testing, follow these steps:

### Prerequisites
- [.NET SDK (version 6.0 or higher)](https://dotnet.microsoft.com/download)
- An IDE or text editor of your choice (e.g., Visual Studio, Visual Studio Code)

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/simon-hirst/BupaTest.git
   ```
2. Navigate to the project directory (second cd is necessary):
   ```bash
   cd BupaTest/BupaTest
   ```
3. Restore the dependencies:
   ```bash
   dotnet restore
   ```

4. Set your MOT API key in the `appsettings.json` file:
   ```json
   {
     "MotApiKey": "YOUR_API_KEY"
   }
   ```

## Usage
To run the application, execute the following command in your terminal:
```bash
dotnet run
```
Open your web browser and navigate to `http://localhost:5000` (or the URL displayed in your terminal).

### How to Check MOT Status
1. Enter the vehicle registration number in the provided input field (note spaces are not permitted).
2. Click the "Check MOT" button.
3. The application will display the MOT details or an error message if the request fails.

## Features
- Input validation for vehicle registration numbers.
- Error handling for API requests, displaying user-friendly error messages.
- Fetches and displays vehicle details such as make, model, colour, MOT expiry date, and mileage.

## Contributing
Contributions are welcome! To contribute to the project:
1. Fork the repository.
2. Create your feature branch:
   ```bash
   git checkout -b feature/YourFeature
   ```
3. Commit your changes:
   ```bash
   git commit -m "Add some feature"
   ```
4. Push to the branch:
   ```bash
   git push origin feature/YourFeature
   ```
5. Open a pull request.

## License
This project is licensed under the [MIT License](LICENSE).

## Contact
Simon Hirst - simonhirst@pm.me
